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

        public BoundedMovement PermittedMovement(
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

            if (direction.Y != 0f)
            {
                throw new ArgumentException("Bounded movement is planar.", nameof(direction));
            }

            float radiusValue = radius.Value.Value;
            float minimumCenterX = MinimumX + radiusValue;
            float maximumCenterX = MaximumX - radiusValue;
            float minimumCenterZ = MinimumZ + radiusValue;
            float maximumCenterZ = MaximumZ - radiusValue;

            float requestedValue = requested.Value;

            float availableX = AvailableAlongAxis(
                from.X,
                direction.X,
                minimumCenterX,
                maximumCenterX);

            float availableZ = AvailableAlongAxis(
                from.Z,
                direction.Z,
                minimumCenterZ,
                maximumCenterZ);

            float firstContact = Math.Min(availableX, availableZ);

            if (requestedValue <= firstContact)
            {
                return new BoundedMovement(direction, requested);
            }

            Distance approachDistance = Distance.FromValue(Math.Min(requestedValue, firstContact));
            Position3D approach = from.MovedAlong(direction, approachDistance);
            float remaining = requestedValue - approachDistance.Value;

            float slideX = 0f;
            float slideZ = 0f;

            if (availableX < availableZ)
            {
                slideZ = SlideOffset(
                    approach.Z,
                    direction.Z,
                    minimumCenterZ,
                    maximumCenterZ,
                    remaining * Math.Abs(direction.Z));
            }
            else if (availableZ < availableX)
            {
                slideX = SlideOffset(
                    approach.X,
                    direction.X,
                    minimumCenterX,
                    maximumCenterX,
                    remaining * Math.Abs(direction.X));
            }

            Displacement3D net = new(
                (approach.X - from.X) + slideX,
                0f,
                (approach.Z - from.Z) + slideZ);

            if (net.Length <= GeometryTolerance.MinimumDirectionLength)
            {
                return new BoundedMovement(direction, Distance.Zero);
            }

            Direction3D netDirection = Direction3D.From(net);

            return new BoundedMovement(netDirection, Distance.FromValue(net.Length));
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

        private static float AvailableAlongAxis(
            float axisPosition,
            float axisDirection,
            float minimum,
            float maximum)
        {
            if (axisDirection > 0f)
            {
                return Math.Max(0f, (maximum - axisPosition) / axisDirection);
            }

            if (axisDirection < 0f)
            {
                return Math.Max(0f, (minimum - axisPosition) / axisDirection);
            }

            return float.PositiveInfinity;
        }

        private static float SlideOffset(
            float axisPosition,
            float axisDirection,
            float minimum,
            float maximum,
            float wanted)
        {
            if (axisDirection > 0f)
            {
                return Math.Min(wanted, Math.Max(0f, maximum - axisPosition));
            }

            if (axisDirection < 0f)
            {
                return -Math.Min(wanted, Math.Max(0f, axisPosition - minimum));
            }

            return 0f;
        }

        private static bool IsFinite(float value)
        {
            return float.IsNaN(value) == false
                && float.IsInfinity(value) == false;
        }
    }
}
