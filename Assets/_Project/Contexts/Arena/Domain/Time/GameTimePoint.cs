using System;

namespace Game.Arena.Domain.Time
{
    public readonly struct GameTimePoint : IEquatable<GameTimePoint>, IComparable<GameTimePoint>
    {
        public GameTimePoint(double seconds)
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

        public int CompareTo(GameTimePoint other)
        {
            return Seconds.CompareTo(other.Seconds);
        }

        public bool Equals(GameTimePoint other)
        {
            return Seconds.Equals(other.Seconds);
        }

        public override bool Equals(object obj)
        {
            return obj is GameTimePoint other && Equals(other);
        }

        public override int GetHashCode()
        {
            return Seconds.GetHashCode();
        }

        public static GameTimePoint operator +(GameTimePoint point, GameDuration duration)
        {
            return new GameTimePoint(point.Seconds + duration.Seconds);
        }

        public static GameDuration operator -(GameTimePoint left, GameTimePoint right)
        {
            if (left < right)
            {
                throw new InvalidOperationException();
            }

            return new GameDuration(left.Seconds - right.Seconds);
        }

        public static bool operator ==(GameTimePoint left, GameTimePoint right)
        {
            return left.Equals(right);
        }

        public static bool operator !=(GameTimePoint left, GameTimePoint right)
        {
            return !left.Equals(right);
        }

        public static bool operator <(GameTimePoint left, GameTimePoint right)
        {
            return left.CompareTo(right) < 0;
        }

        public static bool operator <=(GameTimePoint left, GameTimePoint right)
        {
            return left.CompareTo(right) <= 0;
        }

        public static bool operator >(GameTimePoint left, GameTimePoint right)
        {
            return left.CompareTo(right) > 0;
        }

        public static bool operator >=(GameTimePoint left, GameTimePoint right)
        {
            return left.CompareTo(right) >= 0;
        }
    }
}
