using System;
using Game.Arena.Domain.Geometry;
using Game.Arena.Domain.Interactions.Contracts;
using Game.Arena.Domain.Movement;

namespace Game.Arena.Domain.Interactions.Movement
{
    public readonly struct PlayerMovementRequest : IInteractionRequest, IEquatable<PlayerMovementRequest>
    {
        public PlayerMovementRequest(
            InteractionCorrelation correlation,
            PlanarMovementIntent intent,
            CollisionRadius collisionRadius)
        {
            if (correlation.ArenaRunId.IsNone)
            {
                throw new ArgumentException("ArenaRunId cannot be None.", nameof(correlation));
            }

            if (correlation.InteractionId.IsNone)
            {
                throw new ArgumentException("InteractionId cannot be None.", nameof(correlation));
            }

            if (intent.IsValid == false)
            {
                throw new ArgumentException("Movement intent is invalid.", nameof(intent));
            }

            if (collisionRadius.IsValid == false)
            {
                throw new ArgumentException("CollisionRadius is invalid.", nameof(collisionRadius));
            }

            Correlation = correlation;
            Intent = intent;
            CollisionRadius = collisionRadius;
        }

        public InteractionCorrelation Correlation { get; }
        public PlanarMovementIntent Intent { get; }
        public CollisionRadius CollisionRadius { get; }

        public InteractionKind Kind => InteractionKind.PlayerMovement;
        public Position3D From => Intent.From;
        public Direction3D Direction => Intent.Direction;
        public Distance RequestedDistance => Intent.RequestedDistance;
        public Position3D RequestedPosition => Intent.RequestedPosition;

        public bool Equals(PlayerMovementRequest other)
        {
            return Correlation.Equals(other.Correlation)
                && Intent.Equals(other.Intent)
                && CollisionRadius.Equals(other.CollisionRadius);
        }

        public override bool Equals(object obj)
        {
            return obj is PlayerMovementRequest other && Equals(other);
        }

        public override int GetHashCode()
        {
            return HashCode.Combine(
                Correlation,
                Intent,
                CollisionRadius);
        }

        public static bool operator ==(PlayerMovementRequest left, PlayerMovementRequest right)
        {
            return left.Equals(right);
        }

        public static bool operator !=(PlayerMovementRequest left, PlayerMovementRequest right)
        {
            return !left.Equals(right);
        }
    }
}
