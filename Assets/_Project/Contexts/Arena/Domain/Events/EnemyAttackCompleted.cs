using System;
using Game.Arena.Domain.Combat;
using Game.Arena.Domain.Concurrency;
using Game.Arena.Domain.Identity;
using Game.Arena.Domain.Time;

namespace Game.Arena.Domain.Events
{
    public readonly struct EnemyAttackCompleted : IArenaDomainEvent, IEquatable<EnemyAttackCompleted>
    {
        public EnemyAttackCompleted(
            ArenaRunId arenaRunId,
            AggregateRevision aggregateRevision,
            EnemyId enemyId,
            AttackId attackId,
            AttackOutcome outcome,
            GameTimePoint completedAt)
        {
            if (arenaRunId.IsNone)
            {
                throw new ArgumentException("ArenaRunId cannot be None.", nameof(arenaRunId));
            }

            if (enemyId.IsNone)
            {
                throw new ArgumentException("EnemyId cannot be None.", nameof(enemyId));
            }

            if (attackId.IsNone)
            {
                throw new ArgumentException("AttackId cannot be None.", nameof(attackId));
            }

            ArenaRunId = arenaRunId;
            AggregateRevision = aggregateRevision;
            EnemyId = enemyId;
            AttackId = attackId;
            Outcome = outcome;
            CompletedAt = completedAt;
        }

        public ArenaRunId ArenaRunId { get; }

        public AggregateRevision AggregateRevision { get; }

        public EnemyId EnemyId { get; }

        public AttackId AttackId { get; }

        public AttackOutcome Outcome { get; }

        public GameTimePoint CompletedAt { get; }

        public bool Equals(EnemyAttackCompleted other)
        {
            return ArenaRunId.Equals(other.ArenaRunId)
                && AggregateRevision.Equals(other.AggregateRevision)
                && EnemyId.Equals(other.EnemyId)
                && AttackId.Equals(other.AttackId)
                && Outcome == other.Outcome
                && CompletedAt.Equals(other.CompletedAt);
        }

        public override bool Equals(object obj)
        {
            return obj is EnemyAttackCompleted other && Equals(other);
        }

        public override int GetHashCode()
        {
            return HashCode.Combine(
                ArenaRunId,
                AggregateRevision,
                EnemyId,
                AttackId,
                Outcome,
                CompletedAt);
        }
    }
}
