using System;

namespace Game.Arena.Domain.Vitality
{
    public readonly struct Health : IEquatable<Health>
    {
        private readonly int _current;
        private readonly int _maximum;

        private Health(int current, int maximum)
        {
            _current = current;
            _maximum = maximum;
        }

        public int Current => _current;

        public int Maximum => _maximum;

        public bool IsDepleted => _current <= 0;

        public bool IsValid => _maximum > 0 && _current >= 0 && _current <= _maximum;

        public static Health Full(int maximum)
        {
            if (maximum <= 0)
            {
                throw new ArgumentOutOfRangeException(nameof(maximum));
            }

            return new Health(maximum, maximum);
        }

        public Health Reduce(DamageAmount damage)
        {
            if (IsValid == false)
            {
                throw new InvalidOperationException("Health is not initialized.");
            }

            int reduced = _current - damage.Points;

            if (reduced < 0)
            {
                reduced = 0;
            }

            return new Health(reduced, _maximum);
        }

        public bool Equals(Health other)
        {
            return _current == other._current && _maximum == other._maximum;
        }

        public override bool Equals(object obj)
        {
            return obj is Health other && Equals(other);
        }

        public override int GetHashCode()
        {
            return HashCode.Combine(_current, _maximum);
        }

        public override string ToString()
        {
            return "Health(" + _current.ToString() + "/" + _maximum.ToString() + ")";
        }

        public static bool operator ==(Health left, Health right)
        {
            return left.Equals(right);
        }

        public static bool operator !=(Health left, Health right)
        {
            return !left.Equals(right);
        }
    }
}
