using System;

namespace GullyLabs.GameSdk
{
    /// <summary>
    /// Keccak-256 (the Ethereum hash; pad 0x01, not SHA3's 0x06). Compact Keccak-f[1600] permutation.
    /// Used for ABI selectors, EIP-55 checksums and EIP-712 digests. No Unity dependency.
    /// </summary>
    public static class Keccak256
    {
        private const int Rate = 136; // 1088 bits

        private static readonly ulong[] Rc =
        {
            0x0000000000000001UL, 0x0000000000008082UL, 0x800000000000808aUL, 0x8000000080008000UL,
            0x000000000000808bUL, 0x0000000080000001UL, 0x8000000080008081UL, 0x8000000000008009UL,
            0x000000000000008aUL, 0x0000000000000088UL, 0x0000000080008009UL, 0x000000008000000aUL,
            0x000000008000808bUL, 0x800000000000008bUL, 0x8000000000008089UL, 0x8000000000008003UL,
            0x8000000000008002UL, 0x8000000000000080UL, 0x000000000000800aUL, 0x800000008000000aUL,
            0x8000000080008081UL, 0x8000000000008080UL, 0x0000000080000001UL, 0x8000000080008008UL,
        };

        private static readonly int[] Rotc = { 1, 3, 6, 10, 15, 21, 28, 36, 45, 55, 2, 14, 27, 41, 56, 8, 25, 43, 62, 18, 39, 61, 20, 44 };
        private static readonly int[] Piln = { 10, 7, 11, 17, 18, 3, 5, 16, 8, 21, 24, 4, 15, 23, 19, 13, 12, 2, 20, 14, 22, 9, 6, 1 };

        public static byte[] Hash(byte[] data)
        {
            if (data == null) throw new ArgumentNullException(nameof(data));
            var st = new ulong[25];
            var off = 0;
            while (data.Length - off >= Rate)
            {
                Absorb(st, data, off);
                KeccakF(st);
                off += Rate;
            }

            var last = new byte[Rate];
            Array.Copy(data, off, last, 0, data.Length - off);
            last[data.Length - off] ^= 0x01;
            last[Rate - 1] ^= 0x80;
            Absorb(st, last, 0);
            KeccakF(st);

            var out_ = new byte[32];
            for (var i = 0; i < 4; i++)
                for (var j = 0; j < 8; j++)
                    out_[i * 8 + j] = (byte)(st[i] >> (8 * j));
            return out_;
        }

        public static byte[] Hash(string utf8) => Hash(System.Text.Encoding.UTF8.GetBytes(utf8));

        public static string HashHex(byte[] data) => Hex.ToHex(Hash(data));

        private static void Absorb(ulong[] st, byte[] b, int off)
        {
            for (var i = 0; i < Rate / 8; i++)
            {
                ulong w = 0;
                for (var j = 7; j >= 0; j--) w = (w << 8) | b[off + i * 8 + j];
                st[i] ^= w;
            }
        }

        private static ulong Rotl(ulong x, int n) => (x << n) | (x >> (64 - n));

        private static void KeccakF(ulong[] a)
        {
            var bc = new ulong[5];
            for (var round = 0; round < 24; round++)
            {
                // theta
                for (var i = 0; i < 5; i++) bc[i] = a[i] ^ a[i + 5] ^ a[i + 10] ^ a[i + 15] ^ a[i + 20];
                for (var i = 0; i < 5; i++)
                {
                    var t = bc[(i + 4) % 5] ^ Rotl(bc[(i + 1) % 5], 1);
                    for (var j = 0; j < 25; j += 5) a[j + i] ^= t;
                }

                // rho pi
                var tt = a[1];
                for (var i = 0; i < 24; i++)
                {
                    var j = Piln[i];
                    var tmp = a[j];
                    a[j] = Rotl(tt, Rotc[i]);
                    tt = tmp;
                }

                // chi
                for (var j = 0; j < 25; j += 5)
                {
                    for (var i = 0; i < 5; i++) bc[i] = a[j + i];
                    for (var i = 0; i < 5; i++) a[j + i] ^= ~bc[(i + 1) % 5] & bc[(i + 2) % 5];
                }

                // iota
                a[0] ^= Rc[round];
            }
        }
    }
}
