using System;
using System.Text;

namespace GullyLabs.GameSdk
{
    /// <summary>RFC 4648 base64url without padding (PKCE, JWT) plus plain base64 (x402 header).</summary>
    public static class Base64Url
    {
        public static string Encode(byte[] bytes)
            => Convert.ToBase64String(bytes).TrimEnd('=').Replace('+', '-').Replace('/', '_');

        public static byte[] Decode(string s)
        {
            var t = s.Replace('-', '+').Replace('_', '/');
            switch (t.Length % 4)
            {
                case 2: t += "=="; break;
                case 3: t += "="; break;
            }
            return Convert.FromBase64String(t);
        }

        public static string DecodeToString(string s) => Encoding.UTF8.GetString(Decode(s));

        public static string EncodeStandard(string utf8) => Convert.ToBase64String(Encoding.UTF8.GetBytes(utf8));
    }
}
