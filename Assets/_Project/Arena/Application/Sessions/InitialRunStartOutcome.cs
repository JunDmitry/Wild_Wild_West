using System;
using Game.Arena.Domain.Identity;

namespace Game.Arena.Application.Sessions
{
    public sealed class InitialRunStartOutcome
    {
        private InitialRunStartOutcome(
            InitialRunStartStatus status,
            ArenaRunId arenaRunId,
            PlayerId playerId)
        {
            Status = status;
            ArenaRunId = arenaRunId;
            PlayerId = playerId;
        }

        public InitialRunStartStatus Status { get; }

        public ArenaRunId ArenaRunId { get; }

        public PlayerId PlayerId { get; }

        public bool IsStarted => Status == InitialRunStartStatus.Started;

        public static InitialRunStartOutcome Started(
            ArenaRunId arenaRunId,
            PlayerId playerId)
        {
            if (arenaRunId.IsNone)
            {
                throw new ArgumentException("ArenaRunId cannot be None.", nameof(arenaRunId));
            }

            if (playerId.IsNone)
            {
                throw new ArgumentException("PlayerId cannot be None.", nameof(playerId));
            }

            return new InitialRunStartOutcome(
                InitialRunStartStatus.Started,
                arenaRunId,
                playerId);
        }

        public static InitialRunStartOutcome ActiveRunAlreadyExists()
        {
            return new InitialRunStartOutcome(
                InitialRunStartStatus.ActiveRunAlreadyExists,
                ArenaRunId.None,
                PlayerId.None);
        }
    }
}
