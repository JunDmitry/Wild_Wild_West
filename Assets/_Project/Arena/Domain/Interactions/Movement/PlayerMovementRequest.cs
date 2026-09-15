using System;
using Game.Arena.Domain.Geometry;
using Game.Arena.Domain.Interactions.Contracts;

namespace Game.Arena.Domain.Interactions.Movement
{
    public readonly struct PlayerMovementRequest : IInteractionRequest, IEquatable<PlayerMovementRequest>
    {
        public PlayerMovementRequest(
            InteractionCorrelation correlation,
            Position3D from,
            Direction3D direction,
            Distance requestedDistance,
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

            if (direction.IsValid == false)
            {
                throw new ArgumentException("Direction is invalid.", nameof(direction));
            }

            if (direction.Y != 0f)
            {
                throw new ArgumentException("Player movement is planar.", nameof(direction));
            }

            if (requestedDistance.IsZero)
            {
                throw new ArgumentOutOfRangeException(nameof(requestedDistance));
            }

            if (collisionRadius.IsValid == false)
            {
                throw new ArgumentException("CollisionRadius is invalid.", nameof(collisionRadius));
            }

            Correlation = correlation;
            From = from;
            Direction = direction;
            RequestedDistance = requestedDistance;
            CollisionRadius = collisionRadius;
        }

        public InteractionCorrelation Correlation { get; }
        public InteractionKind Kind => InteractionKind.PlayerMovement;
        public Position3D From { get; }
        public Direction3D Direction { get; }
        public Distance RequestedDistance { get; }
        public Position3D RequestedPosition => From.MovedAlong(Direction, RequestedDistance);
        public CollisionRadius CollisionRadius { get; }

        public bool Equals(PlayerMovementRequest other)
        {
            return Correlation.Equals(other.Correlation)
                && From.Equals(other.From)
                && Direction.Equals(other.Direction)
                && RequestedDistance.Equals(other.RequestedDistance)
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
                Direction,
                RequestedDistance,
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
