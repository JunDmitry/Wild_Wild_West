using System;

namespace Game.Arena.Application.Input
{
    public readonly struct MovementAxes : IEquatable<MovementAxes>
    {
        public MovementAxes(float forward, float right)
        {
            if (IsFinite(forward) == false || IsFinite(right) == false)
            {
                throw new ArgumentOutOfRangeException();
            }

            Forward = forward;
            Right = right;
        }

        public static MovementAxes Zero { get; } = new MovementAxes(0f, 0f);

        public float Forward { get; }

        public float Right { get; }

        public bool IsZero => Forward == 0f && Right == 0f;

        public bool Equals(MovementAxes other)
        {
            return Forward.Equals(other.Forward) && Right.Equals(other.Right);
        }

        public override bool Equals(object obj)
        {
            return obj is MovementAxes other && Equals(other);
        }

        public override int GetHashCode()
        {
            return HashCode.Combine(Forward, Right);
        }

        public override string ToString()
        {
            return "MovementAxes(Forward: " + Forward + ", Right: " + Right + ")";
        }

        public static bool operator ==(MovementAxes left, MovementAxes right)
        {
            return left.Equals(right);
        }

        public static bool operator !=(MovementAxes left, MovementAxes right)
        {
            return !left.Equals(right);
        }

        private static bool IsFinite(float value)
        {
            return float.IsNaN(value) == false && float.IsInfinity(value) == false;
        }
    }
}
