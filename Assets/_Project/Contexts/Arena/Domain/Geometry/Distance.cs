using System;

namespace Game.Arena.Domain.Geometry
{
    public readonly struct Distance : IEquatable<Distance>, IComparable<Distance>
    {
        private Distance(float value)
        {
            Value = value;
        }

        public static Distance Zero { get; } = new Distance(0f);

        public float Value { get; }

        public bool IsZero => Value <= 0f;

        public static Distance FromValue(float value)
        {
            if (float.IsNaN(value))
            {
                throw new ArgumentOutOfRangeException(nameof(value));
            }

            if (float.IsInfinity(value))
            {
                throw new ArgumentOutOfRangeException(nameof(value));
            }

            if (value < 0f)
            {
                throw new ArgumentOutOfRangeException(nameof(value));
            }

            return new Distance(value);
        }

        public Distance Scaled(float factor)
        {
            if (float.IsNaN(factor))
            {
                throw new ArgumentOutOfRangeException(nameof(factor));
            }

            if (float.IsInfinity(factor))
            {
                throw new ArgumentOutOfRangeException(nameof(factor));
            }

            if (factor < 0f)
            {
                throw new ArgumentOutOfRangeException(nameof(factor));
            }

            return FromValue(Value * factor);
        }

        public int CompareTo(Distance other)
        {
            return Value.CompareTo(other.Value);
        }

        public bool Equals(Distance other)
        {
            return Value == other.Value;
        }

        public override bool Equals(object obj)
        {
            return obj is Distance other && Equals(other);
        }

        public override int GetHashCode()
        {
            return Value.GetHashCode();
        }

        public override string ToString()
        {
            return "Distance(" + Value.ToString() + ")";
        }

        public static bool operator ==(Distance left, Distance right)
        {
            return left.Equals(right);
        }

        public static bool operator !=(Distance left, Distance right)
        {
            return !left.Equals(right);
        }

        public static bool operator <(Distance left, Distance right)
        {
            return left.CompareTo(right) < 0;
        }

        public static bool operator <=(Distance left, Distance right)
        {
            return left.CompareTo(right) <= 0;
        }

        public static bool operator >(Distance left, Distance right)
        {
            return left.CompareTo(right) > 0;
        }

        public static bool operator >=(Distance left, Distance right)
        {
            return left.CompareTo(right) >= 0;
        }
    }
}
