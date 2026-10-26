using System;
using System.Collections.Generic;
using System.Numerics;
using System.Text;

namespace GullyLabs.GameSdk
{
    /// <summary>
    /// Just enough Solidity ABI for game clients: static types, dynamic bytes/strings, selectors, word decoding.
    /// No Unity dependency.
    /// </summary>
    public static class Abi
    {
        public static byte[] SelectorBytes(string signature)
        {
            var h = Keccak256.Hash(Encoding.UTF8.GetBytes(signature));
            var s = new byte[4];
            Array.Copy(h, s, 4);
            return s;
        }

        public static string Selector(string signature) => Hex.ToHex(SelectorBytes(signature));

        public static byte[] Uint(BigInteger value) => Hex.ToBytes32(value);
        public static byte[] Uint(long value) => Hex.ToBytes32(new BigInteger(value));

        public static byte[] Address(string address)
        {
            if (!Hex.IsAddress(address)) throw new ArgumentException("not an address: " + address, nameof(address));
            var out_ = new byte[32];
            Array.Copy(Hex.FromHex(address), 0, out_, 12, 20);
            return out_;
        }

        public static byte[] Bytes32(string hex)
        {
            var b = Hex.FromHex(hex);
            if (b.Length != 32) throw new ArgumentException("bytes32 needs exactly 32 bytes", nameof(hex));
            return b;
        }

        public static byte[] Bool(bool v)
        {
            var out_ = new byte[32];
            if (v) out_[31] = 1;
            return out_;
        }

        /// <summary>Tail encoding of `bytes`: length word followed by the data padded to 32 bytes.</summary>
        public static byte[] DynamicBytes(byte[] data)
        {
            var padded = (data.Length + 31) / 32 * 32;
            var out_ = new byte[32 + padded];
            Array.Copy(Uint(data.Length), out_, 32);
            Array.Copy(data, 0, out_, 32, data.Length);
            return out_;
        }

        public static byte[] Concat(params byte[][] parts)
        {
            var n = 0;
            foreach (var p in parts) n += p.Length;
            var out_ = new byte[n];
            var o = 0;
            foreach (var p in parts)
            {
                Array.Copy(p, 0, out_, o, p.Length);
                o += p.Length;
            }
            return out_;
        }

        /// <summary>Calldata for a function whose arguments are all static (uint, address, bytes32, bool).</summary>
        public static string Call(string signature, params byte[][] staticArgs)
            => Hex.ToHex(Concat(SelectorBytes(signature), Concat(staticArgs)));

        // ---- decoding -------------------------------------------------------------------------------------

        public static byte[][] Words(string returnData)
        {
            var b = Hex.FromHex(returnData);
            if (b.Length % 32 != 0) throw new FormatException("return data is not a multiple of 32 bytes");
            var words = new byte[b.Length / 32][];
            for (var i = 0; i < words.Length; i++)
            {
                words[i] = new byte[32];
                Array.Copy(b, i * 32, words[i], 0, 32);
            }
            return words;
        }

        public static BigInteger DecodeUint(byte[] word)
        {
            var le = new byte[33]; // extra zero byte keeps it positive
            for (var i = 0; i < 32; i++) le[i] = word[31 - i];
            return new BigInteger(le);
        }

        public static string DecodeAddress(byte[] word)
        {
            var a = new byte[20];
            Array.Copy(word, 12, a, 0, 20);
            return ToChecksumAddress(Hex.ToHex(a));
        }

        public static bool DecodeBool(byte[] word) => word[31] != 0;

        /// <summary>Decodes a single returned `string` (or `bytes` as UTF-8): offset word, length word, data.</summary>
        public static string DecodeString(string returnData)
        {
            var b = Hex.FromHex(returnData);
            if (b.Length < 64) return "";
            var off = (int)DecodeUint(Slice(b, 0));
            var len = (int)DecodeUint(Slice(b, off));
            return Encoding.UTF8.GetString(b, off + 32, len);
        }

        /// <summary>Decodes `T[]` of static structs with `fields` words each, e.g. GullyArcade.leaderboard().</summary>
        public static List<byte[][]> DecodeStaticStructArray(string returnData, int fields)
        {
            var words = Words(returnData);
            var list = new List<byte[][]>();
            if (words.Length < 2) return list;
            var off = (int)DecodeUint(words[0]) / 32;
            var len = (int)DecodeUint(words[off]);
            for (var i = 0; i < len; i++)
            {
                var item = new byte[fields][];
                for (var f = 0; f < fields; f++) item[f] = words[off + 1 + i * fields + f];
                list.Add(item);
            }
            return list;
        }

        private static byte[] Slice(byte[] b, int off)
        {
            var w = new byte[32];
            Array.Copy(b, off, w, 0, 32);
            return w;
        }

        /// <summary>EIP-55 mixed-case checksum.</summary>
        public static string ToChecksumAddress(string address)
        {
            var lower = Hex.Strip(address).ToLowerInvariant();
            if (lower.Length != 40) throw new ArgumentException("not an address", nameof(address));
            var hash = Hex.ToHex(Keccak256.Hash(Encoding.ASCII.GetBytes(lower)), false);
            var sb = new StringBuilder("0x");
            for (var i = 0; i < 40; i++)
            {
                var c = lower[i];
                sb.Append(c >= 'a' && c <= 'f' && hash[i] >= '8' ? char.ToUpperInvariant(c) : c);
            }
            return sb.ToString();
        }
    }
}
