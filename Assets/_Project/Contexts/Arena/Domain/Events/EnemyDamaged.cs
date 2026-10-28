using System;
using Game.Arena.Domain.Concurrency;
using Game.Arena.Domain.Identity;
using Game.Arena.Domain.Vitality;

namespace Game.Arena.Domain.Events
{
    public sealed class EnemyDamaged : IPooledArenaDomainEvent, IEquatable<EnemyDamaged>
    {
        internal EnemyDamaged()
        {
        }

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

        public ArenaRunId ArenaRunId { get; private set; }

        public AggregateRevision AggregateRevision { get; private set; }

        public EnemyId EnemyId { get; private set; }

        public DamageAmount Damage { get; private set; }

        public Health RemainingHealth { get; private set; }

        internal void Fill(
            ArenaRunId arenaRunId,
            AggregateRevision aggregateRevision,
            EnemyId enemyId,
            DamageAmount damage,
            Health remainingHealth)
        {
            ArenaRunId = arenaRunId;
            AggregateRevision = aggregateRevision;
            EnemyId = enemyId;
            Damage = damage;
            RemainingHealth = remainingHealth;
        }

        void IPooledArenaDomainEvent.Reset()
        {
            ArenaRunId = default;
            AggregateRevision = default;
            EnemyId = default;
            Damage = default;
            RemainingHealth = default;
        }

        public bool Equals(EnemyDamaged other)
        {
            if (other is null)
            {
                return false;
            }

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
