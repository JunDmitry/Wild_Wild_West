using System;

namespace Game.Arena.Domain.Geometry
{
    public readonly struct Position3D : IEquatable<Position3D>
    {
        public Position3D(float x, float y, float z)
        {
            if (!IsFinite(x) || !IsFinite(y) || !IsFinite(z))
            {
                throw new ArgumentOutOfRangeException();
            }

            X = x;
            Y = y;
            Z = z;
        }

        public static Position3D Zero { get; } = new Position3D(0f, 0f, 0f);

        public float X { get; }

        public float Y { get; }

        public float Z { get; }

        public float DistanceTo(Position3D other)
        {
            float dx = X - other.X;
            float dy = Y - other.Y;
            float dz = Z - other.Z;

            return (float)Math.Sqrt((dx * dx) + (dy * dy) + (dz * dz));
        }

        public float GroundDistanceTo(Position3D other)
        {
            float dx = X - other.X;
            float dz = Z - other.Z;

            return (float)Math.Sqrt((dx * dx) + (dz * dz));
        }

        public Position3D MovedAlong(Direction3D direction, float distance)
        {
            if (!IsFinite(distance) || distance < 0f)
            {
                throw new ArgumentOutOfRangeException(nameof(distance));
            }

            return new Position3D(
                X + (direction.X * distance),
                Y + (direction.Y * distance),
                Z + (direction.Z * distance));
        }

        public Position3D WithHeight(float y)
        {
            return new Position3D(X, y, Z);
        }

        public bool Equals(Position3D other)
        {
            return X == other.X && Y == other.Y && Z == other.Z;
        }

        public override bool Equals(object obj)
        {
            return obj is Position3D other && Equals(other);
        }

        public override int GetHashCode()
        {
            return HashCode.Combine(X, Y, Z);
        }

        public override string ToString()
        {
            return "(" + X.ToString() + ", " + Y.ToString() + ", " + Z.ToString() + ")";
        }

        public static bool operator ==(Position3D left, Position3D right)
        {
            return left.Equals(right);
        }

        public static bool operator !=(Position3D left, Position3D right)
        {
            return !left.Equals(right);
        }

        public static Position3D operator +(Position3D position, Displacement3D displacement)
        {
            return new Position3D(
                position.X + displacement.X,
                position.Y + displacement.Y,
                position.Z + displacement.Z);
        }

        public static Displacement3D operator -(Position3D left, Position3D right)
        {
            return new Displacement3D(
                left.X - right.X,
                left.Y - right.Y,
                left.Z - right.Z);
        }

        private static bool IsFinite(float value)
        {
            return !float.IsNaN(value) && !float.IsInfinity(value);
        }
    }
}
