using GullyLabs.GameSdk;
using UnityEngine;

namespace GullyLabs.GameSdk.Samples
{
    public class PayPerCall : MonoBehaviour
    {
        private void Awake() => GullyStudioTheme.ApplyTo(Camera.main); // sample scenes use the GullyStudio brand defaults

        public string resourceUrl = "https://example.gullystudio.xyz/paid/level-7";
        public string playerAddress = "0x0000000000000000000000000000000000000001";

        public async void Fetch()
        {
            var x402 = new X402Client();
            var result = await x402.GetAsync(resourceUrl, new LoggingSender(playerAddress));
            if (result.Paid)
                Debug.Log($"[sample] paid {result.Chosen.amount} {result.Chosen.asset} on {result.Chosen.network}, tx {result.Payment.txHash}; status {result.Response.Status}");
            else
                Debug.Log($"[sample] free resource, status {result.Response.Status}");
        }
    }
}
