using System.Security.Cryptography;
using System.Text;

namespace GullyLabs.GameSdk
{
    /// <summary>RFC 7636 helpers: code verifier, S256 challenge, random state/nonce.</summary>
    public static class Pkce
    {
        public static string NewVerifier() => Base64Url.Encode(RandomBytes(32)); // 43 chars

        public static string Challenge(string verifier)
        {
            using (var sha = SHA256.Create())
                return Base64Url.Encode(sha.ComputeHash(Encoding.ASCII.GetBytes(verifier)));
        }

        public static string NewState() => Base64Url.Encode(RandomBytes(16));

        public static byte[] RandomBytes(int n)
        {
            var b = new byte[n];
            using (var rng = RandomNumberGenerator.Create()) rng.GetBytes(b);
            return b;
        }
    }
}
