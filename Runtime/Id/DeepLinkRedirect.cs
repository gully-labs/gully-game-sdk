using System;
using System.Threading.Tasks;
using UnityEngine;

namespace GullyLabs.GameSdk
{
    /// <summary>
    /// Mobile redirect: register a custom scheme (e.g. gullystudio-mygame://callback) as the Gully ID redirect URI and
    /// in the Android manifest / iOS URL types; Unity raises <see cref="Application.deepLinkActivated"/> with the URL.
    /// </summary>
    public static class DeepLinkRedirect
    {
        public static Task<string> WaitForCallbackAsync(string redirectUriPrefix)
        {
            var tcs = new TaskCompletionSource<string>();
            if (!string.IsNullOrEmpty(Application.absoluteURL) && Application.absoluteURL.StartsWith(redirectUriPrefix, StringComparison.Ordinal))
            {
                tcs.SetResult(Application.absoluteURL);
                return tcs.Task;
            }
            Action<string> handler = null;
            handler = url =>
            {
                if (!url.StartsWith(redirectUriPrefix, StringComparison.Ordinal)) return;
                Application.deepLinkActivated -= handler;
                tcs.TrySetResult(url);
            };
            Application.deepLinkActivated += handler;
            return tcs.Task;
        }
    }
}
