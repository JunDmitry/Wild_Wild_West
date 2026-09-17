using System;
using Game.Arena.Domain.Concurrency;
using Game.Arena.Domain.Identity;
using Game.Arena.Domain.Vitality;

namespace Game.Arena.Domain.Events
{
    public readonly struct EnemyDamaged : IArenaDomainEvent, IEquatable<EnemyDamaged>
    {
        public EnemyDamaged(
            ArenaRunId arenaRunId,
            AggregateRevision aggregateRevision,
            EnemyId enemyId,
            DamageAmount damage,
            Health remainingHealth)
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
            AggregateRevision = aggregateRevision;
            EnemyId = enemyId;
            Damage = damage;
            RemainingHealth = remainingHealth;
        }

        public ArenaRunId ArenaRunId { get; }

        public AggregateRevision AggregateRevision { get; }

        public EnemyId EnemyId { get; }

        public DamageAmount Damage { get; }

        public Health RemainingHealth { get; }

        public bool Equals(EnemyDamaged other)
        {
            return ArenaRunId.Equals(other.ArenaRunId)
                && AggregateRevision.Equals(other.AggregateRevision)
                && EnemyId.Equals(other.EnemyId)
                && Damage.Equals(other.Damage)
                && RemainingHealth.Equals(other.RemainingHealth);
        }

        public override bool Equals(object obj)
        {
            return obj is EnemyDamaged other && Equals(other);
        }

        public override int GetHashCode()
        {
            return HashCode.Combine(
                ArenaRunId,
                AggregateRevision,
                EnemyId,
                Damage,
                RemainingHealth);
        }
    }
}
