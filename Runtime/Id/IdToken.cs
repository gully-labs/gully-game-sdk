using System;
using UnityEngine;

namespace GullyLabs.GameSdk
{
    /// <summary>
    /// Decodes id_token claims. The signature is NOT verified here: a game client has no secret to protect and the
    /// server it talks to must verify the token against https://id.gullychain.xyz/.well-known/jwks.json before
    /// trusting `sub`. Use the claims client-side for display and routing only.
    /// </summary>
    public static class IdToken
    {
        public static IdTokenClaims Decode(string jwt)
        {
            if (string.IsNullOrEmpty(jwt)) throw new ArgumentException("empty token", nameof(jwt));
            var parts = jwt.Split('.');
            if (parts.Length != 3) throw new FormatException("not a JWT");
            var json = Base64Url.DecodeToString(parts[1]);
            var claims = JsonUtility.FromJson<IdTokenClaims>(json);
            if (claims == null || string.IsNullOrEmpty(claims.sub)) throw new FormatException("id_token has no sub");
            return claims;
        }

        public static string PayloadJson(string jwt) => Base64Url.DecodeToString(jwt.Split('.')[1]);
    }
}
