using System;

namespace Game.Arena.Domain.Interactions
{
    public readonly struct InteractionId : IEquatable<InteractionId>
    {
        private InteractionId(ulong value)
        {
            Value = value;
        }

        public static InteractionId None { get; } = new InteractionId(0UL);

        public ulong Value { get; }

        public bool IsNone => Value == 0UL;

        public InteractionId Next()
        {
            if (Value == ulong.MaxValue)
            {
                throw new InvalidOperationException("InteractionId range has been exhausted.");
            }

            return new InteractionId(Value + 1UL);
        }

        public bool Equals(InteractionId other)
        {
            return Value == other.Value;
        }

        public override bool Equals(object obj)
        {
            return obj is InteractionId other && Equals(other);
        }

        public override int GetHashCode()
        {
            return Value.GetHashCode();
        }

        public override string ToString()
        {
            if (this.IsNone)
            {
                return "InteractionId(None)";
            }

            return "InteractionId(" + Value.ToString() + ")";
        }

        public static bool operator ==(InteractionId left, InteractionId right)
        {
            return left.Equals(right);
        }

        public static bool operator !=(InteractionId left, InteractionId right)
        {
            return !left.Equals(right);
        }
    }
}
