using System;
using Game.Core.Model.Entities;

namespace Game.Core.Rules.Mathematics
{
    internal static class Math3D
    {
        private const float NormalizeEpsilon = .0001f;

        public static float LengthXZ(Direction3D direction)
        {
            float x = direction.X;
            float z = direction.Z;

            return (float)Math.Sqrt(x * x + z * z);
        }

        public static Direction3D NormalizeXZ(Direction3D direction)
        {
            float length = LengthXZ(direction);

            return length <= NormalizeEpsilon ? Direction3D.Zero : new Direction3D(direction.X / length, 0f, direction.Z / length);
        }

        public static float DistanceXZ(Position3D a, Position3D b)
        {
            float dx = a.X - b.X;
            float dz = a.Z - b.Z;

            return (float)Math.Sqrt(dx * dx + dz * dz);
        }

        public static Position3D OffsetXZ(Position3D position, Direction3D direction, float distance)
        {
            return new(position.X + direction.X * distance, position.Y, position.Z + direction.Z * distance);
        }

        public static float Clamp01(float value)
        {
            return Clamp(value, 0f, 1f);
        }

        public static float Clamp(float value, float min, float max)
        {
            return value < min ? min : (value > max ? max : value);
        }

        public static int Max(int a, int b)
        {
            return a > b ? a : b;
        }

        public static float Max(float a, float b)
        {
            return a > b ? a : b;
        }

        public static int Min(int a, int b)
        {
            return a < b ? a : b;
        }

        public static float Min(float a, float b)
        {
            return a < b ? a : b;
        }

        public static Direction3D Add(Direction3D a, Direction3D b)
        {
            return new Direction3D(a.X + b.X, a.Y + b.Y, a.Z + b.Z);
        }

        public static Direction3D Subtract(Direction3D a, Direction3D b)
        {
            return new Direction3D(a.X - b.X, a.Y - b.Y, a.Z - b.Z);
        }

        public static Direction3D Multiple(Direction3D d, float s)
        {
            return new Direction3D(d.X * s, d.Y * s, d.Z * s);
        }

        public static Direction3D Multiple(float s, Direction3D d)
        {
            return Multiple(d, s);
        }

        public static Position3D Add(Position3D p, Direction3D d)
        {
            return new Position3D(p.X + d.X, p.Y + d.Y, p.Z + d.Z);
        }

        public static Position3D Subtract(Position3D p)
        {
            return new Position3D(-p.X, -p.Y, -p.Z);
        }

        public static Direction3D Subtract(Position3D a, Position3D b)
        {
            return new Direction3D(a.X - b.X, a.Y - b.Y, a.Z - b.Z);
        }
    }
}
