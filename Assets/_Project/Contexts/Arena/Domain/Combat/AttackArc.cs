using System;

namespace Game.Arena.Domain.Combat
{
    public readonly struct AttackArc : IEquatable<AttackArc>
    {
        private const float FullDegrees = 360f;

        private AttackArc(float degrees)
        {
            Degrees = degrees;
        }

        public static AttackArc Full { get; } = new AttackArc(FullDegrees);

        public float Degrees { get; }

        public bool IsValid =>
            float.IsNaN(Degrees) == false
            && float.IsInfinity(Degrees) == false
            && Degrees > 0f
            && Degrees <= FullDegrees;

        public bool IsFull => Degrees >= FullDegrees;

        public float CosHalfAngle =>
            (float)Math.Cos((Degrees * 0.5f) * (Math.PI / 180d));

        public static AttackArc FromDegrees(float degrees)
        {
            AttackArc arc = new(degrees);

            if (arc.IsValid == false)
            {
                throw new ArgumentOutOfRangeException(nameof(degrees));
            }

            return arc;
        }

        public bool Equals(AttackArc other)
        {
            return Degrees.Equals(other.Degrees);
        }

        public override bool Equals(object obj)
        {
            return obj is AttackArc other && Equals(other);
        }

        public override int GetHashCode()
        {
            return Degrees.GetHashCode();
        }

        public override string ToString()
        {
            return $"{Degrees}°";
        }

        public static bool operator ==(AttackArc left, AttackArc right)
        {
            return left.Equals(right);
        }

        public static bool operator !=(AttackArc left, AttackArc right)
        {
            return left.Equals(right) == false;
        }
    }
}
