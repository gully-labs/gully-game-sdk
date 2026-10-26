using System;
using System.Collections.Generic;
using System.Numerics;
using System.Threading.Tasks;
using UnityEngine;

namespace GullyLabs.GameSdk
{
    public class ArcadeException : Exception
    {
        public long Status { get; }
        public ArcadeException(string message, long status = 0) : base(message) { Status = status; }
    }

    /// <summary>
    /// Gully Arcade: free-to-play games with on-chain leaderboards on Gully Chain. Flow per run:
    /// 1. <see cref="StartRunAsync"/> gets a server-chosen seed + ticket (the player cannot pick the level).
    /// 2. Play deterministically from that seed, recording the input log.
    /// 3. <see cref="AttestAsync"/> replays the log server side and returns an EIP-712 Score signature.
    /// 4. <see cref="SubmitScoreAsync"/> sends submitScore(...) from the player's account (gas sponsored for Gully
    ///    passkey accounts). Only the best score counts; each run can be submitted once.
    /// Reads (season, leaderboard, best score) go straight to the contract over JSON-RPC.
    /// </summary>
    public sealed class GullyArcadeClient
    {
        [Serializable] private class ApiError { public string error; }
        [Serializable] private class AttestRequest { public string ticket; public ArcadeRunLog log; public string player; public long score; }

        public string AttestUrl { get; }
        public string ArcadeAddress { get; }
        public JsonRpcClient Rpc { get; }

        public GullyArcadeClient(string attestUrl = null, JsonRpcClient rpc = null, string arcadeAddress = null)
        {
            AttestUrl = (attestUrl ?? GullyChain.TestnetArcadeAttestUrl).TrimEnd('/');
            Rpc = rpc ?? new JsonRpcClient();
            ArcadeAddress = arcadeAddress ?? GullyChain.TestnetArcadeAddress;
        }

        // ---- attestation server ---------------------------------------------------------------------------

        public async Task<ArcadeRunTicket> StartRunAsync(int gameId = 1)
        {
            var res = await GullyHttp.PostJsonAsync(AttestUrl + "/v1/run", "{\"gameId\":" + gameId + "}");
            Check(res, "start run");
            return JsonUtility.FromJson<ArcadeRunTicket>(res.Body);
        }

        public async Task<ArcadeAttestation> AttestAsync(string ticket, ArcadeRunLog log, string playerAddress, long claimedScore)
        {
            if (!Hex.IsAddress(playerAddress)) throw new ArgumentException("player must be a 0x address", nameof(playerAddress));
            var body = JsonUtility.ToJson(new AttestRequest { ticket = ticket, log = log, player = playerAddress, score = claimedScore });
            var res = await GullyHttp.PostJsonAsync(AttestUrl + "/v1/attest", body);
            Check(res, "attest");
            return JsonUtility.FromJson<ArcadeAttestation>(res.Body);
        }

        /// <summary>The game's latest season as the attestation server sees it (null when none exists).</summary>
        public async Task<ArcadeSeasonInfo> ServerSeasonAsync()
        {
            var res = await GullyHttp.GetAsync(AttestUrl + "/v1/season");
            Check(res, "season");
            if (string.IsNullOrEmpty(res.Body) || res.Body.Trim() == "null") return null;
            return JsonUtility.FromJson<ArcadeSeasonInfo>(res.Body);
        }

        private static void Check(HttpResponse res, string what)
        {
            if (res.IsSuccess) return;
            string msg = null;
            try { msg = JsonUtility.FromJson<ApiError>(res.Body)?.error; } catch { /* not json */ }
            throw new ArcadeException($"arcade {what} failed ({res.Status}): {msg ?? res.Body}", res.Status);
        }

        // ---- chain -----------------------------------------------------------------------------------------

        public async Task<long> LatestSeasonIdAsync(long gameId = 1)
            => (long)Abi.DecodeUint(Abi.Words(await Rpc.EthCallAsync(ArcadeAddress, Abi.Call(ArcadeAbi.LatestSeasonOfSig, Abi.Uint(gameId))))[0]);

        public async Task<ArcadeSeason> ReadSeasonAsync(long seasonId)
            => ArcadeAbi.DecodeSeason(seasonId, await Rpc.EthCallAsync(ArcadeAddress, Abi.Call(ArcadeAbi.SeasonSig, Abi.Uint(seasonId))));

        public async Task<List<ArcadeEntry>> LeaderboardAsync(long seasonId)
            => ArcadeAbi.DecodeLeaderboard(await Rpc.EthCallAsync(ArcadeAddress, Abi.Call(ArcadeAbi.LeaderboardSig, Abi.Uint(seasonId))));

        public async Task<BigInteger> BestScoreAsync(long seasonId, string player)
            => Abi.DecodeUint(Abi.Words(await Rpc.EthCallAsync(ArcadeAddress, Abi.Call(ArcadeAbi.BestScoreSig, Abi.Uint(seasonId), Abi.Address(player))))[0]);

        public async Task<bool> RunUsedAsync(string runId)
            => Abi.DecodeBool(Abi.Words(await Rpc.EthCallAsync(ArcadeAddress, Abi.Call(ArcadeAbi.RunUsedSig, Abi.Bytes32(runId))))[0]);

        /// <summary>Builds and sends submitScore from the player's account. The sender's address must equal attestation.player.</summary>
        public Task<string> SubmitScoreAsync(ArcadeAttestation attestation, ITransactionSender sender)
        {
            if (sender == null) throw new ArgumentNullException(nameof(sender));
            if (!string.Equals(sender.Address, attestation.player, StringComparison.OrdinalIgnoreCase))
                throw new ArcadeException("the attestation was issued for another player address");
            if (DateTimeOffset.UtcNow.ToUnixTimeSeconds() >= attestation.deadline)
                throw new ArcadeException("attestation expired; attest the run again");
            return sender.SendTransactionAsync(new TransactionRequest
            {
                To = ArcadeAddress,
                Data = ArcadeAbi.EncodeSubmitScore(attestation),
                Value = BigInteger.Zero,
                ChainId = GullyChain.TestnetChainId,
            });
        }
    }
}
