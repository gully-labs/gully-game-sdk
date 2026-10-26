using UnityEditor;
using UnityEngine;

namespace GullyLabs.GameSdk.Editor
{
    public static class GullySdkMenu
    {
        [MenuItem("Gully/Game SDK/Documentation")]
        private static void Docs() => Application.OpenURL("https://github.com/gully-labs/gully-game-sdk#readme");

        [MenuItem("Gully/Game SDK/Gully Chain explorer")]
        private static void Explorer() => Application.OpenURL(GullyChain.TestnetExplorer);

        [MenuItem("Gully/Game SDK/Ping Gully Chain Testnet")]
        private static async void Ping()
        {
            try
            {
                var rpc = new JsonRpcClient();
                var chain = await rpc.ChainIdAsync();
                var block = await rpc.BlockNumberAsync();
                Debug.Log($"[Gully] {rpc.Url}: chain {chain}, block {block}");
                var arcade = new GullyArcadeClient(rpc: rpc);
                var season = await arcade.LatestSeasonIdAsync();
                Debug.Log(season > 0 ? $"[Gully] Arcade latest season for game 1: #{season}" : "[Gully] Arcade: no season yet");
            }
            catch (System.Exception e)
            {
                Debug.LogError("[Gully] ping failed: " + e.Message);
            }
        }
    }
}
