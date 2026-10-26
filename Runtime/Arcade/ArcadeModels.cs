using System;
using System.Numerics;

namespace GullyLabs.GameSdk
{
    [Serializable]
    public class ArcadeRunTicket
    {
        public string ticket;
        public string runId;        // bytes32 hex
        public string seed;         // bytes32 hex: the deterministic level seed
        public int engineVersion;
        public long expiresAt;      // unix ms
    }

    /// <summary>The replayable input log the attestation server re-simulates. For Frog Hop: tap ticks.</summary>
    [Serializable]
    public class ArcadeRunLog
    {
        public int v = 1;
        public string seed;
        public int[] taps;
    }

    /// <summary>What the attestation server signs (EIP-712 Score) and what submitScore needs.</summary>
    [Serializable]
    public class ArcadeAttestation
    {
        public long seasonId;
        public long gameId;
        public long score;
        public string runId;
        public long deadline;
        public string signature;    // 65-byte hex
        public string player;
    }

    [Serializable]
    public class ArcadeSeasonInfo
    {
        public long seasonId;
        public long gameId;
        public long start;
        public long end;
        public string signer;
        public bool cancelled;
        public bool settled;
        public int players;
        public bool signerMatches;
    }

    public sealed class ArcadeSeason
    {
        public long SeasonId;
        public BigInteger GameId;
        public long Start;
        public long End;
        public string Signer;
        public bool Settled;
        public bool Cancelled;
        public long Players;
        public bool IsOpen(long nowUnix) => !Cancelled && !Settled && nowUnix >= Start && nowUnix < End;
    }

    public sealed class ArcadeEntry
    {
        public int Rank;
        public string Player;
        public BigInteger Score;
    }
}
