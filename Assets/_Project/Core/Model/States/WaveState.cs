using System;
using Game.Core.Model.Enums;

namespace Game.Core.Model.States
{
    /// <summary>
    /// Represents an immutable snapshot of the current wave state.
    /// </summary>
    public readonly struct WaveState : IEquatable<WaveState>
    {
        /// <summary>
        /// Initializes a new instance of the <see cref="WaveState"/> struct.
        /// </summary>
        /// <param name="number">The wave number.</param>
        /// <param name="phase">The current phase of the wave.</param>
        /// <param name="regularToSpawn">The number of regular enemies remaining to spawn.</param>
        /// <param name="regularAlive">The number of regular enemies currently alive.</param>
        /// <param name="bossStatus"></param>
        public WaveState(
            int number,
            WavePhase phase,
            int regularToSpawn,
            int regularAlive,
            BossStatus bossStatus)
        {
            Number = number;
            Phase = phase;
            RegularToSpawn = regularToSpawn;
            RegularAlive = regularAlive;
            BossStatus = bossStatus;
        }

        /// <summary>
        /// Gets the wave number.
        /// </summary>
        public int Number { get; }

        /// <summary>
        /// Gets the current phase of the wave.
        /// </summary>
        public WavePhase Phase { get; }

        /// <summary>
        /// Gets the number of regular enemies remaining to spawn.
        /// </summary>
        public int RegularToSpawn { get; }

        /// <summary>
        /// Gets the number of regular enemies currently alive.
        /// </summary>
        public int RegularAlive { get; }

        public BossStatus BossStatus { get; }

        /// <summary>
        /// Returns the hash code for the current wave state.
        /// </summary>
        /// <returns>A hash code for the current wave state.</returns>
        public override int GetHashCode()
        {
            return HashCode.Combine(
                Number,
                Phase,
                RegularToSpawn,
                RegularAlive,
                BossStatus);
        }

        /// <summary>
        /// Determines whether the current instance is equal to another object.
        /// </summary>
        /// <param name="obj">The object to compare with the current instance.</param>
        /// <returns><see langword="true"/> if <paramref name="obj"/> is a <see cref="WaveState"/> and is equal to the current instance; otherwise, <see langword="false"/>.</returns>
        public override bool Equals(object obj)
        {
            return obj is WaveState other && Equals(other);
        }

        /// <summary>
        /// Determines whether the current instance is equal to another <see cref="WaveState"/>.
        /// </summary>
        /// <param name="other">The <see cref="WaveState"/> to compare with the current instance.</param>
        /// <returns><see langword="true"/> if the current instance is equal to <paramref name="other"/>; otherwise, <see langword="false"/>.</returns>
        public bool Equals(WaveState other)
        {
            return Number == other.Number
                && Phase == other.Phase
                && RegularToSpawn == other.RegularToSpawn
                && RegularAlive == other.RegularAlive
                && BossStatus == other.BossStatus;
        }
    }
}
