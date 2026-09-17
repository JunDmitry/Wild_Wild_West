using System;

namespace Game.Arena.Domain.Combat
{
    public readonly struct AttackId : IEquatable<AttackId>
    {
        private AttackId(ulong value)
        {
            Value = value;
        }

        public static AttackId None { get; } = new AttackId(0UL);

        public ulong Value { get; }

        public bool IsNone => Value == 0UL;

        public AttackId Next()
        {
            if (Value == ulong.MaxValue)
            {
                throw new InvalidOperationException("AttackId range has been exhausted.");
            }

            return new AttackId(Value + 1UL);
        }

        public bool Equals(AttackId other)
        {
            return Value == other.Value;
        }

        public override bool Equals(object obj)
        {
            return obj is AttackId other && Equals(other);
        }

        public override int GetHashCode()
        {
            return Value.GetHashCode();
        }

        public override string ToString()
        {
            if (IsNone)
            {
                return "AttackId(None)";
            }

            return "AttackId(" + Value.ToString() + ")";
        }

        public static bool operator ==(AttackId left, AttackId right)
        {
            return left.Equals(right);
        }

        public static bool operator !=(AttackId left, AttackId right)
        {
            return !left.Equals(right);
        }
    }
}
