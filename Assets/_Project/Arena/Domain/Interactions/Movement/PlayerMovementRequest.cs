using System;
using Game.Arena.Domain.Geometry;
using Game.Arena.Domain.Interactions;
using Game.Arena.Domain.Interactions.Contracts;

namespace Game.Arena.Domain.Interactions.Movement
{
    public readonly struct PlayerMovementRequest : IInteractionRequest, IEquatable<PlayerMovementRequest>
    {
        public PlayerMovementRequest(
            InteractionCorrelation correlation,
            Position3D from,
            Position3D requestedPosition,
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

            if (collisionRadius.IsValid == false)
            {
                throw new ArgumentException("CollisionRadius is invalid.", nameof(collisionRadius));
            }

            if (from == requestedPosition)
            {
                throw new ArgumentException("Requested position must differ from source position.", nameof(requestedPosition));
            }

            Correlation = correlation;
            From = from;
            RequestedPosition = requestedPosition;
            CollisionRadius = collisionRadius;
        }

        public InteractionCorrelation Correlation { get; }
        public InteractionKind Kind => InteractionKind.PlayerMovement;
        public Position3D From { get; }
        public Position3D RequestedPosition { get; }
        public CollisionRadius CollisionRadius { get; }

        public bool Equals(PlayerMovementRequest other)
        {
            return Correlation.Equals(other.Correlation)
                && From.Equals(other.From)
                && RequestedPosition.Equals(other.RequestedPosition)
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
                From,
                RequestedPosition,
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
