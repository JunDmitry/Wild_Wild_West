using System;
using Game.Arena.Domain.Time;

namespace Game.Arena.Domain.Combat
{
    public readonly struct PendingAttack : IEquatable<PendingAttack>
    {
        public PendingAttack(
            AttackId id,
            AttackActor actor,
            WeaponKind weaponKind,
            GameTimePoint startedAt,
            GameTimePoint impactAt)
        {
            if (id.IsNone)
            {
                throw new ArgumentException("AttackId cannot be None.", nameof(id));
            }

            if (impactAt < startedAt)
            {
                throw new ArgumentOutOfRangeException(nameof(impactAt));
            }

            Id = id;
            Actor = actor;
            WeaponKind = weaponKind;
            StartedAt = startedAt;
            ImpactAt = impactAt;
        }

        public AttackId Id { get; }
        public AttackActor Actor { get; }
        public WeaponKind WeaponKind { get; }
        public GameTimePoint StartedAt { get; }
        public GameTimePoint ImpactAt { get; }

        public bool IsReadyToImpact(GameTimePoint now)
        {
            return now >= ImpactAt;
        }

        public bool Equals(PendingAttack other)
        {
            return Id.Equals(other.Id)
                && Actor.Equals(other.Actor)
                && WeaponKind == other.WeaponKind
                && StartedAt.Equals(other.StartedAt)
                && ImpactAt.Equals(other.ImpactAt);
        }

        public override bool Equals(object obj)
        {
            return obj is PendingAttack other && Equals(other);
        }

        public override int GetHashCode()
        {
            return HashCode.Combine(
                Id, 
                Actor, 
                WeaponKind, 
                StartedAt, 
                ImpactAt);
        }
    }
}
