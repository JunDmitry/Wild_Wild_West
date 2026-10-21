using System;
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

            if (impactAt < startedAt)
            {
                throw new ArgumentOutOfRangeException(nameof(impactAt));
            }

            Id = id;
            PlayerId = playerId;
            WeaponKind = weaponKind;
            StartedAt = startedAt;
            ImpactAt = impactAt;
        }

        public AttackId Id { get; }
        public PlayerId PlayerId { get; }
        public WeaponKind WeaponKind { get; }
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
