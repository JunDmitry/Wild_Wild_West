using System;

namespace Game.Arena.Domain.Vitality
{
    public readonly struct DamageAmount : IEquatable<DamageAmount>, IComparable<DamageAmount>
    {
        private readonly int _points;

        private DamageAmount(int points)
        {
            _points = points;
        }

        public int Points => _points;

        public static DamageAmount FromPoints(int points)
        {
            if (points <= 0)
            {
                throw new ArgumentOutOfRangeException(nameof(points));
            }

            return new DamageAmount(points);
        }

        public int CompareTo(DamageAmount other)
        {
            return _points.CompareTo(other._points);
        }

        public bool Equals(DamageAmount other)
        {
            return _points == other._points;
        }

        public override bool Equals(object obj)
        {
            return obj is DamageAmount other && Equals(other);
        }

        public override int GetHashCode()
        {
            return _points.GetHashCode();
        }

        public override string ToString()
        {
            return "DamageAmount(" + _points.ToString() + ")";
        }

        public static bool operator ==(DamageAmount left, DamageAmount right)
        {
            return left.Equals(right);
        }

        public static bool operator !=(DamageAmount left, DamageAmount right)
        {
            return !left.Equals(right);
        }

        public static bool operator <(DamageAmount left, DamageAmount right)
        {
            return left.CompareTo(right) < 0;
        }

        public static bool operator <=(DamageAmount left, DamageAmount right)
        {
            return left.CompareTo(right) <= 0;
        }

        public static bool operator >(DamageAmount left, DamageAmount right)
        {
            return left.CompareTo(right) > 0;
        }

        public static bool operator >=(DamageAmount left, DamageAmount right)
        {
            return left.CompareTo(right) >= 0;
        }
    }
}
