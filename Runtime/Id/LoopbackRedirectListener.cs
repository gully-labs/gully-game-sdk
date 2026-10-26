#if UNITY_STANDALONE || UNITY_EDITOR
using System;
using System.Net;
using System.Text;
using System.Threading;
using System.Threading.Tasks;

namespace GullyLabs.GameSdk
{
    /// <summary>
    /// RFC 8252 loopback redirect for desktop builds and the editor: listens on http://127.0.0.1:{port}/callback/,
    /// returns the full redirect URL once the browser lands there, and shows a tiny "return to the game" page.
    /// Register exactly this redirect URI (with the port) on the Gully ID client. Not available on WebGL/mobile.
    /// </summary>
    public sealed class LoopbackRedirectListener : IDisposable
    {
        public int Port { get; }
        public string RedirectUri => $"http://127.0.0.1:{Port}/callback";
        private readonly HttpListener _listener = new HttpListener();

        public LoopbackRedirectListener(int port = 53682)
        {
            Port = port;
            _listener.Prefixes.Add(RedirectUri + "/");
        }

        public async Task<string> WaitForCallbackAsync(CancellationToken ct = default, string pageHtml = null)
        {
            _listener.Start();
            try
            {
                using (ct.Register(() => { try { _listener.Stop(); } catch { /* closing */ } }))
                {
                    var ctx = await _listener.GetContextAsync();
                    var url = RedirectUri + ctx.Request.Url.Query;
                    var html = Encoding.UTF8.GetBytes(pageHtml ?? DefaultPage);
                    ctx.Response.ContentType = "text/html; charset=utf-8";
                    ctx.Response.ContentLength64 = html.Length;
                    await ctx.Response.OutputStream.WriteAsync(html, 0, html.Length);
                    ctx.Response.Close();
                    return url;
                }
            }
            finally
            {
                try { _listener.Stop(); } catch { /* already stopped */ }
            }
        }

        public void Dispose() => _listener.Close();

        private const string DefaultPage =
            "<!doctype html><meta charset=utf-8><title>Gully ID</title><body style=\"font-family:system-ui;background:#0B0E13;color:#F4F2EC;display:grid;place-items:center;height:100vh;margin:0\">" +
            "<div style=\"text-align:center\"><h1 style=\"color:#E8C060\">Signed in with Gully</h1><p>You can close this tab and return to the game.</p></div>";
    }
}
#endif
