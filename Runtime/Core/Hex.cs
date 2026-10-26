using System;
using System.Globalization;
using System.Numerics;
using System.Text;

namespace GullyLabs.GameSdk
{
    /// <summary>Hex helpers shared by the ABI, RPC and attestation code. No Unity dependency.</summary>
    public static class Hex
    {
        public static string ToHex(byte[] bytes, bool prefix = true)
        {
            var sb = new StringBuilder(bytes.Length * 2 + 2);
            if (prefix) sb.Append("0x");
            foreach (var b in bytes) sb.Append(b.ToString("x2"));
            return sb.ToString();
        }

        public static byte[] FromHex(string hex)
        {
            if (hex == null) throw new ArgumentNullException(nameof(hex));
            var s = Strip(hex);
            if (s.Length % 2 == 1) s = "0" + s;
            var out_ = new byte[s.Length / 2];
            for (var i = 0; i < out_.Length; i++)
                out_[i] = byte.Parse(s.Substring(i * 2, 2), NumberStyles.HexNumber);
            return out_;
        }

        public static string Strip(string hex)
        {
            var s = hex.Trim();
            return s.StartsWith("0x", StringComparison.OrdinalIgnoreCase) ? s.Substring(2) : s;
        }

        public static bool IsHex(string s)
        {
            if (string.IsNullOrEmpty(s)) return false;
            foreach (var c in Strip(s))
                if (!Uri.IsHexDigit(c)) return false;
            return true;
        }

        /// <summary>Unsigned parse of a 0x quantity ("0x0", "0xce", 64-hex words).</summary>
        public static BigInteger ToBigInteger(string hex)
        {
            var s = Strip(hex);
            if (s.Length == 0) return BigInteger.Zero;
            return BigInteger.Parse("0" + s, NumberStyles.HexNumber);
        }

        /// <summary>JSON-RPC quantity encoding: minimal hex, no leading zeros, "0x0" for zero.</summary>
        public static string FromBigInteger(BigInteger value)
        {
            if (value.Sign < 0) throw new ArgumentOutOfRangeException(nameof(value), "negative quantity");
            var s = value.ToString("x").TrimStart('0');
            return "0x" + (s.Length == 0 ? "0" : s);
        }

        public static byte[] ToBytes32(BigInteger value)
        {
            if (value.Sign < 0) throw new ArgumentOutOfRangeException(nameof(value), "negative uint256");
            var raw = value.ToByteArray(); // little-endian, two's complement
            var out_ = new byte[32];
            var n = raw.Length;
            if (n > 0 && raw[n - 1] == 0) n--; // drop sign byte
            if (n > 32) throw new ArgumentOutOfRangeException(nameof(value), "does not fit in uint256");
            for (var i = 0; i < n; i++) out_[31 - i] = raw[i];
            return out_;
        }

        public static bool IsAddress(string s)
            => s != null && s.Length == 42 && s.StartsWith("0x", StringComparison.OrdinalIgnoreCase) && IsHex(s);
    }
}
