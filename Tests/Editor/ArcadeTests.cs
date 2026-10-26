using System.Numerics;
using NUnit.Framework;

namespace GullyLabs.GameSdk.Tests
{
    public class ArcadeTests
    {
        [Test]
        public void SubmitScoreCalldataLayout()
        {
            var sig = new byte[65];
            for (var i = 0; i < sig.Length; i++) sig[i] = (byte)i;
            var runId = "0x" + new string('a', 64);
            var data = ArcadeAbi.EncodeSubmitScore(1, 1590, 7, runId, 1793558924, sig);
            var bytes = Hex.FromHex(data);
            Assert.AreEqual(4 + 32 * 11, bytes.Length); // selector + 3 head + 4 tuple head + len + 3 words of sig
            Assert.AreEqual("0xbbe24c85", data.Substring(0, 10));
            var words = Abi.Words("0x" + data.Substring(10));
            Assert.AreEqual(new BigInteger(1), Abi.DecodeUint(words[0]));      // gameId
            Assert.AreEqual(new BigInteger(1590), Abi.DecodeUint(words[1]));   // score
            Assert.AreEqual(new BigInteger(0x60), Abi.DecodeUint(words[2]));   // offset to Proof
            Assert.AreEqual(new BigInteger(7), Abi.DecodeUint(words[3]));      // seasonId
            Assert.AreEqual(runId, Hex.ToHex(words[4]));                        // runId
            Assert.AreEqual(new BigInteger(1793558924), Abi.DecodeUint(words[5])); // deadline
            Assert.AreEqual(new BigInteger(0x80), Abi.DecodeUint(words[6]));   // offset to bytes inside Proof
            Assert.AreEqual(new BigInteger(65), Abi.DecodeUint(words[7]));     // signature length
            Assert.AreEqual(0x40, words[10][0]);                                // byte 64 of the signature starts the last word
        }

        [Test]
        public void DecodesSeason()
        {
            var data = "0x" + Hex.ToHex(Abi.Uint(1), false) + Hex.ToHex(Abi.Uint(1792954124), false) + Hex.ToHex(Abi.Uint(1793558924), false)
                       + Hex.ToHex(Abi.Address("0xa35FC76AF64A09D953675025E5BD3549B189FFa1"), false)
                       + Hex.ToHex(Abi.Bool(false), false) + Hex.ToHex(Abi.Bool(false), false) + Hex.ToHex(Abi.Uint(3), false);
            var s = ArcadeAbi.DecodeSeason(1, data);
            Assert.AreEqual(1, s.SeasonId);
            Assert.AreEqual(new BigInteger(1), s.GameId);
            Assert.AreEqual(1792954124, s.Start);
            Assert.AreEqual("0xa35FC76AF64A09D953675025E5BD3549B189FFa1", s.Signer);
            Assert.IsFalse(s.Settled);
            Assert.AreEqual(3, s.Players);
            Assert.IsTrue(s.IsOpen(1793000000));
            Assert.IsFalse(s.IsOpen(1793558924));
        }

        [Test]
        public void RunLogSerializesForTheAttestServer()
        {
            var log = new ArcadeRunLog { seed = "0x00", taps = new[] { 0, 18, 54 } };
            var json = UnityEngine.JsonUtility.ToJson(log);
            Assert.AreEqual("{\"v\":1,\"seed\":\"0x00\",\"taps\":[0,18,54]}", json);
        }
    }
}
