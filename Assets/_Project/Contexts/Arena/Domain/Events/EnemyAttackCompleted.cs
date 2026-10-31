using System;
using Game.Arena.Domain.Combat;
using Game.Arena.Domain.Concurrency;
using Game.Arena.Domain.Identity;
using Game.Arena.Domain.Time;

namespace Game.Arena.Domain.Events
{
    public sealed class EnemyAttackCompleted : IPooledArenaDomainEvent, IEquatable<EnemyAttackCompleted>
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

        internal EnemyAttackCompleted()
        {
        }

        public ArenaRunId ArenaRunId { get; private set; }

        public AggregateRevision AggregateRevision { get; private set; }

        public EnemyId EnemyId { get; private set; }

        public AttackId AttackId { get; private set; }

        public AttackOutcome Outcome { get; private set; }

        public GameTimePoint CompletedAt { get; private set; }

        internal void Fill(
            ArenaRunId arenaRunId,
            AggregateRevision aggregateRevision,
            EnemyId enemyId,
            AttackId attackId,
            AttackOutcome outcome,
            GameTimePoint completedAt)
        {
            ArenaRunId = arenaRunId;
            AggregateRevision = aggregateRevision;
            EnemyId = enemyId;
            AttackId = attackId;
            Outcome = outcome;
            CompletedAt = completedAt;
        }

        void IPooledArenaDomainEvent.Reset()
        {
            ArenaRunId = default;
            AggregateRevision = default;
            EnemyId = default;
            AttackId = default;
            Outcome = default;
            CompletedAt = default;
        }

        public bool Equals(EnemyAttackCompleted other)
        {
            if (other is null)
            {
                return false;
            }

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
