using System.Numerics;
using System.Threading.Tasks;
using GullyLabs.GameSdk;
using UnityEngine;

namespace GullyLabs.GameSdk.Samples
{
    /// <summary>Stub sender: logs instead of broadcasting. Replace with your wallet integration.</summary>
    public sealed class LoggingSender : ITransactionSender
    {
        public string Address { get; }
        public LoggingSender(string address) { Address = address; }

        public Task<string> SendTransactionAsync(TransactionRequest tx)
        {
            Debug.Log($"[sample] would send to {tx.To} value {tx.Value} data {tx.Data}");
            return Task.FromResult("0x" + new string('0', 64)); // not a real hash
        }
    }

    public class ArcadeQuickstart : MonoBehaviour
    {
        [Tooltip("Player address the attestation is bound to (msg.sender of submitScore).")]
        public string playerAddress = "0x0000000000000000000000000000000000000001";

        private GullyArcadeClient _arcade;

        private async void Start()
        {
            _arcade = new GullyArcadeClient();
            var seasonId = await _arcade.LatestSeasonIdAsync();
            Debug.Log(seasonId == 0 ? "[sample] no season open" : $"[sample] season #{seasonId}");
            if (seasonId > 0)
            {
                var board = await _arcade.LeaderboardAsync(seasonId);
                foreach (var e in board) Debug.Log($"[sample] #{e.Rank} {e.Player} {e.Score}");
            }

            var run = await _arcade.StartRunAsync(gameId: 1);
            Debug.Log($"[sample] run {run.runId} seed {run.seed} engine v{run.engineVersion}");
            // ... play the deterministic game from run.seed, recording taps (tick numbers) ...
            await FinishRun(run, new int[0], 0);
        }

        private async Task FinishRun(ArcadeRunTicket run, int[] taps, long score)
        {
            try
            {
                var attestation = await _arcade.AttestAsync(run.ticket, new ArcadeRunLog { seed = run.seed, taps = taps }, playerAddress, score);
                Debug.Log($"[sample] attested score {attestation.score}, deadline {attestation.deadline}");
                var tx = await _arcade.SubmitScoreAsync(attestation, new LoggingSender(playerAddress));
                Debug.Log("[sample] submit tx " + tx);
            }
            catch (ArcadeException e)
            {
                Debug.LogWarning("[sample] attest refused (expected for an empty run): " + e.Message);
            }
        }
    }
}
