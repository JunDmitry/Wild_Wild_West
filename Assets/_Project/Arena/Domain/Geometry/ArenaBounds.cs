using System;

namespace Game.Arena.Domain.Geometry
{
    public readonly struct ArenaBounds : IEquatable<ArenaBounds>
    {
        public ArenaBounds(
            float minimumX,
            float maximumX,
            float minimumZ,
            float maximumZ,
            float groundY)
        {
            if (IsFinite(minimumX) == false)
            {
                throw new ArgumentOutOfRangeException(nameof(minimumX));
            }

            if (IsFinite(maximumX) == false)
            {
                throw new ArgumentOutOfRangeException(nameof(maximumX));
            }

            if (IsFinite(minimumZ) == false)
            {
                throw new ArgumentOutOfRangeException(nameof(minimumZ));
            }

            if (IsFinite(maximumZ) == false)
            {
                throw new ArgumentOutOfRangeException(nameof(maximumZ));
            }

            if (IsFinite(groundY) == false)
            {
                throw new ArgumentOutOfRangeException(nameof(groundY));
            }

            if (minimumX >= maximumX)
            {
                throw new ArgumentOutOfRangeException(nameof(minimumX));
            }

            if (minimumZ >= maximumZ)
            {
                throw new ArgumentOutOfRangeException(nameof(minimumZ));
            }

            MinimumX = minimumX;
            MaximumX = maximumX;
            MinimumZ = minimumZ;
            MaximumZ = maximumZ;
            GroundY = groundY;
        }

        public float MinimumX { get; }

        public float MaximumX { get; }

        public float MinimumZ { get; }

        public float MaximumZ { get; }

        public float GroundY { get; }

        public bool CanContain(CollisionRadius radius)
        {
            if (radius.IsValid == false)
            {
                return false;
            }

            float diameter = radius.Value * 2f;
            float width = MaximumX - MinimumX;
            float depth = MaximumZ - MinimumZ;

            return diameter <= width && diameter <= depth;
        }

        public bool Contains(
            Position3D position,
            CollisionRadius radius)
        {
            if (CanContain(radius) == false)
            {
                return false;
            }

            float minimumCenterX = MinimumX + radius.Value;
            float maximumCenterX = MaximumX - radius.Value;
            float minimumCenterZ = MinimumZ + radius.Value;
            float maximumCenterZ = MaximumZ - radius.Value;

            return position.X >= minimumCenterX
                && position.X <= maximumCenterX
                && position.Z >= minimumCenterZ
                && position.Z <= maximumCenterZ
                && position.Y == GroundY;
        }

        public Position3D Clamp(
            Position3D position,
            CollisionRadius radius)
        {
            if (CanContain(radius) == false)
            {
                throw new ArgumentOutOfRangeException(nameof(radius));
            }

            float minimumCenterX = MinimumX + radius.Value;
            float maximumCenterX = MaximumX - radius.Value;
            float minimumCenterZ = MinimumZ + radius.Value;
            float maximumCenterZ = MaximumZ - radius.Value;

            float x = ClampValue(position.X, minimumCenterX, maximumCenterX);
            float z = ClampValue(position.Z, minimumCenterZ, maximumCenterZ);

            return new Position3D(x, GroundY, z);
        }

        public bool Equals(ArenaBounds other)
        {
            return MinimumX == other.MinimumX
                && MaximumX == other.MaximumX
                && MinimumZ == other.MinimumZ
                && MaximumZ == other.MaximumZ
                && GroundY == other.GroundY;
        }

        public override bool Equals(object obj)
        {
            return obj is ArenaBounds other && Equals(other);
        }

        public override int GetHashCode()
        {
            return HashCode.Combine(
                MinimumX,
                MaximumX,
                MinimumZ,
                MaximumZ,
                GroundY);
        }

        public static bool operator ==(
            ArenaBounds left,
            ArenaBounds right)
        {
            return left.Equals(right);
        }

        public static bool operator !=(
            ArenaBounds left,
            ArenaBounds right)
        {
            return !left.Equals(right);
        }

        private static float ClampValue(
            float value,
            float minimum,
            float maximum)
        {
            if (value < minimum)
            {
                return minimum;
            }

            if (value > maximum)
            {
                return maximum;
            }

            return value;
        }

        private static bool IsFinite(float value)
        {
            return float.IsNaN(value) == false
                && float.IsInfinity(value) == false;
        }
    }
}
