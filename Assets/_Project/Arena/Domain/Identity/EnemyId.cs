using System;

namespace Game.Arena.Domain.Identity
{
    public readonly struct EnemyId : IEquatable<EnemyId>
    {
        private EnemyId(ulong value)
        {
            Value = value;
        }

        public static EnemyId None { get; } = new EnemyId(0UL);

        public ulong Value { get; }

        public bool IsNone => Value == 0UL;

        public static EnemyId FromValue(ulong value)
        {
            if (value == 0UL)
            {
                throw new ArgumentOutOfRangeException(nameof(value));
            }

            return new EnemyId(value);
        }

        public bool Equals(EnemyId other)
        {
            return Value == other.Value;
        }

        public override bool Equals(object obj)
        {
            return obj is EnemyId other && this.Equals(other);
        }

        public override int GetHashCode()
        {
            return Value.GetHashCode();
        }

        public override string ToString()
        {
            if (IsNone)
            {
                return "EnemyId(None)";
            }

            return "EnemyId(" + Value.ToString() + ")";
        }

        public static bool operator ==(EnemyId left, EnemyId right)
        {
            return left.Equals(right);
        }

        public static bool operator !=(EnemyId left, EnemyId right)
        {
            return !left.Equals(right);
        }
    }
}
