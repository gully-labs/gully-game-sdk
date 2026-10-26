using GullyLabs.GameSdk;
using UnityEngine;

namespace GullyLabs.GameSdk.Samples
{
    public class SignInWithGully : MonoBehaviour
    {
        public string clientId = "";
        public int loopbackPort = 53682;

        public async void SignIn()
        {
#if UNITY_STANDALONE || UNITY_EDITOR
            if (string.IsNullOrEmpty(clientId)) { Debug.LogError("[sample] set clientId (public OIDC client at id.gullychain.xyz)"); return; }
            using (var listener = new LoopbackRedirectListener(loopbackPort))
            {
                var id = new GullyIdClient(new GullyIdConfig { ClientId = clientId, RedirectUri = listener.RedirectUri });
                var req = id.BeginAuthorization();
                Application.OpenURL(req.Url);
                var redirected = await listener.WaitForCallbackAsync();
                var code = GullyIdClient.ParseCallback(redirected, req.State);
                var session = await id.ExchangeCodeAsync(code, req);
                Debug.Log($"[sample] signed in: sub={session.Claims.sub} email={session.Claims.email} passkey={session.Claims.SignedInWithPasskey} " +
                          $"account={session.Claims.gully_account?.address}");
                Debug.Log("[sample] /v1/me: " + await id.GetMeRawAsync(session.AccessToken));
            }
#else
            Debug.LogError("[sample] use DeepLinkRedirect with a custom scheme on mobile; WebGL should sign in on the hosting page");
#endif
        }
    }
}
