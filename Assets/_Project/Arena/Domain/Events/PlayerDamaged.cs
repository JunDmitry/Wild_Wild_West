using System;
using Game.Arena.Domain.Combat;
using Game.Arena.Domain.Concurrency;
using Game.Arena.Domain.Identity;
using Game.Arena.Domain.Vitality;

namespace Game.Arena.Domain.Events
{
    public readonly struct PlayerDamaged : IArenaDomainEvent, IEquatable<PlayerDamaged>
    {
        public PlayerDamaged(
            ArenaRunId arenaRunId,
            AggregateRevision aggregateRevision,
            PlayerId playerId,
            EnemyId sourceEnemyId,
            AttackId sourceAttackId,
            DamageAmount appliedDamage,
            Health remainingHealth)
        {
            if (arenaRunId.IsNone)
            {
                throw new ArgumentException("ArenaRunId cannot be None.", nameof(arenaRunId));
            }

            if (playerId.IsNone)
            {
                throw new ArgumentException("PlayerId cannot be None.", nameof(playerId));
            }

            if (sourceEnemyId.IsNone)
            {
                throw new ArgumentException("EnemyId cannot be None.", nameof(sourceEnemyId));
            }

            if (sourceAttackId.IsNone)
            {
                throw new ArgumentException("AttackId cannot be None.", nameof(sourceAttackId));
            }

            if (remainingHealth.IsValid == false)
            {
                throw new ArgumentException("Health is invalid.", nameof(remainingHealth));
            }

            ArenaRunId = arenaRunId;
            AggregateRevision = aggregateRevision;
            PlayerId = playerId;
            SourceEnemyId = sourceEnemyId;
            SourceAttackId = sourceAttackId;
            AppliedDamage = appliedDamage;
            RemainingHealth = remainingHealth;
        }

        public ArenaRunId ArenaRunId { get; }

        public AggregateRevision AggregateRevision { get; }

        public PlayerId PlayerId { get; }

        public EnemyId SourceEnemyId { get; }

        public AttackId SourceAttackId { get; }

        public DamageAmount AppliedDamage { get; }

        public Health RemainingHealth { get; }

        public bool Equals(PlayerDamaged other)
        {
            return ArenaRunId.Equals(other.ArenaRunId)
                && AggregateRevision.Equals(other.AggregateRevision)
                && PlayerId.Equals(other.PlayerId)
                && SourceEnemyId.Equals(other.SourceEnemyId)
                && SourceAttackId.Equals(other.SourceAttackId)
                && AppliedDamage.Equals(other.AppliedDamage)
                && RemainingHealth.Equals(other.RemainingHealth);
        }

        public override bool Equals(object obj)
        {
            return obj is PlayerDamaged other && Equals(other);
        }

        public override int GetHashCode()
        {
            return HashCode.Combine(
                ArenaRunId,
                AggregateRevision,
                PlayerId,
                SourceEnemyId,
                SourceAttackId,
                AppliedDamage,
                RemainingHealth);
        }
    }
}
