using System;

namespace Game.Core.Model.Configs
{
    /// <summary>
    /// Represents the configuration of a wave.
    /// </summary>
    public readonly struct WaveConfig : IEquatable<WaveConfig>
    {
        /// <summary>
        /// Initializes a new instance of the <see cref="WaveConfig"/> struct.
        /// </summary>
        /// <param name="number">The wave number.</param>
        /// <param name="regularCount">The number of regular enemies in the wave.</param>
        /// <param name="hasBoss">A value indicating whether the wave contains a boss.</param>
        public WaveConfig(
            int number,
            int regularCount,
            bool hasBoss)
        {
            Number = number;
            RegularCount = regularCount;
            HasBoss = hasBoss;
        }

        /// <summary>
        /// Gets the wave number.
        /// </summary>
        public int Number { get; }

        /// <summary>
        /// Gets the number of regular enemies in the wave.
        /// </summary>
        public int RegularCount { get; }

        /// <summary>
        /// Gets a value indicating whether the wave contains a boss.
        /// </summary>
        public bool HasBoss { get; }

        /// <summary>
        /// Returns the hash code for the current wave configuration.
        /// </summary>
        /// <returns>A hash code for the current wave configuration.</returns>
        public override int GetHashCode()
        {
            return HashCode.Combine(
                Number,
                RegularCount,
                HasBoss);
        }

        /// <summary>
        /// Determines whether the current instance is equal to another object.
        /// </summary>
        /// <param name="obj">The object to compare with the current instance.</param>
        /// <returns><see langword="true"/> if <paramref name="obj"/> is a <see cref="WaveConfig"/> and is equal to the current instance; otherwise, <see langword="false"/>.</returns>
        public override bool Equals(object obj)
        {
            return obj is WaveConfig other && Equals(other);
        }

        /// <summary>
        /// Determines whether the current instance is equal to another <see cref="WaveConfig"/>.
        /// </summary>
        /// <param name="other">The <see cref="WaveConfig"/> to compare with the current instance.</param>
        /// <returns><see langword="true"/> if the current instance is equal to <paramref name="other"/>; otherwise, <see langword="false"/>.</returns>
        public bool Equals(WaveConfig other)
        {
            return Number == other.Number
                && RegularCount == other.RegularCount
                && HasBoss == other.HasBoss;
        }
    }
}
