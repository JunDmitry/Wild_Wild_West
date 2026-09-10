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
    }
}
