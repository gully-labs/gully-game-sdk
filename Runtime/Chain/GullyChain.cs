namespace GullyLabs.GameSdk
{
    /// <summary>Gully Chain network constants. Mainnet values are added when the chain launches.</summary>
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

        public const string GullyIdIssuer = "https://id.gullychain.xyz";
        public const string GullyPayApi = "https://gullypay.xyz/api";

        /// <summary>Gully RPC gateway URL for a project key: every Gully node is served through Gully RPC.</summary>
        public static string GullyRpcUrl(string projectKey, long chainId = TestnetChainId)
            => $"https://rpc.gullyrpc.xyz/{projectKey}/evm/{chainId}";

        public static string TxUrl(string txHash) => TestnetExplorer + "/tx/" + txHash;
        public static string AddressUrl(string address) => TestnetExplorer + "/address/" + address;
    }
}
