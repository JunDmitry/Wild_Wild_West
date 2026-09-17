using System;
using Game.Arena.Domain.Combat;
using Game.Arena.Domain.Concurrency;
using Game.Arena.Domain.Identity;
using Game.Arena.Domain.Time;

namespace Game.Arena.Domain.Events
{
    public readonly struct PlayerAttackStarted : IArenaDomainEvent, IEquatable<PlayerAttackStarted>
    {
        public PlayerAttackStarted(
            ArenaRunId arenaRunId,
            AggregateRevision aggregateRevision,
            PlayerId playerId,
            AttackId attackId,
            WeaponKind weaponKind,
            GameTimePoint startedAt,
            GameTimePoint impactAt)
        {
            if (arenaRunId.IsNone)
            {
                throw new ArgumentException("ArenaRunId cannot be None.", nameof(arenaRunId));
            }

            if (playerId.IsNone)
            {
                throw new ArgumentException("PlayerId cannot be None.", nameof(playerId));
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
            PlayerId = playerId;
            AttackId = attackId;
            WeaponKind = weaponKind;
            StartedAt = startedAt;
            ImpactAt = impactAt;
        }

        public ArenaRunId ArenaRunId { get; }
        public AggregateRevision AggregateRevision { get; }
        public PlayerId PlayerId { get; }
        public AttackId AttackId { get; }
        public WeaponKind WeaponKind { get; }
        public GameTimePoint StartedAt { get; }
        public GameTimePoint ImpactAt { get; }

        public bool Equals(PlayerAttackStarted other)
        {
            return ArenaRunId.Equals(other.ArenaRunId)
                && AggregateRevision.Equals(other.AggregateRevision)
                && PlayerId.Equals(other.PlayerId)
                && AttackId.Equals(other.AttackId)
                && WeaponKind == other.WeaponKind
                && StartedAt.Equals(other.StartedAt)
                && ImpactAt.Equals(other.ImpactAt);
        }

        public override bool Equals(object obj)
        {
            return obj is PlayerAttackStarted other && Equals(other);
        }

        public override int GetHashCode()
        {
            return HashCode.Combine(
                ArenaRunId,
                AggregateRevision,
                PlayerId,
                AttackId,
                WeaponKind,
                StartedAt,
                ImpactAt);
        }
    }
}
