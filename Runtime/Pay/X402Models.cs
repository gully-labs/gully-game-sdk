using System;

namespace GullyLabs.GameSdk
{
    [Serializable]
    public class X402Extra
    {
        public long chainId;
        public string caip2;
        public bool native;
        public string assetAddress;
        public int decimals;
    }

    [Serializable]
    public class X402Accept
    {
        public string scheme;      // "exact"
        public string network;     // "gully-chain-testnet", "base", ...
        public string asset;       // "ETH", "WETH", "USDC"
        public string amount;      // integer base units as a string
        public string payTo;       // settlement address
        public int maxTimeoutSeconds;
        public X402Extra extra;
    }

    [Serializable]
    public class X402Resource
    {
        public string url;
        public string description;
    }

    /// <summary>The 402 body / X-Payment header Gully Pay returns for a registered resource.</summary>
    [Serializable]
    public class X402Requirements
    {
        public int x402Version;
        public X402Resource resource;
        public X402Accept[] accepts;
    }

    /// <summary>
    /// What the client sends back (base64 JSON in the X-PAYMENT header). The resource server forwards txHash,
    /// network, amount and payerAddress to Gully Pay `POST /x402/pay` with its API key and gets a receipt token.
    /// </summary>
    [Serializable]
    public class X402PaymentPayload
    {
        public int x402Version = 1;
        public string scheme;
        public string network;
        public string resource;
        public string txHash;
        public string amount;
        public string payerAddress;
    }

    public sealed class X402Result
    {
        public HttpResponse Response;
        public X402Requirements Requirements;
        public X402Accept Chosen;
        public X402PaymentPayload Payment;
        public bool Paid => Payment != null;
    }
}
