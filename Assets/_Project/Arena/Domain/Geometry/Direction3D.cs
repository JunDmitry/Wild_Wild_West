using System;

namespace Game.Arena.Domain.Geometry
{
    public readonly struct Direction3D : IEquatable<Direction3D>
    {
        private Direction3D(float x, float y, float z)
        {
            X = x;
            Y = y;
            Z = z;
        }

        public static Direction3D Forward { get; } = new Direction3D(0f, 0f, 1f);

        public static Direction3D Back { get; } = new Direction3D(0f, 0f, -1f);

        public static Direction3D Up { get; } = new Direction3D(0f, 1f, 0f);

        public static Direction3D Down { get; } = new Direction3D(0f, -1f, 0f);

        public static Direction3D Right { get; } = new Direction3D(1f, 0f, 0f);

        public static Direction3D Left { get; } = new Direction3D(-1f, 0f, 0f);

        public float X { get; }

        public float Y { get; }

        public float Z { get; }

        public static Direction3D From(Displacement3D displacement)
        {
            bool succeeded = TryFrom(displacement, out Direction3D direction);

            if (succeeded == false)
            {
                throw new ArgumentException("Displacement is too small to determine a direction.", nameof(displacement));
            }

            return direction;
        }

        public static bool TryFrom(Displacement3D displacement, out Direction3D direction)
        {
            float length = displacement.Length;

            if (length < GeometryTolerance.MinimumDirectionLength)
            {
                direction = default;
                return false;
            }

            direction = new Direction3D(
                displacement.X / length,
                displacement.Y / length,
                displacement.Z / length);

            return true;
        }

        public float Dot(Direction3D other)
        {
            return (X * other.X) + (Y * other.Y) + (Z * other.Z);
        }

        public Direction3D Negated()
        {
            return new Direction3D(-X, -Y, -Z);
        }

        public bool Equals(Direction3D other)
        {
            return X == other.X && Y == other.Y && Z == other.Z;
        }

        public override bool Equals(object obj)
        {
            return obj is Direction3D other && Equals(other);
        }

        public override int GetHashCode()
        {
            return HashCode.Combine(X, Y, Z);
        }

        public override string ToString()
        {
            return "<" + X.ToString() + ", " + Y.ToString() + ", " + Z.ToString() + ">";
        }

        public static bool operator ==(Direction3D left, Direction3D right)
        {
            return left.Equals(right);
        }

        public static bool operator !=(Direction3D left, Direction3D right)
        {
            return !left.Equals(right);
        }

        public static Displacement3D operator *(Direction3D direction, float scale)
        {
            return new Displacement3D(direction.X * scale, direction.Y * scale, direction.Z * scale);
        }

        public static Displacement3D operator *(float scale, Direction3D direction)
        {
            return direction * scale;
        }
    }
}
