using System;

namespace Game.Arena.Domain.Geometry
{
    public readonly struct CollisionRadius : IEquatable<CollisionRadius>
    {
        private CollisionRadius(Distance value)
        {
            Value = value;
        }

        public Distance Value { get; }

        public bool IsValid => Value.IsZero == false;

        public static CollisionRadius FromDistance(Distance value)
        {
            if (value.IsZero)
            {
                throw new ArgumentOutOfRangeException(nameof(value));
            }

            return new CollisionRadius(value);
        }

        public static CollisionRadius FromValue(float value)
        {
            return FromDistance(Distance.FromValue(value));
        }

        public bool Equals(CollisionRadius other)
        {
            return Value == other.Value;
        }

        public override bool Equals(object obj)
        {
            return obj is CollisionRadius other && Equals(other);
        }

        public override int GetHashCode()
        {
            return Value.GetHashCode();
        }

        public override string ToString()
        {
            return "CollisionRadius(" + Value.ToString() + ")";
        }

        public static bool operator ==(
            CollisionRadius left,
            CollisionRadius right)
        {
            return left.Equals(right);
        }

        public static bool operator !=(
            CollisionRadius left,
            CollisionRadius right)
        {
            return !left.Equals(right);
        }
    }
}
