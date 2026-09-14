using System;

namespace Game.Arena.Domain.Movement
{
    public readonly struct MovementSpeed : IEquatable<MovementSpeed>
    {
        private MovementSpeed(float unitsPerSecond)
        {
            UnitsPerSecond = unitsPerSecond;
        }

        public float UnitsPerSecond { get; }

        public bool IsValid => UnitsPerSecond > 0f && float.IsNaN(UnitsPerSecond) == false && float.IsInfinity(UnitsPerSecond) == false;

        public static MovementSpeed FromUnitsPerSecond(float unitsPerSecond)
        {
            if (float.IsNaN(unitsPerSecond))
            {
                throw new ArgumentOutOfRangeException(nameof(unitsPerSecond));
            }

            if (float.IsInfinity(unitsPerSecond))
            {
                throw new ArgumentOutOfRangeException(nameof(unitsPerSecond));
            }

            if (unitsPerSecond <= 0f)
            {
                throw new ArgumentOutOfRangeException(nameof(unitsPerSecond));
            }

            return new MovementSpeed(unitsPerSecond);
        }

        public bool Equals(MovementSpeed other)
        {
            return UnitsPerSecond == other.UnitsPerSecond;
        }

        public override bool Equals(object obj)
        {
            return obj is MovementSpeed other && Equals(other);
        }

        public override int GetHashCode()
        {
            return UnitsPerSecond.GetHashCode();
        }

        public override string ToString()
        {
            return "MovementSpeed("
                + UnitsPerSecond.ToString()
                + ")";
        }

        public static bool operator ==(
            MovementSpeed left,
            MovementSpeed right)
        {
            return left.Equals(right);
        }

        public static bool operator !=(
            MovementSpeed left,
            MovementSpeed right)
        {
            return !left.Equals(right);
        }
    }
}
