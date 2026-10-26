using System.Collections.Generic;
using System.Numerics;
using NUnit.Framework;

namespace GullyLabs.GameSdk.Tests
{
    public class X402Tests
    {
        private const string Requirements =
            "{\"x402Version\":1,\"resource\":{\"url\":\"https://api.example.xyz/levels/7\",\"description\":\"Level 7\"}," +
            "\"accepts\":[{\"scheme\":\"exact\",\"network\":\"base\",\"asset\":\"USDC\",\"amount\":\"10000\",\"payTo\":\"0x0000000000000000000000000000000000000009\",\"maxTimeoutSeconds\":3600,\"extra\":{}}," +
            "{\"scheme\":\"exact\",\"network\":\"gully-chain-testnet\",\"asset\":\"ETH\",\"amount\":\"1000000000000000\",\"payTo\":\"0x0000000000000000000000000000000000000009\",\"maxTimeoutSeconds\":3600," +
            "\"extra\":{\"chainId\":485590,\"caip2\":\"eip155:485590\",\"native\":true,\"assetAddress\":null,\"decimals\":18}}]}";

        [Test]
        public void PrefersGullyChainAndBuildsNativeTx()
        {
            var res = new HttpResponse { Status = 402, Body = Requirements, Headers = new Dictionary<string, string>() };
            var req = X402Client.ParseRequirements(res);
            Assert.AreEqual(2, req.accepts.Length);
            var chosen = new X402Client().Choose(req);
            Assert.AreEqual("gully-chain-testnet", chosen.network);
            var tx = X402Client.BuildPaymentTx(chosen);
            Assert.AreEqual("0x0000000000000000000000000000000000000009", tx.To);
            Assert.AreEqual(BigInteger.Parse("1000000000000000"), tx.Value);
            Assert.AreEqual("0x", tx.Data);
            Assert.AreEqual(485590, tx.ChainId);
        }

        [Test]
        public void Erc20OptionNeedsAssetAddress()
        {
            var a = new X402Accept { network = "base", asset = "USDC", amount = "10000", payTo = "0x0000000000000000000000000000000000000009", extra = new X402Extra() };
            Assert.Throws<X402Exception>(() => X402Client.BuildPaymentTx(a));
            a.extra.assetAddress = "0x0000000000000000000000000000000000000077";
            var tx = X402Client.BuildPaymentTx(a);
            Assert.AreEqual("0x0000000000000000000000000000000000000077", tx.To);
            StringAssert.StartsWith("0xa9059cbb", tx.Data);
        }

        [Test]
        public void PaymentHeaderIsBase64Json()
        {
            var p = new X402PaymentPayload { scheme = "exact", network = "gully-chain-testnet", resource = "https://x", txHash = "0xabc", amount = "1", payerAddress = "0x1" };
            var decoded = System.Text.Encoding.UTF8.GetString(System.Convert.FromBase64String(X402Client.EncodePaymentHeader(p)));
            StringAssert.Contains("\"txHash\":\"0xabc\"", decoded);
            StringAssert.Contains("\"x402Version\":1", decoded);
        }
    }
}
