using System;
using Game.Arena.Domain.Concurrency;
using Game.Arena.Domain.Identity;

namespace Game.Arena.Application.Notifications
{
    public sealed class ArenaRunEnemyDamagedNotification : IArenaRunNotification
    {
        internal ArenaRunEnemyDamagedNotification()
        {
        }

        public ArenaRunEnemyDamagedNotification(
            ArenaRunId arenaRunId,
            AggregateRevision revision,
            EnemyId enemyId)
        {
            Fill(arenaRunId, revision, enemyId);
        }

        public ArenaRunId ArenaRunId { get; private set; }

        public AggregateRevision Revision { get; private set; }

        public EnemyId EnemyId { get; private set; }

        internal void Fill(
            ArenaRunId arenaRunId,
            AggregateRevision revision,
            EnemyId enemyId)
        {
            if (arenaRunId.IsNone)
            {
                throw new ArgumentException("ArenaRunId cannot be None.", nameof(arenaRunId));
            }

            if (enemyId.IsNone)
            {
                throw new ArgumentException("EnemyId cannot be None.", nameof(enemyId));
            }

            ArenaRunId = arenaRunId;
            Revision = revision;
            EnemyId = enemyId;
        }
    }
}
