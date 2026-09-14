using System;

namespace Game.Arena.Domain.Identity
{
    public readonly struct ArenaRunId : IEquatable<ArenaRunId>
    {
        private ArenaRunId(ulong value)
        {
            Value = value;
        }

        public static ArenaRunId None { get; } = new ArenaRunId(0UL);

        public ulong Value { get; }

        public bool IsNone => Value == 0;

        public static ArenaRunId FromValue(ulong value)
        {
            if (value == 0UL)
            {
                throw new ArgumentOutOfRangeException(nameof(value));
            }

            return new ArenaRunId(value);
        }

        public bool Equals(ArenaRunId other)
        {
            return Value == other.Value;
        }

        public override bool Equals(object obj)
        {
            return obj is ArenaRunId other && Equals(other);
        }

        public override int GetHashCode()
        {
            return Value.GetHashCode();
        }

        public override string ToString()
        {
            if (IsNone)
            {
                return "ArenaRunId(None)";
            }

            return "ArenaRunId(" + Value.ToString() + ")";
        }

        public static bool operator ==(ArenaRunId left, ArenaRunId right)
        {
            return left.Equals(right);
        }

        public static bool operator !=(ArenaRunId left, ArenaRunId right)
        {
            return !left.Equals(right);
        }
    }
}
