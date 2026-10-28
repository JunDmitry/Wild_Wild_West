using System;
using Game.Arena.Domain.Concurrency;
using Game.Arena.Domain.Identity;

namespace Game.Arena.Application.Notifications
{
    public sealed class ArenaRunEnemyAttackResolvedNotification : IArenaRunNotification
    {
        internal ArenaRunEnemyAttackResolvedNotification()
        {
        }

        public ArenaRunEnemyAttackResolvedNotification(
            ArenaRunId arenaRunId,
            AggregateRevision revision,
            EnemyId enemyId,
            ArenaRunEnemyAttackOutcome outcome)
        {
            Fill(arenaRunId, revision, enemyId, outcome);
        }

        public ArenaRunId ArenaRunId { get; private set; }

        public AggregateRevision Revision { get; private set; }

        public EnemyId EnemyId { get; private set; }

        public ArenaRunEnemyAttackOutcome Outcome { get; private set; }

        internal void Fill(
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
    }
}
