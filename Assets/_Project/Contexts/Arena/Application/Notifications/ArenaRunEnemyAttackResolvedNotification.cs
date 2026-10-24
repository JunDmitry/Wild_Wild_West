using System;
using Game.Arena.Domain.Concurrency;
using Game.Arena.Domain.Identity;

namespace Game.Arena.Application.Notifications
{
    public sealed class ArenaRunEnemyAttackResolvedNotification : IArenaRunNotification
    {
        public ArenaRunEnemyAttackResolvedNotification(
            ArenaRunId arenaRunId,
            AggregateRevision revision,
            EnemyId enemyId,
            ArenaRunEnemyAttackOutcome outcome)
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
            Outcome = outcome;
        }

        public ArenaRunId ArenaRunId { get; }

        public AggregateRevision Revision { get; }

        public EnemyId EnemyId { get; }

        public ArenaRunEnemyAttackOutcome Outcome { get; }
    }
}
