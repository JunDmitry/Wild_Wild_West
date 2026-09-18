using System;
using Game.Arena.Domain.Combat;
using Game.Arena.Domain.Concurrency;
using Game.Arena.Domain.Identity;

namespace Game.Arena.Domain.Events
{
    public readonly struct EnemyAttackCancelled : IArenaDomainEvent, IEquatable<EnemyAttackCancelled>
    {
        public EnemyAttackCancelled(
            ArenaRunId arenaRunId,
            AggregateRevision aggregateRevision,
            EnemyId enemyId,
            AttackId attackId,
            AttackCancellationCause cause)
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
            Cause = cause;
        }

        public ArenaRunId ArenaRunId { get; }

        public AggregateRevision AggregateRevision { get; }

        public EnemyId EnemyId { get; }

        public AttackId AttackId { get; }

        public AttackCancellationCause Cause { get; }

        public bool Equals(EnemyAttackCancelled other)
        {
            return ArenaRunId.Equals(other.ArenaRunId)
                && AggregateRevision.Equals(other.AggregateRevision)
                && EnemyId.Equals(other.EnemyId)
                && AttackId.Equals(other.AttackId)
                && Cause == other.Cause;
        }

        public override bool Equals(object obj)
        {
            return obj is EnemyAttackCancelled other && Equals(other);
        }

        public override int GetHashCode()
        {
            return HashCode.Combine(
                ArenaRunId,
                AggregateRevision,
                EnemyId,
                AttackId,
                Cause);
        }
    }
}
