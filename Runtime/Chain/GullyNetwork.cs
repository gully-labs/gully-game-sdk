using System;

namespace GullyLabs.GameSdk
{
    /// <summary>
    /// One Gully Chain network: chain id, RPC, explorer and the contract addresses the SDK uses. Pick one at start-up
    /// with <see cref="GullyChain.Use"/>; <see cref="Testnet"/> is the default.
    /// <para><see cref="Mainnet"/> (chain 48559, parent Arbitrum One) has no RPC, explorer, contract or x402 values built
    /// in: they are published when the chain launches. Until a game passes them with <see cref="With"/>, every SDK call
    /// that needs one throws <see cref="GullyNetworkNotConfiguredException"/> instead of falling back to testnet values.</para>
    /// Pure C# (no UnityEngine), so it is tested with plain <c>dotnet test</c> (Tools~/dotnet-tests).
    /// </summary>
    public sealed class GullyNetwork
    {
        public long ChainId { get; }
        public string Name { get; }
        public bool IsTestnet { get; }
        /// <summary>Public RPC URL, or null when not configured.</summary>
        public string Rpc { get; }
        public string Explorer { get; }
        public string Weth { get; }
        /// <summary>x402 network id Gully Pay uses for this chain.</summary>
        public string X402Network { get; }
        public string ArcadeAddress { get; }
        public string ArcadeAttestUrl { get; }

        public string Caip2 => "eip155:" + ChainId;

        public GullyNetwork(long chainId, string name, bool isTestnet, string rpc = null, string explorer = null, string weth = null,
            string x402Network = null, string arcadeAddress = null, string arcadeAttestUrl = null)
        {
            if (chainId <= 0) throw new ArgumentOutOfRangeException(nameof(chainId), "chain id must be positive");
            if (string.IsNullOrWhiteSpace(name)) throw new ArgumentException("name is required", nameof(name));
            ChainId = chainId;
            Name = name;
            IsTestnet = isTestnet;
            Rpc = Url(rpc, nameof(rpc));
            Explorer = Url(explorer, nameof(explorer));
            Weth = Address(weth, nameof(weth));
            X402Network = Blank(x402Network);
            ArcadeAddress = Address(arcadeAddress, nameof(arcadeAddress));
            ArcadeAttestUrl = Url(arcadeAttestUrl, nameof(arcadeAttestUrl));
        }

        /// <summary>Gully Chain Testnet (485590): the default network, every value built in.</summary>
        public static readonly GullyNetwork Testnet = new GullyNetwork(
            GullyChain.TestnetChainId, GullyChain.TestnetName, true,
            rpc: GullyChain.TestnetRpc,
            explorer: GullyChain.TestnetExplorer,
            weth: GullyChain.TestnetWeth,
            x402Network: GullyChain.TestnetX402Network,
            arcadeAddress: GullyChain.TestnetArcadeAddress,
            arcadeAttestUrl: GullyChain.TestnetArcadeAttestUrl);

        /// <summary>Gully Chain mainnet (48559). Nothing but the id and name is built in; see <see cref="With"/>.</summary>
        public static readonly GullyNetwork Mainnet = new GullyNetwork(GullyChain.MainnetChainId, GullyChain.MainnetName, false);

        /// <summary>A copy with the given values set (null keeps the current one), e.g. the mainnet values on launch day.</summary>
        public GullyNetwork With(string rpc = null, string explorer = null, string weth = null, string x402Network = null,
            string arcadeAddress = null, string arcadeAttestUrl = null)
            => new GullyNetwork(ChainId, Name, IsTestnet, rpc ?? Rpc, explorer ?? Explorer, weth ?? Weth, x402Network ?? X402Network,
                arcadeAddress ?? ArcadeAddress, arcadeAttestUrl ?? ArcadeAttestUrl);

        public string RequireRpc() => Require(Rpc, "an RPC URL");
        public string RequireExplorer() => Require(Explorer, "an explorer URL");
        public string RequireWeth() => Require(Weth, "the WETH address");
        public string RequireX402Network() => Require(X402Network, "an x402 network id");
        public string RequireArcadeAddress() => Require(ArcadeAddress, "the GullyArcade address");
        public string RequireArcadeAttestUrl() => Require(ArcadeAttestUrl, "the arcade attestation URL");

        public override string ToString() => $"{Name} ({ChainId})";

        private string Require(string value, string what)
        {
            if (string.IsNullOrEmpty(value)) throw new GullyNetworkNotConfiguredException(this, what);
            return value;
        }

        private static string Blank(string v) => string.IsNullOrWhiteSpace(v) ? null : v.Trim();

        private static string Url(string v, string what)
        {
            v = Blank(v);
            if (v == null) return null;
            if (!Uri.TryCreate(v, UriKind.Absolute, out var u) || u.Scheme != Uri.UriSchemeHttps)
                throw new ArgumentException($"{what} must be an https URL", what);
            return v.TrimEnd('/');
        }

        private static string Address(string v, string what)
        {
            v = Blank(v);
            if (v == null) return null;
            if (v.Length != 42 || !v.StartsWith("0x", StringComparison.Ordinal)) throw new ArgumentException($"{what} must be a 0x address", what);
            for (var i = 2; i < v.Length; i++)
                if (!Uri.IsHexDigit(v[i])) throw new ArgumentException($"{what} must be a 0x address", what);
            if (v == "0x0000000000000000000000000000000000000000") throw new ArgumentException($"{what} must not be the zero address", what);
            return v;
        }
    }

    /// <summary>The selected network lacks a value an SDK call needs (mainnet before its values are set).</summary>
    public sealed class GullyNetworkNotConfiguredException : InvalidOperationException
    {
        public GullyNetwork Network { get; }

        public GullyNetworkNotConfiguredException(GullyNetwork network, string what)
            : base($"{network} has no {what} configured. Pass it with GullyNetwork.With(...) and GullyChain.Use(...); the SDK never falls back to testnet values.")
        {
            Network = network;
        }
    }
}
