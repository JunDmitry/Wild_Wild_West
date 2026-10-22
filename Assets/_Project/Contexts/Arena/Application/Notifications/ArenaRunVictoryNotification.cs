using System;
using Game.Arena.Domain.Concurrency;
using Game.Arena.Domain.Identity;

namespace Game.Arena.Application.Notifications
{
    public sealed class ArenaRunVictoryNotification : IArenaRunNotification
    {
        public ArenaRunVictoryNotification(
            ArenaRunId arenaRunId,
            AggregateRevision revision)
        {
            if (arenaRunId.IsNone)
            {
                throw new ArgumentException("ArenaRunId cannot be None.", nameof(arenaRunId));
            }

            ArenaRunId = arenaRunId;
            Revision = revision;
        }

        public ArenaRunId ArenaRunId { get; }

        public AggregateRevision Revision { get; }
    }
}
