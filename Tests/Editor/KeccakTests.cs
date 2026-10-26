using System.Text;
using NUnit.Framework;

namespace GullyLabs.GameSdk.Tests
{
    public class KeccakTests
    {
        [Test]
        public void EmptyInput()
            => Assert.AreEqual("0xc5d2460186f7233c927e7db2dcc703c0e500b653ca82273b7bfad8045d85a470", Keccak256.HashHex(new byte[0]));

        [Test]
        public void Abc()
            => Assert.AreEqual("0x4e03657aea45a94fc7d47ba826c8d667c0d1e6e33a64a036ec44f58fa12d6c45", Keccak256.HashHex(Encoding.ASCII.GetBytes("abc")));

        [Test]
        public void LongerThanOneBlock()
        {
            // 200 bytes of 'a' crosses the 136-byte rate boundary
            var data = Encoding.ASCII.GetBytes(new string('a', 200));
            Assert.AreEqual(66, Keccak256.HashHex(data).Length);
            Assert.AreNotEqual(Keccak256.HashHex(data), Keccak256.HashHex(Encoding.ASCII.GetBytes(new string('a', 199))));
        }

        [TestCase("latestSeasonOf(uint256)", "0x0e48b19a")]
        [TestCase("leaderboard(uint256)", "0xbf368399")]
        [TestCase("season(uint256)", "0x8371b1c5")]
        [TestCase("bestScore(uint256,address)", "0x8da97647")]
        [TestCase("submitScore(uint256,uint256,(uint256,bytes32,uint256,bytes))", "0xbbe24c85")]
        [TestCase("runUsed(bytes32)", "0x84271f30")]
        [TestCase("balanceOf(address)", "0x70a08231")]
        [TestCase("transfer(address,uint256)", "0xa9059cbb")]
        [TestCase("decimals()", "0x313ce567")]
        public void Selectors(string sig, string expected) => Assert.AreEqual(expected, Abi.Selector(sig));

        [Test]
        public void ChecksumAddress()
        {
            Assert.AreEqual("0xe573789b235314661DEdDA08a18Ca261c069c872", Abi.ToChecksumAddress("0xe573789b235314661dedda08a18ca261c069c872"));
            Assert.AreEqual("0x26aCCCb2e98ca9e067E58668Aa3B101beF432754", Abi.ToChecksumAddress("0x26acccb2e98ca9e067e58668aa3b101bef432754"));
        }
    }
}
