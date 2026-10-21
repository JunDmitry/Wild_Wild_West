using System;
using Game.Arena.Domain.Geometry;
using Game.Arena.Domain.Interactions;
using Game.Arena.Domain.Interactions.Contracts;

namespace Game.Arena.Domain.Interactions.Movement
{
    public readonly struct PlayerMovementResolution : IInteractionResolution, IEquatable<PlayerMovementResolution>
    {
        public PlayerMovementResolution(
            InteractionCorrelation correlation,
            Position3D acceptedPosition)
        {
            if (correlation.ArenaRunId.IsNone)
            {
                throw new ArgumentException("ArenaRunId cannot be None.", nameof(correlation));
            }

            if (correlation.InteractionId.IsNone)
            {
                throw new ArgumentException("InteractionId cannot be None.", nameof(correlation));
            }

            Correlation = correlation;
            AcceptedPosition = acceptedPosition;
        }

        public InteractionCorrelation Correlation { get; }

        public InteractionKind Kind => InteractionKind.PlayerMovement;

        public Position3D AcceptedPosition { get; }

        public bool Equals(PlayerMovementResolution other)
        {
            return Correlation.Equals(other.Correlation)
                && AcceptedPosition.Equals(other.AcceptedPosition);
        }

        public override bool Equals(object obj)
        {
            return obj is PlayerMovementResolution other && Equals(other);
        }

        public override int GetHashCode()
        {
            return HashCode.Combine(
                Correlation,
                AcceptedPosition);
        }

        public static bool operator ==(PlayerMovementResolution left, PlayerMovementResolution right)
        {
            return left.Equals(right);
        }

        public static bool operator !=(PlayerMovementResolution left, PlayerMovementResolution right)
        {
            return !left.Equals(right);
        }
    }
}
