using System.Text;
using NUnit.Framework;

namespace GullyLabs.GameSdk.Tests
{
    public class PkceAndIdTests
    {
        [Test]
        public void Rfc7636AppendixB()
            => Assert.AreEqual("E9Melhoa2OwvFrEMTJguCHaoeK1t8URWbuGJSstw-cM", Pkce.Challenge("dBjftJeZ4CVP-mB92K27uhbUJU1p1r_wW1gFWFOEjXk"));

        [Test]
        public void VerifierShape()
        {
            var v = Pkce.NewVerifier();
            Assert.AreEqual(43, v.Length);
            Assert.IsFalse(v.Contains("="));
            Assert.AreNotEqual(v, Pkce.NewVerifier());
        }

        [Test]
        public void Base64UrlRoundTrip()
        {
            var b = new byte[] { 0, 1, 2, 250, 251, 252, 253, 254, 255 };
            Assert.AreEqual(b, Base64Url.Decode(Base64Url.Encode(b)));
        }

        [Test]
        public void AuthorizeUrlHasPkceAndState()
        {
            var id = new GullyIdClient(new GullyIdConfig { ClientId = "gully-studio-demo", RedirectUri = "http://127.0.0.1:53682/callback" });
            var req = id.BeginAuthorization();
            StringAssert.StartsWith("https://id.gullychain.xyz/oauth2/auth?", req.Url);
            StringAssert.Contains("code_challenge_method=S256", req.Url);
            StringAssert.Contains("code_challenge=" + Pkce.Challenge(req.CodeVerifier), req.Url);
            StringAssert.Contains("state=" + req.State, req.Url);
            StringAssert.Contains("scope=openid%20email%20gully.account%20offline_access", req.Url);
        }

        [Test]
        public void ParseCallbackChecksState()
        {
            Assert.AreEqual("abc", GullyIdClient.ParseCallback("http://127.0.0.1:53682/callback?code=abc&state=s1", "s1"));
            Assert.Throws<GullyIdException>(() => GullyIdClient.ParseCallback("http://127.0.0.1:53682/callback?code=abc&state=other", "s1"));
            Assert.Throws<GullyIdException>(() => GullyIdClient.ParseCallback("http://127.0.0.1:53682/callback?error=access_denied&state=s1", "s1"));
        }

        [Test]
        public void DecodesIdTokenClaims()
        {
            const string payload = "{\"iss\":\"https://id.gullychain.xyz\",\"sub\":\"u_123\",\"email\":\"frog@gullylabs.xyz\",\"email_verified\":true," +
                                   "\"nonce\":\"n1\",\"exp\":9999999999,\"iat\":1,\"amr\":[\"passkey\"]," +
                                   "\"gully_account\":{\"chainId\":485590,\"address\":\"0x0000000000000000000000000000000000000001\",\"kind\":\"kernel\"}}";
            var jwt = Base64Url.Encode(Encoding.UTF8.GetBytes("{\"alg\":\"RS256\"}")) + "." + Base64Url.Encode(Encoding.UTF8.GetBytes(payload)) + ".sig";
            var c = IdToken.Decode(jwt);
            Assert.AreEqual("u_123", c.sub);
            Assert.AreEqual("n1", c.nonce);
            Assert.IsTrue(c.email_verified);
            Assert.IsTrue(c.SignedInWithPasskey);
            Assert.IsFalse(c.IsExpired());
            Assert.AreEqual(485590, c.gully_account.chainId);
        }
    }
}
