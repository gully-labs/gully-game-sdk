using System.Numerics;
using System.Threading.Tasks;

namespace GullyLabs.GameSdk
{
    public sealed class TransactionRequest
    {
        public string To;
        public string Data = "0x";
        public BigInteger Value = BigInteger.Zero;
        public long ChainId = GullyChain.TestnetChainId;
    }

    /// <summary>
    /// The one seam between the SDK and a wallet. The SDK never holds keys. Implement this with the player's Gully ID
    /// passkey smart account (ZeroDev Kernel via session keys, sponsored gas), an injected wallet, or a server-side
    /// signer for dedicated-server games. Return the transaction hash once the transaction is broadcast.
    /// </summary>
    public interface ITransactionSender
    {
        string Address { get; }
        Task<string> SendTransactionAsync(TransactionRequest tx);
    }
}
