using System;

namespace Game.Arena.Domain.Geometry
{
    public readonly struct CollisionRadius : IEquatable<CollisionRadius>
    {
        private CollisionRadius(float value)
        {
            Value = value;
        }

        public float Value { get; }

        public bool IsValid => Value > 0f && float.IsNaN(Value) == false && float.IsInfinity(Value) == false;

        public static CollisionRadius FromValue(float value)
        {
            if (float.IsNaN(value))
            {
                throw new ArgumentOutOfRangeException(nameof(value));
            }

            if (float.IsInfinity(value))
            {
                throw new ArgumentOutOfRangeException(nameof(value));
            }

            if (value <= 0f)
            {
                throw new ArgumentOutOfRangeException(nameof(value));
            }

            return new CollisionRadius(value);
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
