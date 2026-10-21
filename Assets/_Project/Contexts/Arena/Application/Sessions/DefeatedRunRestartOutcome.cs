using System;
using Game.Arena.Application.ReadModels;
using Game.Arena.Domain.Aggregates.ArenaRun;
using Game.Arena.Domain.Concurrency;
using Game.Arena.Domain.Identity;

namespace Game.Arena.Application.Sessions
{
    public sealed class DefeatedRunRestartOutcome
    {
        private DefeatedRunRestartOutcome(
            DefeatedRunRestartStatus status,
            ArenaRunId arenaRunId,
            PlayerId playerId,
            ArenaRunSnapshot snapshot)
        {
            Status = status;
            ArenaRunId = arenaRunId;
            PlayerId = playerId;
            Snapshot = snapshot;
        }

        public static DefeatedRunRestartOutcome NoActiveRun => new(DefeatedRunRestartStatus.NoActiveRun, ArenaRunId.None, PlayerId.None, null);

        public static DefeatedRunRestartOutcome ActiveRunIsNotDefeated => new(DefeatedRunRestartStatus.ActiveRunIsNotDefeated, ArenaRunId.None, PlayerId.None, null);

        public DefeatedRunRestartStatus Status { get; }
        public ArenaRunId ArenaRunId { get; }
        public PlayerId PlayerId { get; }
        public ArenaRunSnapshot Snapshot { get; }

        public bool IsRestarted => Status == DefeatedRunRestartStatus.Restarted;

        public static DefeatedRunRestartOutcome Restarted(ArenaRunId arenaRunId, PlayerId playerId, ArenaRunSnapshot snapshot)
        {
            if (arenaRunId.IsNone)
            {
                throw new ArgumentException("ArenaRunId cannot be None.", nameof(arenaRunId));
            }

            if (playerId.IsNone)
            {
                throw new ArgumentException("PlayerId cannot be None.", nameof(playerId));
            }

            if (snapshot == null)
            {
                throw new ArgumentNullException(nameof(snapshot));
            }

            if (snapshot.ArenaRunId != arenaRunId)
            {
                throw new ArgumentException("Snapshot belongs to another ArenaRun.", nameof(snapshot));
            }

            if (snapshot.Revision != AggregateRevision.Initial)
            {
                throw new ArgumentException("Restart snapshot must have initial revision.", nameof(snapshot));
            }

            if (snapshot.Status != ArenaRunStatus.Playing)
            {
                throw new ArgumentException("Restart snapshot must be playing.", nameof(snapshot));
            }

            return new DefeatedRunRestartOutcome(
                DefeatedRunRestartStatus.Restarted,
                arenaRunId,
                playerId,
                snapshot);
        }
    }
}
