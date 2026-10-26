using System;

namespace GullyLabs.GameSdk
{
    [Serializable]
    public class TokenResponse
    {
        public string access_token;
        public string id_token;
        public string refresh_token;
        public string token_type;
        public string scope;
        public long expires_in;
        public string error;
        public string error_description;
    }

    [Serializable]
    public class GullyAccountClaim
    {
        public long chainId;
        public string address;
        public string kind;
    }

    /// <summary>Claims Gully ID puts in the id_token (scopes openid, email, gully.account).</summary>
    [Serializable]
    public class IdTokenClaims
    {
        public string iss;
        public string sub;
        public string email;
        public bool email_verified;
        public string nonce;
        public long exp;
        public long iat;
        public string[] amr;
        public GullyAccountClaim gully_account;

        public bool IsExpired(long skewSeconds = 60) => DateTimeOffset.UtcNow.ToUnixTimeSeconds() > exp + skewSeconds;
        public bool SignedInWithPasskey => amr != null && Array.IndexOf(amr, "passkey") >= 0;
    }

    public sealed class GullyIdSession
    {
        public string AccessToken;
        public string RefreshToken;
        public string IdToken;
        public IdTokenClaims Claims;
        public DateTimeOffset AccessTokenExpiresAt;
        public bool AccessTokenExpired => DateTimeOffset.UtcNow >= AccessTokenExpiresAt;
    }

    public sealed class AuthorizationRequest
    {
        public string Url;
        public string State;
        public string Nonce;
        public string CodeVerifier;
        public string RedirectUri;
    }

    public sealed class GullyIdConfig
    {
        public string Issuer = GullyChain.GullyIdIssuer;
        /// <summary>Public OIDC client registered at Gully ID (token_endpoint_auth_method none, PKCE enforced).</summary>
        public string ClientId;
        /// <summary>Registered redirect: http://127.0.0.1:{port}/callback on desktop, your custom scheme on mobile.</summary>
        public string RedirectUri;
        public string[] Scopes = { "openid", "email", "gully.account", "offline_access" };
    }
}
