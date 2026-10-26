using System.Numerics;

namespace GullyLabs.GameSdk
{
    /// <summary>GullyArcade v1.0 ABI pieces (gully-ops/l3/arcade/src/GullyArcade.sol). No Unity dependency.</summary>
    public static class ArcadeAbi
    {
        public const string LatestSeasonOfSig = "latestSeasonOf(uint256)";
        public const string SeasonSig = "season(uint256)";
        public const string LeaderboardSig = "leaderboard(uint256)";
        public const string BestScoreSig = "bestScore(uint256,address)";
        public const string RunUsedSig = "runUsed(bytes32)";
        public const string SubmitScoreSig = "submitScore(uint256,uint256,(uint256,bytes32,uint256,bytes))";
        public const string ScoreTypeHashSource = "Score(address player,uint256 seasonId,uint256 gameId,uint256 score,bytes32 runId,uint256 deadline)";

        /// <summary>
        /// Calldata for submitScore(gameId, score, Proof{seasonId, runId, deadline, signature}). Proof is a dynamic
        /// tuple: head = gameId, score, offset(0x60); tuple = seasonId, runId, deadline, offset(0x80), bytes.
        /// </summary>
        public static string EncodeSubmitScore(BigInteger gameId, BigInteger score, BigInteger seasonId, string runId, BigInteger deadline, byte[] signature)
        {
            var head = Abi.Concat(Abi.Uint(gameId), Abi.Uint(score), Abi.Uint(0x60));
            var proof = Abi.Concat(Abi.Uint(seasonId), Abi.Bytes32(runId), Abi.Uint(deadline), Abi.Uint(0x80), Abi.DynamicBytes(signature));
            return Hex.ToHex(Abi.Concat(Abi.SelectorBytes(SubmitScoreSig), head, proof));
        }

        public static string EncodeSubmitScore(ArcadeAttestation a)
            => EncodeSubmitScore(a.gameId, a.score, a.seasonId, a.runId, a.deadline, Hex.FromHex(a.signature));

        public static ArcadeSeason DecodeSeason(long seasonId, string returnData)
        {
            var w = Abi.Words(returnData);
            return new ArcadeSeason
            {
                SeasonId = seasonId,
                GameId = Abi.DecodeUint(w[0]),
                Start = (long)Abi.DecodeUint(w[1]),
                End = (long)Abi.DecodeUint(w[2]),
                Signer = Abi.DecodeAddress(w[3]),
                Settled = Abi.DecodeBool(w[4]),
                Cancelled = Abi.DecodeBool(w[5]),
                Players = (long)Abi.DecodeUint(w[6]),
            };
        }

        public static System.Collections.Generic.List<ArcadeEntry> DecodeLeaderboard(string returnData)
        {
            var rows = Abi.DecodeStaticStructArray(returnData, 2);
            var list = new System.Collections.Generic.List<ArcadeEntry>(rows.Count);
            for (var i = 0; i < rows.Count; i++)
                list.Add(new ArcadeEntry { Rank = i + 1, Player = Abi.DecodeAddress(rows[i][0]), Score = Abi.DecodeUint(rows[i][1]) });
            return list;
        }
    }
}
