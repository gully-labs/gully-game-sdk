using System;
using System.IO;
using System.Linq;
using System.Text.RegularExpressions;
using NUnit.Framework;

namespace GullyLabs.GameSdk.Tests
{
    // Mainnet readiness: the SDK's chain values come from the selected GullyNetwork. Testnet is the default; mainnet
    // (48559) has nothing built in and refuses to be used until its values are passed.
    public class GullyNetworkTests
    {
        [TearDown] public void Reset() => GullyChain.Use(GullyNetwork.Testnet);

        [Test]
        public void TestnetIsTheDefaultWithEveryValue()
        {
            var n = GullyChain.Network;
            Assert.AreSame(GullyNetwork.Testnet, n);
            Assert.AreEqual(485590, n.ChainId);
            Assert.AreEqual("eip155:485590", n.Caip2);
            Assert.IsTrue(n.IsTestnet);
            Assert.AreEqual(GullyChain.TestnetRpc, n.RequireRpc());
            Assert.AreEqual(GullyChain.TestnetExplorer, n.RequireExplorer());
            Assert.AreEqual(GullyChain.TestnetWeth, n.RequireWeth());
            Assert.AreEqual(GullyChain.TestnetX402Network, n.RequireX402Network());
            Assert.AreEqual(GullyChain.TestnetArcadeAddress, n.RequireArcadeAddress());
            Assert.AreEqual(GullyChain.TestnetArcadeAttestUrl, n.RequireArcadeAttestUrl());
            Assert.AreEqual("https://testnet-explorer.gullychain.xyz/tx/0xabc", GullyChain.TxUrl("0xabc"));
            Assert.AreEqual("https://rpc.gullyrpc.xyz/p/evm/485590", GullyChain.GullyRpcUrl("p"));
        }

        [Test]
        public void MainnetHasOnlyItsIdAndNameAndRefusesEverythingElse()
        {
            var m = GullyNetwork.Mainnet;
            Assert.AreEqual(48559, m.ChainId);
            Assert.AreEqual("eip155:48559", m.Caip2);
            Assert.AreEqual("Gully Chain", m.Name);
            Assert.IsFalse(m.IsTestnet);
            Assert.IsNull(m.Rpc);
            Assert.IsNull(m.Explorer);
            Assert.IsNull(m.Weth);
            Assert.IsNull(m.X402Network);
            Assert.IsNull(m.ArcadeAddress);
            Assert.IsNull(m.ArcadeAttestUrl);
            foreach (var call in new Func<string>[] { m.RequireRpc, m.RequireExplorer, m.RequireWeth, m.RequireX402Network, m.RequireArcadeAddress, m.RequireArcadeAttestUrl })
            {
                var e = Assert.Throws<GullyNetworkNotConfiguredException>(() => call());
                StringAssert.Contains("Gully Chain (48559)", e.Message);
                StringAssert.Contains("never falls back to testnet", e.Message);
            }
        }

        [Test]
        public void SelectingMainnetMovesTheDefaultsAndNeverFallsBackToTestnet()
        {
            GullyChain.Use(GullyNetwork.Mainnet);
            Assert.AreEqual(48559, GullyChain.Network.ChainId);
            Assert.AreEqual("https://rpc.gullyrpc.xyz/p/evm/48559", GullyChain.GullyRpcUrl("p"));
            Assert.AreEqual("https://rpc.gullyrpc.xyz/p/evm/485590", GullyChain.GullyRpcUrl("p", 485590));
            Assert.Throws<GullyNetworkNotConfiguredException>(() => GullyChain.TxUrl("0xabc"));
            Assert.Throws<GullyNetworkNotConfiguredException>(() => GullyChain.AddressUrl("0x1"));
            Assert.Throws<ArgumentNullException>(() => GullyChain.Use(null));
        }

        [Test]
        public void MainnetWithLaunchValues()
        {
            var m = GullyNetwork.Mainnet.With(rpc: "https://rpc.example/", explorer: "https://explorer.example",
                arcadeAddress: "0x1111111111111111111111111111111111111111", arcadeAttestUrl: "https://arcade.example", x402Network: "gully-chain");
            GullyChain.Use(m);
            Assert.AreEqual(48559, m.ChainId);
            Assert.AreEqual("https://rpc.example", m.RequireRpc());
            Assert.AreEqual("https://explorer.example/address/0x2", GullyChain.AddressUrl("0x2"));
            Assert.AreEqual("gully-chain", m.RequireX402Network());
            Assert.Throws<GullyNetworkNotConfiguredException>(() => m.RequireWeth()); // still not set
            Assert.IsNull(GullyNetwork.Mainnet.Rpc, "With() must not change the shared Mainnet instance");
        }

        [Test]
        public void RejectsBadValues()
        {
            Assert.Throws<ArgumentException>(() => GullyNetwork.Mainnet.With(rpc: "http://rpc.example"));
            Assert.Throws<ArgumentException>(() => GullyNetwork.Mainnet.With(explorer: "not a url"));
            Assert.Throws<ArgumentException>(() => GullyNetwork.Mainnet.With(weth: "0x123"));
            Assert.Throws<ArgumentException>(() => GullyNetwork.Mainnet.With(arcadeAddress: "0x0000000000000000000000000000000000000000"));
            Assert.Throws<ArgumentOutOfRangeException>(() => new GullyNetwork(0, "x", true));
        }

        [Test]
        public void NoTestnetValueIsTypedIntoTheRuntimeOutsideTheNetworkFiles()
        {
            // bin/<config>/net8.0 -> Tools~/dotnet-tests -> package root
            var root = Path.GetFullPath(Path.Combine(TestContext.CurrentContext.TestDirectory, "..", "..", "..", "..", ".."));
            var runtime = Path.Combine(root, "Runtime");
            Assert.IsTrue(Directory.Exists(runtime), runtime);
            var skip = new[] { "GullyChain.cs", "GullyNetwork.cs" };
            var bad = Directory.GetFiles(runtime, "*.cs", SearchOption.AllDirectories)
                .Concat(Directory.GetFiles(Path.Combine(root, "Editor"), "*.cs", SearchOption.AllDirectories))
                .Where(f => !skip.Contains(Path.GetFileName(f)))
                .Where(f => Regex.IsMatch(File.ReadAllText(f), @"485590|GullyChain\.Testnet[A-Za-z0-9]+|testnet-[a-z]+\.gullychain\.xyz"))
                .Select(f => f.Substring(root.Length + 1))
                .ToArray();
            CollectionAssert.IsEmpty(bad);
        }
    }
}
