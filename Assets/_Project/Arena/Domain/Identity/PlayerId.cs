using System;

namespace Game.Arena.Domain.Identity
{
    public readonly struct PlayerId : IEquatable<PlayerId>
    {
        private PlayerId(ulong value)
        {
            Value = value;
        }

        public static PlayerId None { get; } = new PlayerId(0UL);

        public ulong Value { get; }

        public bool IsNone => Value == 0UL;

        public static PlayerId FromValue(ulong value)
        {
            if (value == 0UL)
            {
                throw new ArgumentOutOfRangeException(nameof(value));
            }

            return new PlayerId(value);
        }

        public bool Equals(PlayerId other)
        {
            return Value == other.Value;
        }

        public override bool Equals(object obj)
        {
            return obj is PlayerId other && Equals(other);
        }

        public override int GetHashCode()
        {
            return Value.GetHashCode();
        }

        public override string ToString()
        {
            if (IsNone)
            {
                return "PlayerId(None)";
            }

            return "PlayerId(" + Value.ToString() + ")";
        }

        public static bool operator ==(PlayerId left, PlayerId right)
        {
            return left.Equals(right);
        }

        public static bool operator !=(PlayerId left, PlayerId right)
        {
            return !left.Equals(right);
        }
    }
}
