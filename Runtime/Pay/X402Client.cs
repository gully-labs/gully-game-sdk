using System;
using System.Collections.Generic;
using System.Numerics;
using System.Threading.Tasks;
using UnityEngine;

namespace GullyLabs.GameSdk
{
    public class X402Exception : Exception
    {
        public X402Exception(string message) : base(message) { }
    }

    /// <summary>
    /// Pay-per-call through Gully Pay (x402): GET the resource, read the 402 requirements, pay the chosen option on
    /// Gully Chain with the player's <see cref="ITransactionSender"/>, retry with the X-PAYMENT header. The resource
    /// server (your game backend) verifies the payment through Gully Pay's API; the game client never holds a Gully
    /// Pay API key.
    /// </summary>
    public sealed class X402Client
    {
        public const string PaymentHeader = "X-PAYMENT";
        public string PreferredNetwork = GullyChain.TestnetX402Network;

        public static X402Requirements ParseRequirements(HttpResponse res)
        {
            var header = res.Header("X-Payment");
            var json = !string.IsNullOrEmpty(header) ? header : res.Body;
            var req = JsonUtility.FromJson<X402Requirements>(json);
            if (req == null || req.accepts == null || req.accepts.Length == 0) throw new X402Exception("402 without payment requirements");
            return req;
        }

        public X402Accept Choose(X402Requirements req)
        {
            foreach (var a in req.accepts)
                if (string.Equals(a.network, PreferredNetwork, StringComparison.OrdinalIgnoreCase)) return a;
            return req.accepts[0];
        }

        /// <summary>Native transfer for ETH-like assets, ERC-20 transfer otherwise. Amount is in base units.</summary>
        public static TransactionRequest BuildPaymentTx(X402Accept a)
        {
            var amount = BigInteger.Parse(a.amount);
            if (amount.Sign <= 0) throw new X402Exception("amount must be positive");
            var chainId = a.extra != null && a.extra.chainId > 0 ? a.extra.chainId : GullyChain.TestnetChainId;
            if (a.extra != null && a.extra.native)
                return new TransactionRequest { To = a.payTo, Value = amount, Data = "0x", ChainId = chainId };
            if (a.extra == null || string.IsNullOrEmpty(a.extra.assetAddress))
                throw new X402Exception($"no asset address for {a.asset} on {a.network}; cannot build the transfer");
            return new TransactionRequest { To = a.extra.assetAddress, Value = BigInteger.Zero, Data = Erc20.TransferCalldata(a.payTo, amount), ChainId = chainId };
        }

        public static string EncodePaymentHeader(X402PaymentPayload p) => Base64Url.EncodeStandard(JsonUtility.ToJson(p));

        public async Task<X402Result> GetAsync(string url, ITransactionSender payer, IDictionary<string, string> headers = null)
        {
            var first = await GullyHttp.GetAsync(url, headers);
            var result = new X402Result { Response = first };
            if (first.Status != 402) return result;

            result.Requirements = ParseRequirements(first);
            result.Chosen = Choose(result.Requirements);
            if (payer == null) throw new X402Exception("resource requires payment and no ITransactionSender was given");

            var tx = BuildPaymentTx(result.Chosen);
            var txHash = await payer.SendTransactionAsync(tx);
            result.Payment = new X402PaymentPayload
            {
                x402Version = result.Requirements.x402Version > 0 ? result.Requirements.x402Version : 1,
                scheme = result.Chosen.scheme,
                network = result.Chosen.network,
                resource = result.Requirements.resource != null && !string.IsNullOrEmpty(result.Requirements.resource.url) ? result.Requirements.resource.url : url,
                txHash = txHash,
                amount = result.Chosen.amount,
                payerAddress = payer.Address,
            };

            var h = headers != null ? new Dictionary<string, string>(headers) : new Dictionary<string, string>();
            h[PaymentHeader] = EncodePaymentHeader(result.Payment);
            result.Response = await GullyHttp.GetAsync(url, h);
            return result;
        }
    }
}
