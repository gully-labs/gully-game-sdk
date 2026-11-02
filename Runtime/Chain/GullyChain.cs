using System;

namespace GullyLabs.GameSdk
{
    /// <summary>
    /// Gully Chain networks and the one the SDK uses. Testnet is the default; select another once at start-up:
    /// <code>GullyChain.Use(GullyNetwork.Mainnet.With(rpc: "...", explorer: "...", ...));</code>
    /// The Testnet* constants stay for existing code; new code reads <see cref="Network"/>.
    /// </summary>
    public static class GullyChain
    {
        public const long TestnetChainId = 485590;
        public const string TestnetName = "Gully Chain Testnet";
        public const string TestnetCaip2 = "eip155:485590";
        /// <summary>Free, rate-limited public RPC. Production games use a Gully RPC key (see <see cref="GullyRpcUrl"/>).</summary>
        public const string TestnetRpc = "https://testnet-rpc.gullychain.xyz";
        public const string TestnetExplorer = "https://testnet-explorer.gullychain.xyz";
        public const string TestnetWeth = "0x26aCCCb2e98ca9e067E58668Aa3B101beF432754";
        /// <summary>x402 network id Gully Pay uses for this chain.</summary>
        public const string TestnetX402Network = "gully-chain-testnet";

        public const string TestnetArcadeAddress = "0xe573789b235314661DEdDA08a18Ca261c069c872";
        public const string TestnetArcadeAttestUrl = "https://testnet-arcade.gullychain.xyz";

        /// <summary>Gully Chain mainnet (parent Arbitrum One). Its RPC, explorer and contracts come with the launch.</summary>
        public const long MainnetChainId = 48559;
        public const string MainnetName = "Gully Chain";

        public const string GullyIdIssuer = "https://id.gullychain.xyz";
        public const string GullyPayApi = "https://gullypay.xyz/api";

        private static GullyNetwork _network;

        /// <summary>The network SDK defaults read (RPC, explorer links, arcade, x402, transaction chain id).</summary>
        public static GullyNetwork Network => _network ?? GullyNetwork.Testnet;

        /// <summary>Selects the network for the whole SDK. Call once at start-up, before creating clients.</summary>
        public static void Use(GullyNetwork network)
        {
            _network = network ?? throw new ArgumentNullException(nameof(network));
        }

        /// <summary>Gully RPC gateway URL for a project key: every Gully node is served through Gully RPC.
        /// <paramref name="chainId"/> 0 means the selected <see cref="Network"/>.</summary>
        public static string GullyRpcUrl(string projectKey, long chainId = 0)
            => $"https://rpc.gullyrpc.xyz/{projectKey}/evm/{(chainId > 0 ? chainId : Network.ChainId)}";

        public static string TxUrl(string txHash) => Network.RequireExplorer() + "/tx/" + txHash;
        public static string AddressUrl(string address) => Network.RequireExplorer() + "/address/" + address;
    }
}
