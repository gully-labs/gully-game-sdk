using System;
using System.Collections.Generic;
using System.Text;
using System.Threading.Tasks;
using UnityEngine.Networking;

namespace GullyLabs.GameSdk
{
    public sealed class HttpResponse
    {
        public long Status;
        public string Body;
        public Dictionary<string, string> Headers = new Dictionary<string, string>();
        public bool IsSuccess => Status >= 200 && Status < 300;

        public string Header(string name)
        {
            foreach (var kv in Headers)
                if (string.Equals(kv.Key, name, StringComparison.OrdinalIgnoreCase)) return kv.Value;
            return null;
        }
    }

    public class GullyHttpException : Exception
    {
        public long Status { get; }
        public string Body { get; }

        public GullyHttpException(string message, long status = 0, string body = null) : base(message)
        {
            Status = status;
            Body = body;
        }
    }

    /// <summary>
    /// Task wrapper around UnityWebRequest. Call from the main thread (UnityWebRequest requirement); continuations run
    /// on Unity's synchronization context so you can touch scene objects after await.
    /// </summary>
    public static class GullyHttp
    {
        public static int TimeoutSeconds = 20;
        public static string UserAgent = "gully-game-sdk/0.1.0 (unity)";

        public static Task<HttpResponse> GetAsync(string url, IDictionary<string, string> headers = null)
            => SendAsync("GET", url, null, null, headers);

        public static Task<HttpResponse> PostJsonAsync(string url, string json, IDictionary<string, string> headers = null)
            => SendAsync("POST", url, Encoding.UTF8.GetBytes(json), "application/json", headers);

        public static Task<HttpResponse> PostFormAsync(string url, IDictionary<string, string> form, IDictionary<string, string> headers = null)
        {
            var sb = new StringBuilder();
            foreach (var kv in form)
            {
                if (sb.Length > 0) sb.Append('&');
                sb.Append(Uri.EscapeDataString(kv.Key)).Append('=').Append(Uri.EscapeDataString(kv.Value ?? ""));
            }
            return SendAsync("POST", url, Encoding.UTF8.GetBytes(sb.ToString()), "application/x-www-form-urlencoded", headers);
        }

        private static Task<HttpResponse> SendAsync(string method, string url, byte[] body, string contentType, IDictionary<string, string> headers)
        {
            var tcs = new TaskCompletionSource<HttpResponse>();
            var req = new UnityWebRequest(url, method) { downloadHandler = new DownloadHandlerBuffer(), timeout = TimeoutSeconds };
            if (body != null)
            {
                req.uploadHandler = new UploadHandlerRaw(body);
                req.SetRequestHeader("Content-Type", contentType);
            }
            req.SetRequestHeader("Accept", "application/json");
            if (!string.IsNullOrEmpty(UserAgent)) TrySetHeader(req, "User-Agent", UserAgent);
            if (headers != null)
                foreach (var kv in headers) req.SetRequestHeader(kv.Key, kv.Value);

            var op = req.SendWebRequest();
            op.completed += _ =>
            {
                try
                {
                    if (req.result == UnityWebRequest.Result.ConnectionError || req.result == UnityWebRequest.Result.DataProcessingError)
                    {
                        tcs.SetException(new GullyHttpException($"{method} {url}: {req.error}", req.responseCode));
                        return;
                    }
                    var res = new HttpResponse
                    {
                        Status = req.responseCode,
                        Body = req.downloadHandler != null ? req.downloadHandler.text : "",
                        Headers = req.GetResponseHeaders() ?? new Dictionary<string, string>(),
                    };
                    tcs.SetResult(res);
                }
                catch (Exception e)
                {
                    tcs.TrySetException(e);
                }
                finally
                {
                    req.Dispose();
                }
            };
            return tcs.Task;
        }

        private static void TrySetHeader(UnityWebRequest req, string name, string value)
        {
            try { req.SetRequestHeader(name, value); } catch (InvalidOperationException) { /* WebGL forbids User-Agent */ }
        }

        /// <summary>Minimal JSON string escaping for hand-built request bodies.</summary>
        public static string JsonString(string s)
        {
            if (s == null) return "null";
            var sb = new StringBuilder("\"");
            foreach (var c in s)
            {
                switch (c)
                {
                    case '"': sb.Append("\\\""); break;
                    case '\\': sb.Append("\\\\"); break;
                    case '\n': sb.Append("\\n"); break;
                    case '\r': sb.Append("\\r"); break;
                    case '\t': sb.Append("\\t"); break;
                    default:
                        if (c < 0x20) sb.Append("\\u").Append(((int)c).ToString("x4"));
                        else sb.Append(c);
                        break;
                }
            }
            return sb.Append('"').ToString();
        }
    }
}
