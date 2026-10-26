using System.Numerics;
using NUnit.Framework;

namespace GullyLabs.GameSdk.Tests
{
    public class AbiTests
    {
        [Test]
        public void UintRoundTrip()
        {
            var v = BigInteger.Parse("123456789012345678901234567890");
            Assert.AreEqual(v, Abi.DecodeUint(Abi.Uint(v)));
            Assert.AreEqual(BigInteger.Zero, Abi.DecodeUint(Abi.Uint(0)));
            var max = BigInteger.Pow(2, 256) - 1;
            Assert.AreEqual(max, Abi.DecodeUint(Abi.Uint(max)));
        }

        [Test]
        public void AddressEncodesLeftPadded()
        {
            var w = Abi.Address("0xe573789b235314661DEdDA08a18Ca261c069c872");
            Assert.AreEqual(32, w.Length);
            Assert.AreEqual(0, w[0]);
            Assert.AreEqual(0xe5, w[12]);
            Assert.AreEqual("0xe573789b235314661DEdDA08a18Ca261c069c872", Abi.DecodeAddress(w));
        }

        [Test]
        public void CallBuildsSelectorPlusArgs()
        {
            var data = Abi.Call("latestSeasonOf(uint256)", Abi.Uint(1));
            Assert.AreEqual("0x0e48b19a0000000000000000000000000000000000000000000000000000000000000001", data);
        }

        [Test]
        public void HexQuantities()
        {
            Assert.AreEqual("0x0", Hex.FromBigInteger(0));
            Assert.AreEqual("0xce", Hex.FromBigInteger(206));
            Assert.AreEqual(new BigInteger(485590), Hex.ToBigInteger("0x768d6"));
        }

        [Test]
        public void DecodesStaticStructArray()
        {
            // leaderboard() with two entries: offset 0x20, length 2, (player, score) x2
            var p1 = Hex.ToHex(Abi.Address("0x0000000000000000000000000000000000000001"), false);
            var p2 = Hex.ToHex(Abi.Address("0x0000000000000000000000000000000000000002"), false);
            var data = "0x" + Hex.ToHex(Abi.Uint(0x20), false) + Hex.ToHex(Abi.Uint(2), false)
                       + p1 + Hex.ToHex(Abi.Uint(1590), false) + p2 + Hex.ToHex(Abi.Uint(600), false);
            var board = ArcadeAbi.DecodeLeaderboard(data);
            Assert.AreEqual(2, board.Count);
            Assert.AreEqual(1, board[0].Rank);
            Assert.AreEqual(new BigInteger(1590), board[0].Score);
            Assert.AreEqual("0x0000000000000000000000000000000000000002", board[1].Player);
        }

        [Test]
        public void DecodesString()
        {
            var data = "0x" + Hex.ToHex(Abi.Uint(0x20), false) + Hex.ToHex(Abi.DynamicBytes(System.Text.Encoding.UTF8.GetBytes("WETH")), false);
            Assert.AreEqual("WETH", Abi.DecodeString(data));
        }

        [Test]
        public void Erc20Format()
        {
            Assert.AreEqual("1.5", Erc20.Format(new BigInteger(1500000), 6));
            Assert.AreEqual("0.000001", Erc20.Format(BigInteger.One, 6));
            Assert.AreEqual("12", Erc20.Format(BigInteger.Parse("12000000000000000000"), 18));
        }
    }
}
