using System;
using Game.Arena.Domain.Geometry;

namespace Game.Arena.Domain.Movement
{
    public readonly struct PlanarMovementIntent : IEquatable<PlanarMovementIntent>
    {
        public PlanarMovementIntent(
            Position3D from,
            Direction3D direction,
            Distance requestedDistance)
        {
            if (direction.IsValid == false)
            {
                throw new ArgumentException("Direction is invalid.", nameof(direction));
            }

            if (direction.Y != 0f)
            {
                throw new ArgumentException("Movement intent is planar.", nameof(direction));
            }

            if (requestedDistance.IsZero)
            {
                throw new ArgumentOutOfRangeException(nameof(requestedDistance));
            }

            From = from;
            Direction = direction;
            RequestedDistance = requestedDistance;
        }

        public Position3D From { get; }

        public Direction3D Direction { get; }

        public Distance RequestedDistance { get; }

        public Position3D RequestedPosition => From.MovedAlong(Direction, RequestedDistance);

        public bool IsValid => Direction.IsValid && RequestedDistance.IsZero == false;

        public bool Equals(PlanarMovementIntent other)
        {
            return From.Equals(other.From)
                && Direction.Equals(other.Direction)
                && RequestedDistance.Equals(other.RequestedDistance);
        }

        public override bool Equals(object obj)
        {
            return obj is PlanarMovementIntent other && Equals(other);
        }

        public override int GetHashCode()
        {
            return HashCode.Combine(From, Direction, RequestedDistance);
        }

        public static bool operator ==(PlanarMovementIntent left, PlanarMovementIntent right)
        {
            return left.Equals(right);
        }

        public static bool operator !=(PlanarMovementIntent left, PlanarMovementIntent right)
        {
            return !left.Equals(right);
        }
    }
}
