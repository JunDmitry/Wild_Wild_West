using System;

namespace Game.Arena.Domain.Geometry
{
    public readonly struct BoundedMovement : IEquatable<BoundedMovement>
    {
        public BoundedMovement(
            Direction3D direction,
            Distance distance)
        {
            if (direction.IsValid == false)
            {
                throw new ArgumentException("Direction is invalid.", nameof(direction));
            }

            if (direction.Y != 0f)
            {
                throw new ArgumentException("Bounded movement is planar.", nameof(direction));
            }

            Direction = direction;
            Distance = distance;
        }

        public Direction3D Direction { get; }

        public Distance Distance { get; }

        public bool IsZero => Distance.IsZero;

        public bool Equals(BoundedMovement other)
        {
            return Direction.Equals(other.Direction)
                && Distance.Equals(other.Distance);
        }

        public override bool Equals(object obj)
        {
            return obj is BoundedMovement other && Equals(other);
        }

        public override int GetHashCode()
        {
            return HashCode.Combine(Direction, Distance);
        }

        public override string ToString()
        {
            return "BoundedMovement" + Direction + " " + Distance;
        }

        public static bool operator ==(BoundedMovement left, BoundedMovement right)
        {
            return left.Equals(right);
        }

        public static bool operator !=(BoundedMovement left, BoundedMovement right)
        {
            return !left.Equals(right);
        }
    }
}
