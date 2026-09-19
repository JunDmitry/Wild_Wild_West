using System;
using Game.Arena.Domain.Identity;

namespace Game.Arena.Application.Sessions
{
    public sealed class DefeatedRunRestartOutcome
    {
        private DefeatedRunRestartOutcome(
            DefeatedRunRestartStatus status,
            ArenaRunId arenaRunId,
            PlayerId playerId)
        {
            Status = status;
            ArenaRunId = arenaRunId;
            PlayerId = playerId;
        }

        public static DefeatedRunRestartOutcome NoActiveRun => new(DefeatedRunRestartStatus.NoActiveRun, ArenaRunId.None, PlayerId.None);

        public static DefeatedRunRestartOutcome ActiveRunIsNotDefeated => new(DefeatedRunRestartStatus.ActiveRunIsNotDefeated, ArenaRunId.None, PlayerId.None);

        public DefeatedRunRestartStatus Status { get; }

        public ArenaRunId ArenaRunId { get; }

        public PlayerId PlayerId { get; }

        public bool IsRestarted => Status == DefeatedRunRestartStatus.Restarted;

        public static DefeatedRunRestartOutcome Restarted(ArenaRunId arenaRunId, PlayerId playerId)
        {
            if (arenaRunId.IsNone)
            {
                throw new ArgumentException("ArenaRunId cannot be None.", nameof(arenaRunId));
            }

            if (playerId.IsNone)
            {
                throw new ArgumentException("PlayerId cannot be None.", nameof(playerId));
            }

            return new DefeatedRunRestartOutcome(
                DefeatedRunRestartStatus.Restarted,
                arenaRunId,
                playerId);
        }
    }
}
