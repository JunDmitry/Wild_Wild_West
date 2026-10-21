using System;

namespace Game.Arena.Domain.Time
{
    public readonly struct GameDuration : IEquatable<GameDuration>
    {
        public GameDuration(double seconds)
        {
            if (double.IsNaN(seconds))
            {
                throw new ArgumentOutOfRangeException(nameof(seconds));
            }

            if (double.IsInfinity(seconds))
            {
                throw new ArgumentOutOfRangeException(nameof(seconds));
            }

            if (seconds < 0d)
            {
                throw new ArgumentOutOfRangeException(nameof(seconds));
            }

            Seconds = seconds;
        }

        public double Seconds { get; }

        public bool Equals(GameDuration other)
        {
            return Seconds.Equals(other.Seconds);
        }

        public override bool Equals(object obj)
        {
            return obj is GameDuration other && Equals(other);
        }

        public override int GetHashCode()
        {
            return Seconds.GetHashCode();
        }

        public static bool operator ==(GameDuration left, GameDuration right)
        {
            return left.Equals(right);
        }

        public static bool operator !=(GameDuration left, GameDuration right)
        {
            return !left.Equals(right);
        }
    }
}
