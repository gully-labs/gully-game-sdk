using System.Numerics;
using System.Threading.Tasks;

namespace GullyLabs.GameSdk
{
    /// <summary>ERC-20 reads and transfer calldata (WETH and USDC.e on Gully Chain Testnet).</summary>
    public static class Erc20
    {
        public const string BalanceOfSig = "balanceOf(address)";
        public const string TransferSig = "transfer(address,uint256)";

        public static async Task<BigInteger> BalanceOfAsync(JsonRpcClient rpc, string token, string owner)
            => Abi.DecodeUint(Abi.Words(await rpc.EthCallAsync(token, Abi.Call(BalanceOfSig, Abi.Address(owner))))[0]);

        public static async Task<int> DecimalsAsync(JsonRpcClient rpc, string token)
            => (int)Abi.DecodeUint(Abi.Words(await rpc.EthCallAsync(token, Abi.Call("decimals()")))[0]);

        public static async Task<string> SymbolAsync(JsonRpcClient rpc, string token)
            => Abi.DecodeString(await rpc.EthCallAsync(token, Abi.Call("symbol()")));

        public static string TransferCalldata(string to, BigInteger amount)
            => Abi.Call(TransferSig, Abi.Address(to), Abi.Uint(amount));

        /// <summary>Formats base units with the token's decimals ("1500000", 6 → "1.5").</summary>
        public static string Format(BigInteger amount, int decimals)
        {
            var s = amount.ToString().PadLeft(decimals + 1, '0');
            var ip = s.Substring(0, s.Length - decimals);
            var fp = s.Substring(s.Length - decimals).TrimEnd('0');
            return fp.Length == 0 ? ip : ip + "." + fp;
        }
    }
}
