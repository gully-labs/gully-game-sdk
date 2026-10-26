using System;
using System.Collections.Generic;
using System.Text;
using System.Threading.Tasks;
using UnityEngine;

namespace GullyLabs.GameSdk
{
    public class GullyIdException : Exception
    {
        public GullyIdException(string message) : base(message) { }
    }

    /// <summary>
    /// "Sign in with Gully": OpenID Connect authorization code flow with PKCE against Gully ID (Ory Hydra at
    /// id.gullychain.xyz). The game opens the system browser at <see cref="BeginAuthorization"/>.Url, receives the
    /// redirect (loopback listener on desktop, deep link on mobile), then calls <see cref="ExchangeCodeAsync"/>.
    /// Gully ID never gives the game the user's keys; wallet actions go through the player's passkey account.
    /// </summary>
    public sealed class GullyIdClient
    {
        public GullyIdConfig Config { get; }

        public GullyIdClient(GullyIdConfig config)
        {
            if (config == null) throw new ArgumentNullException(nameof(config));
            if (string.IsNullOrEmpty(config.ClientId)) throw new ArgumentException("ClientId is required", nameof(config));
            if (string.IsNullOrEmpty(config.RedirectUri)) throw new ArgumentException("RedirectUri is required", nameof(config));
            Config = config;
        }

        public string AuthorizeEndpoint => Config.Issuer.TrimEnd('/') + "/oauth2/auth";
        public string TokenEndpoint => Config.Issuer.TrimEnd('/') + "/oauth2/token";
        public string LogoutEndpoint => Config.Issuer.TrimEnd('/') + "/oauth2/sessions/logout";
        public string MeEndpoint => Config.Issuer.TrimEnd('/') + "/v1/me";

        public AuthorizationRequest BeginAuthorization(string redirectUri = null)
        {
            var verifier = Pkce.NewVerifier();
            var req = new AuthorizationRequest
            {
                State = Pkce.NewState(),
                Nonce = Pkce.NewState(),
                CodeVerifier = verifier,
                RedirectUri = redirectUri ?? Config.RedirectUri,
            };
            var q = new Dictionary<string, string>
            {
                ["response_type"] = "code",
                ["client_id"] = Config.ClientId,
                ["redirect_uri"] = req.RedirectUri,
                ["scope"] = string.Join(" ", Config.Scopes),
                ["state"] = req.State,
                ["nonce"] = req.Nonce,
                ["code_challenge"] = Pkce.Challenge(verifier),
                ["code_challenge_method"] = "S256",
            };
            req.Url = AuthorizeEndpoint + "?" + Query(q);
            return req;
        }

        /// <summary>Pulls `code` out of the redirect URL and checks `state`. Throws on an OAuth error response.</summary>
        public static string ParseCallback(string redirectedUrl, string expectedState)
        {
            var qi = redirectedUrl.IndexOf('?');
            var query = qi >= 0 ? redirectedUrl.Substring(qi + 1) : "";
            var hi = query.IndexOf('#');
            if (hi >= 0) query = query.Substring(0, hi);
            string code = null, state = null, error = null, desc = null;
            foreach (var pair in query.Split('&'))
            {
                var eq = pair.IndexOf('=');
                if (eq < 0) continue;
                var k = Uri.UnescapeDataString(pair.Substring(0, eq));
                var v = Uri.UnescapeDataString(pair.Substring(eq + 1).Replace('+', ' '));
                switch (k)
                {
                    case "code": code = v; break;
                    case "state": state = v; break;
                    case "error": error = v; break;
                    case "error_description": desc = v; break;
                }
            }
            if (error != null) throw new GullyIdException(error + (desc != null ? ": " + desc : ""));
            if (state != expectedState) throw new GullyIdException("state mismatch (possible CSRF); start over");
            if (string.IsNullOrEmpty(code)) throw new GullyIdException("redirect has no code");
            return code;
        }

        public async Task<GullyIdSession> ExchangeCodeAsync(string code, AuthorizationRequest request)
        {
            var form = new Dictionary<string, string>
            {
                ["grant_type"] = "authorization_code",
                ["code"] = code,
                ["redirect_uri"] = request.RedirectUri,
                ["client_id"] = Config.ClientId,
                ["code_verifier"] = request.CodeVerifier,
            };
            var session = await TokenRequestAsync(form);
            if (session.Claims == null) throw new GullyIdException("token response has no id_token");
            if (session.Claims.nonce != request.Nonce) throw new GullyIdException("nonce mismatch; start over");
            return session;
        }

        public Task<GullyIdSession> RefreshAsync(string refreshToken)
            => TokenRequestAsync(new Dictionary<string, string>
            {
                ["grant_type"] = "refresh_token",
                ["refresh_token"] = refreshToken,
                ["client_id"] = Config.ClientId,
            });

        private async Task<GullyIdSession> TokenRequestAsync(Dictionary<string, string> form)
        {
            var res = await GullyHttp.PostFormAsync(TokenEndpoint, form);
            var tr = JsonUtility.FromJson<TokenResponse>(res.Body);
            if (!res.IsSuccess || tr == null || string.IsNullOrEmpty(tr.access_token))
                throw new GullyIdException($"token endpoint {res.Status}: {(tr != null && !string.IsNullOrEmpty(tr.error) ? tr.error + " " + tr.error_description : res.Body)}");
            var session = new GullyIdSession
            {
                AccessToken = tr.access_token,
                RefreshToken = tr.refresh_token,
                IdToken = tr.id_token,
                AccessTokenExpiresAt = DateTimeOffset.UtcNow.AddSeconds(tr.expires_in > 0 ? tr.expires_in : 3600),
            };
            if (!string.IsNullOrEmpty(tr.id_token)) session.Claims = IdToken.Decode(tr.id_token);
            return session;
        }

        /// <summary>GET /v1/me with the Hydra access token (read-only). Returns the raw JSON; shape in gully-id openapi.yaml.</summary>
        public async Task<string> GetMeRawAsync(string accessToken)
        {
            var res = await GullyHttp.GetAsync(MeEndpoint, new Dictionary<string, string> { ["Authorization"] = "Bearer " + accessToken });
            if (!res.IsSuccess) throw new GullyHttpException($"/v1/me: {res.Status}", res.Status, res.Body);
            return res.Body;
        }

        /// <summary>RP-initiated logout URL; open it in the browser to end the Gully ID session too.</summary>
        public string LogoutUrl(string idTokenHint = null, string postLogoutRedirectUri = null)
        {
            var q = new Dictionary<string, string>();
            if (!string.IsNullOrEmpty(idTokenHint)) q["id_token_hint"] = idTokenHint;
            if (!string.IsNullOrEmpty(postLogoutRedirectUri)) q["post_logout_redirect_uri"] = postLogoutRedirectUri;
            return q.Count == 0 ? LogoutEndpoint : LogoutEndpoint + "?" + Query(q);
        }

        private static string Query(Dictionary<string, string> q)
        {
            var sb = new StringBuilder();
            foreach (var kv in q)
            {
                if (sb.Length > 0) sb.Append('&');
                sb.Append(Uri.EscapeDataString(kv.Key)).Append('=').Append(Uri.EscapeDataString(kv.Value));
            }
            return sb.ToString();
        }
    }
}
