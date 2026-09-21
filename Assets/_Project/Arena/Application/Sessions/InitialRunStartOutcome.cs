using System;
using Game.Arena.Application.ReadModels;
using Game.Arena.Domain.Aggregates.ArenaRun;
using Game.Arena.Domain.Concurrency;
using Game.Arena.Domain.Identity;

namespace Game.Arena.Application.Sessions
{
    public sealed class InitialRunStartOutcome
    {
        private InitialRunStartOutcome(
            InitialRunStartStatus status,
            ArenaRunId arenaRunId,
            PlayerId playerId,
            ArenaRunSnapshot snapshot)
        {
            Status = status;
            ArenaRunId = arenaRunId;
            PlayerId = playerId;
            Snapshot = snapshot;
        }

        public InitialRunStartStatus Status { get; }
        public ArenaRunId ArenaRunId { get; }
        public PlayerId PlayerId { get; }
        public ArenaRunSnapshot Snapshot { get; }

        public bool IsStarted => Status == InitialRunStartStatus.Started;

        public static InitialRunStartOutcome Started(
            ArenaRunId arenaRunId,
            PlayerId playerId,
            ArenaRunSnapshot snapshot)
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
                throw new ArgumentException("Initial run snapshot must have initial revision.", nameof(snapshot));
            }

            if (snapshot.Status != ArenaRunStatus.Playing)
            {
                throw new ArgumentException("Initial run snapshot must be playing.", nameof(snapshot));
            }

            return new InitialRunStartOutcome(
                InitialRunStartStatus.Started,
                arenaRunId,
                playerId,
                snapshot);
        }

        public static InitialRunStartOutcome ActiveRunAlreadyExists()
        {
            return new InitialRunStartOutcome(
                InitialRunStartStatus.ActiveRunAlreadyExists,
                ArenaRunId.None,
                PlayerId.None,
                null);
        }
    }
}
