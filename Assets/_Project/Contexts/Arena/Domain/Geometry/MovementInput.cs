using System;

namespace Game.Arena.Domain.Geometry
{
    public readonly struct MovementInput : IEquatable<MovementInput>
    {
        private MovementInput(Displacement3D vector)
        {
            Vector = vector;
        }

        public static MovementInput Zero { get; } = new MovementInput(Displacement3D.Zero);

        public Displacement3D Vector { get; }

        public bool IsZero => Vector.IsZero;
        public float Magnitude => Vector.Length;

        public static MovementInput FromVector(Displacement3D vector)
        {
            if (vector.Y != 0f)
            {
                throw new ArgumentOutOfRangeException(nameof(vector));
            }

            float length = vector.Length;

            if (length > GeometryTolerance.MovementInputMaximumAcceptedLength)
            {
                throw new ArgumentOutOfRangeException(nameof(vector));
            }

            if (length > 1f)
            {
                Direction3D direction = Direction3D.From(vector);
                Displacement3D normalized = direction * 1f;

                return new MovementInput(normalized);
            }

            return new MovementInput(vector);
        }

        public bool TryGetDirection(out Direction3D direction)
        {
            return Vector.TryToDirection(out direction);
        }

        public bool Equals(MovementInput other)
        {
            return Vector.Equals(other.Vector);
        }

        public override bool Equals(object obj)
        {
            return obj is MovementInput other && Equals(other);
        }

        public override int GetHashCode()
        {
            return Vector.GetHashCode();
        }

        public override string ToString()
        {
            return "MovementInput" + Vector.ToString();
        }

        public static bool operator ==(MovementInput left, MovementInput right)
        {
            return left.Equals(right);
        }

        public static bool operator !=(MovementInput left, MovementInput right)
        {
            return !left.Equals(right);
        }
    }
}
