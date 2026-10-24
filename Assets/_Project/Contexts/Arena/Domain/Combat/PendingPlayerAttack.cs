using System;
using Game.Arena.Domain.Geometry;
using Game.Arena.Domain.Identity;
using Game.Arena.Domain.Time;

namespace Game.Arena.Domain.Combat
{
    public readonly struct PendingPlayerAttack : IEquatable<PendingPlayerAttack>
    {
        public PendingPlayerAttack(
            AttackId id,
            PlayerId playerId,
            WeaponKind weaponKind,
            Distance range,
            AttackArc arc,
            GameTimePoint startedAt,
            GameTimePoint impactAt)
        {
            if (id.IsNone)
            {
                throw new ArgumentException("AttackId cannot be None.", nameof(id));
            }

            if (playerId.IsNone)
            {
                throw new ArgumentException("PlayerId cannot be None.", nameof(id));
            }

            if (range.IsZero)
            {
                throw new ArgumentOutOfRangeException(nameof(range));
            }

            if (arc.IsValid == false)
            {
                throw new ArgumentOutOfRangeException(nameof(arc));
            }

            if (impactAt < startedAt)
            {
                throw new ArgumentOutOfRangeException(nameof(impactAt));
            }

            Id = id;
            PlayerId = playerId;
            WeaponKind = weaponKind;
            Range = range;
            Arc = arc;
            StartedAt = startedAt;
            ImpactAt = impactAt;
        }

        public AttackId Id { get; }
        public PlayerId PlayerId { get; }
        public WeaponKind WeaponKind { get; }
        public Distance Range { get; }
        public AttackArc Arc { get; }
        public GameTimePoint StartedAt { get; }
        public GameTimePoint ImpactAt { get; }

        public bool IsReadyToImpact(GameTimePoint now)
        {
            return now >= ImpactAt;
        }

        public bool Equals(PendingPlayerAttack other)
        {
            return Id.Equals(other.Id)
                && PlayerId.Equals(other.PlayerId)
                && WeaponKind == other.WeaponKind
                && Range.Equals(other.Range)
                && Arc.Equals(other.Arc)
                && StartedAt.Equals(other.StartedAt)
                && ImpactAt.Equals(other.ImpactAt);
        }

        public override bool Equals(object obj)
        {
            return obj is PendingPlayerAttack other && Equals(other);
        }

        public override int GetHashCode()
        {
            return HashCode.Combine(
                Id,
                PlayerId,
                WeaponKind,
                Range,
                Arc,
                StartedAt,
                ImpactAt);
        }

        public static bool operator ==(PendingPlayerAttack left, PendingPlayerAttack right)
        {
            return left.Equals(right);
        }

        public static bool operator !=(PendingPlayerAttack left, PendingPlayerAttack right)
        {
            return !left.Equals(right);
        }
    }
}
