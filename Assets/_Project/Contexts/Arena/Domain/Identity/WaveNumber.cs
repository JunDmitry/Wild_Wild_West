using System;

namespace Game.Arena.Domain.Identity
{
    public readonly struct WaveNumber : IEquatable<WaveNumber>, IComparable<WaveNumber>
    {
        private WaveNumber(int value)
        {
            Value = value;
        }

        public static WaveNumber First { get; } = new WaveNumber(1);

        public int Value { get; }

        public static WaveNumber FromValue(int value)
        {
            if (value < 1)
            {
                throw new ArgumentOutOfRangeException(nameof(value));
            }

            return new WaveNumber(value);
        }

        public WaveNumber Next()
        {
            if (Value == int.MaxValue)
            {
                throw new InvalidOperationException("WaveNumber range has been exhausted.");
            }

            return new WaveNumber(Value + 1);
        }

        public int CompareTo(WaveNumber other)
        {
            return Value.CompareTo(other.Value);
        }

        public bool Equals(WaveNumber other)
        {
            return Value == other.Value;
        }

        public override bool Equals(object obj)
        {
            return obj is WaveNumber other && this.Equals(other);
        }

        public override int GetHashCode()
        {
            return Value.GetHashCode();
        }

        public override string ToString()
        {
            return "WaveNumber(" + Value.ToString() + ")";
        }

        public static bool operator ==(WaveNumber left, WaveNumber right)
        {
            return left.Equals(right);
        }

        public static bool operator !=(WaveNumber left, WaveNumber right)
        {
            return !left.Equals(right);
        }

        public static bool operator <(WaveNumber left, WaveNumber right)
        {
            return left.CompareTo(right) < 0;
        }

        public static bool operator <=(WaveNumber left, WaveNumber right)
        {
            return left.CompareTo(right) <= 0;
        }

        public static bool operator >(WaveNumber left, WaveNumber right)
        {
            return left.CompareTo(right) > 0;
        }

        public static bool operator >=(WaveNumber left, WaveNumber right)
        {
            return left.CompareTo(right) >= 0;
        }
    }
}
