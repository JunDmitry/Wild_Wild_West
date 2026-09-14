using System;

namespace Game.Arena.Domain.Concurrency
{
    public readonly struct AggregateRevision : IEquatable<AggregateRevision>, IComparable<AggregateRevision>
    {
        private AggregateRevision(ulong value)
        {
            Value = value;
        }

        public static AggregateRevision Initial { get; } = new AggregateRevision(0UL);

        public ulong Value { get; }

        public AggregateRevision Next()
        {
            if (Value == ulong.MaxValue)
            {
                throw new InvalidOperationException("AggregateRevision range has been exhausted.");
            }

            return new AggregateRevision(Value + 1UL);
        }

        public int CompareTo(AggregateRevision other)
        {
            return Value.CompareTo(other.Value);
        }

        public bool Equals(AggregateRevision other)
        {
            return Value == other.Value;
        }

        public override bool Equals(object obj)
        {
            return obj is AggregateRevision other && Equals(other);
        }

        public override int GetHashCode()
        {
            return Value.GetHashCode();
        }

        public override string ToString()
        {
            return "AggregateRevision(" + Value.ToString() + ")";
        }

        public static bool operator ==(AggregateRevision left, AggregateRevision right)
        {
            return left.Equals(right);
        }

        public static bool operator !=(AggregateRevision left, AggregateRevision right)
        {
            return !left.Equals(right);
        }

        public static bool operator <(AggregateRevision left, AggregateRevision right)
        {
            return left.CompareTo(right) < 0;
        }

        public static bool operator <=(AggregateRevision left, AggregateRevision right)
        {
            return left.CompareTo(right) <= 0;
        }

        public static bool operator >(AggregateRevision left, AggregateRevision right)
        {
            return left.CompareTo(right) > 0;
        }

        public static bool operator >=(AggregateRevision left, AggregateRevision right)
        {
            return left.CompareTo(right) >= 0;
        }
    }
}
