using System;
using Game.Arena.Domain.Combat;
using Game.Arena.Domain.Geometry;
using Game.Arena.Domain.Interactions.Contracts;

namespace Game.Arena.Domain.Interactions.Attack
{
    public readonly struct PlayerAttackImpactRequest : IInteractionRequest, IEquatable<PlayerAttackImpactRequest>
    {
        public PlayerAttackImpactRequest(
            InteractionCorrelation correlation,
            AttackId attackId,
            WeaponKind weaponKind,
            Position3D origin,
            Direction3D aimDirection,
            Distance range)
        {
            if (correlation.ArenaRunId.IsNone)
            {
                throw new ArgumentException("ArenaRunId cannot be None.", nameof(correlation));
            }

            if (correlation.InteractionId.IsNone)
            {
                throw new ArgumentException("InteractionId cannot be None.", nameof(correlation));
            }

            if (attackId.IsNone)
            {
                throw new ArgumentException("AttackId cannot be None.", nameof(attackId));
            }

            if (aimDirection.IsValid == false)
            {
                throw new ArgumentException("Direction is invalid.", nameof(aimDirection));
            }

            if (range.IsZero)
            {
                throw new ArgumentOutOfRangeException(nameof(range));
            }

            Correlation = correlation;
            AttackId = attackId;
            WeaponKind = weaponKind;
            Origin = origin;
            AimDirection = aimDirection;
            Range = range;
        }

        public InteractionCorrelation Correlation { get; }

        public InteractionKind Kind => InteractionKind.PlayerAttack;

        public AttackId AttackId { get; }

        public WeaponKind WeaponKind { get; }

        public Position3D Origin { get; }

        public Direction3D AimDirection { get; }

        public Distance Range { get; }

        public bool Equals(PlayerAttackImpactRequest other)
        {
            return Correlation.Equals(other.Correlation)
                && AttackId.Equals(other.AttackId)
                && WeaponKind == other.WeaponKind
                && Origin.Equals(other.Origin)
                && AimDirection.Equals(other.AimDirection)
                && Range.Equals(other.Range);
        }

        public override bool Equals(object obj)
        {
            return obj is PlayerAttackImpactRequest other && Equals(other);
        }

        public override int GetHashCode()
        {
            return HashCode.Combine(Correlation, AttackId, WeaponKind, Origin, AimDirection, Range);
        }

        public static bool operator ==(PlayerAttackImpactRequest left, PlayerAttackImpactRequest right)
        {
            return left.Equals(right);
        }

        public static bool operator !=(PlayerAttackImpactRequest left, PlayerAttackImpactRequest right)
        {
            return !left.Equals(right);
        }
    }
}
