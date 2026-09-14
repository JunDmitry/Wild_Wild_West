using System;

namespace Game.Arena.Domain.Geometry
{
    public readonly struct Displacement3D : IEquatable<Displacement3D>
    {
        public Displacement3D(float x, float y, float z)
        {
            if (!IsFinite(x) || !IsFinite(y) || !IsFinite(z))
            {
                throw new ArgumentOutOfRangeException();
            }

            X = x;
            Y = y;
            Z = z;
        }

        public static Displacement3D Zero { get; } = new Displacement3D(0f, 0f, 0f);

        public float X { get; }

        public float Y { get; }

        public float Z { get; }

        public float LengthSquared => (float)(((double)X * X) + ((double)Y * Y) + ((double)Z * Z));

        public float Length => (float)Math.Sqrt(((double)X * X) + ((double)Y * Y) + ((double)Z * Z));

        public bool IsZero
        {
            get
            {
                float toleranceSquared = GeometryTolerance.MinimumDirectionLength * GeometryTolerance.MinimumDirectionLength;

                return LengthSquared <= toleranceSquared;
            }
        }

        public bool TryToDirection(out Direction3D direction)
        {
            return Direction3D.TryFrom(this, out direction);
        }

        public bool Equals(Displacement3D other)
        {
            return X == other.X && Y == other.Y && Z == other.Z;
        }

        public override bool Equals(object obj)
        {
            return obj is Displacement3D other && Equals(other);
        }

        public override int GetHashCode()
        {
            return HashCode.Combine(X, Y, Z);
        }

        public override string ToString()
        {
            return "<" + X.ToString() + ", " + Y.ToString() + ", " + Z.ToString() + ">";
        }

        public static bool operator ==(Displacement3D left, Displacement3D right)
        {
            return left.Equals(right);
        }

        public static bool operator !=(Displacement3D left, Displacement3D right)
        {
            return !left.Equals(right);
        }

        public static Displacement3D operator +(Displacement3D left, Displacement3D right)
        {
            return new Displacement3D(
                left.X + right.X,
                left.Y + right.Y,
                left.Z + right.Z);
        }

        public static Displacement3D operator -(Displacement3D left, Displacement3D right)
        {
            return new Displacement3D(
                left.X - right.X,
                left.Y - right.Y,
                left.Z - right.Z);
        }

        public static Displacement3D operator -(Displacement3D value)
        {
            return new Displacement3D(-value.X, -value.Y, -value.Z);
        }

        public static Displacement3D operator *(Displacement3D value, float scale)
        {
            return new Displacement3D(value.X * scale, value.Y * scale, value.Z * scale);
        }

        public static Displacement3D operator *(float scale, Displacement3D value)
        {
            return value * scale;
        }

        private static bool IsFinite(float value)
        {
            return !float.IsNaN(value) && !float.IsInfinity(value);
        }
    }
}
