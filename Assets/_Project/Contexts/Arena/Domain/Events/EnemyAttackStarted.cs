using System;
using Game.Arena.Domain.Combat;
using Game.Arena.Domain.Concurrency;
using Game.Arena.Domain.Identity;
using Game.Arena.Domain.Time;

namespace Game.Arena.Domain.Events
{
    public sealed class EnemyAttackStarted : IPooledArenaDomainEvent, IEquatable<EnemyAttackStarted>
    {
        internal EnemyAttackStarted()
        {
        }

        public EnemyAttackStarted(
            ArenaRunId arenaRunId,
            AggregateRevision aggregateRevision,
            EnemyId enemyId,
            AttackId attackId,
            GameTimePoint startedAt,
            GameTimePoint impactAt)
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

            if (impactAt < startedAt)
            {
                throw new ArgumentOutOfRangeException(nameof(impactAt));
            }

            ArenaRunId = arenaRunId;
            AggregateRevision = aggregateRevision;
            EnemyId = enemyId;
            AttackId = attackId;
            StartedAt = startedAt;
            ImpactAt = impactAt;
        }

        public ArenaRunId ArenaRunId { get; private set; }

        public AggregateRevision AggregateRevision { get; private set; }

        public EnemyId EnemyId { get; private set; }

        public AttackId AttackId { get; private set; }

        public GameTimePoint StartedAt { get; private set; }

        public GameTimePoint ImpactAt { get; private set; }

        internal void Fill(
            ArenaRunId arenaRunId,
            AggregateRevision aggregateRevision,
            EnemyId enemyId,
            AttackId attackId,
            GameTimePoint startedAt,
            GameTimePoint impactAt)
        {
            ArenaRunId = arenaRunId;
            AggregateRevision = aggregateRevision;
            EnemyId = enemyId;
            AttackId = attackId;
            StartedAt = startedAt;
            ImpactAt = impactAt;
        }

        void IPooledArenaDomainEvent.Reset()
        {
            ArenaRunId = default;
            AggregateRevision = default;
            EnemyId = default;
            AttackId = default;
            StartedAt = default;
            ImpactAt = default;
        }

        public bool Equals(EnemyAttackStarted other)
        {
            if (other is null)
            {
                return false;
            }

            return ArenaRunId.Equals(other.ArenaRunId)
                && AggregateRevision.Equals(other.AggregateRevision)
                && EnemyId.Equals(other.EnemyId)
                && AttackId.Equals(other.AttackId)
                && StartedAt.Equals(other.StartedAt)
                && ImpactAt.Equals(other.ImpactAt);
        }

        public override bool Equals(object obj)
        {
            return obj is EnemyAttackStarted other && Equals(other);
        }

        public override int GetHashCode()
        {
            return HashCode.Combine(
                ArenaRunId,
                AggregateRevision,
                EnemyId,
                AttackId,
                StartedAt,
                ImpactAt);
        }
    }
}
