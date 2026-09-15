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

            float diameter = radius.Value.Value * 2f;
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

            float radiusValue = radius.Value.Value;
            float tolerance = GeometryTolerance.BoundsTolerance;
            float minimumCenterX = MinimumX + radiusValue - tolerance;
            float maximumCenterX = MaximumX - radiusValue + tolerance;
            float minimumCenterZ = MinimumZ + radiusValue - tolerance;
            float maximumCenterZ = MaximumZ - radiusValue + tolerance;

            return position.X >= minimumCenterX
                && position.X <= maximumCenterX
                && position.Z >= minimumCenterZ
                && position.Z <= maximumCenterZ
                && position.Y == GroundY;
        }

        public Distance PermittedTravel(
            Position3D from,
            Direction3D direction,
            CollisionRadius radius,
            Distance requested)
        {
            if (CanContain(radius) == false)
            {
                throw new ArgumentOutOfRangeException(nameof(radius));
            }

            if (direction.IsValid == false)
            {
                throw new ArgumentException("Direction is invalid.", nameof(direction));
            }

            float radiusValue = radius.Value.Value;
            float permitted = requested.Value;

            permitted = LimitAlongAxis(
                permitted,
                from.X,
                direction.X,
                MinimumX + radiusValue,
                MaximumX - radiusValue);

            permitted = LimitAlongAxis(
                permitted,
                from.Z,
                direction.Z,
                MinimumZ + radiusValue,
                MaximumZ - radiusValue);

            if (permitted < 0f)
            {
                permitted = 0f;
            }

            return Distance.FromValue(permitted);
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

        private static float LimitAlongAxis(
            float distance,
            float axisPosition,
            float axisDirection,
            float minimum,
            float maximum)
        {
            if (axisDirection > 0f)
            {
                float available = (maximum - axisPosition) / axisDirection;

                if (available < distance)
                {
                    return available;
                }

                return distance;
            }

            if (axisDirection < 0f)
            {
                float available = (minimum - axisPosition) / axisDirection;

                if (available < distance)
                {
                    return available;
                }

                return distance;
            }

            return distance;
        }

        private static bool IsFinite(float value)
        {
            return float.IsNaN(value) == false
                && float.IsInfinity(value) == false;
        }
    }
}
