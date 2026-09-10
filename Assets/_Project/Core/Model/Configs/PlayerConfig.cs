using System;
using Game.Core.Model.Entities;

namespace Game.Core.Model.Configs
{
    /// <summary>
    /// Represents the configuration of the player.
    /// </summary>
    public readonly struct PlayerConfig : IEquatable<PlayerConfig>
    {
        /// <summary>
        /// Initializes a new instance of the <see cref="PlayerConfig"/> struct.
        /// </summary>
        /// <param name="maxHealth">The maximum health of the player.</param>
        /// <param name="startPosition">The starting position of the player.</param>
        /// <param name="startRangedReadyTime">The initial time until the ranged weapon is ready.</param>
        /// <param name="startMeleeReadyTime">The initial time until the melee weapon is ready.</param>
        public PlayerConfig(
            int maxHealth,
            Position3D startPosition,
            float startRangedReadyTime,
            float startMeleeReadyTime,
            float moveSpeed)
        {
            MaxHealth = maxHealth;
            StartPosition = startPosition;
            StartRangedReadyTime = startRangedReadyTime;
            StartMeleeReadyTime = startMeleeReadyTime;
            MoveSpeed = moveSpeed;
        }

        /// <summary>
        /// Gets the maximum health of the player.
        /// </summary>
        public int MaxHealth { get; }

        /// <summary>
        /// Gets the starting position of the player.
        /// </summary>
        public Position3D StartPosition { get; }

        /// <summary>
        /// Gets the initial time until the ranged weapon is ready.
        /// </summary>
        public float StartRangedReadyTime { get; }

        /// <summary>
        /// Gets the initial time until the melee weapon is ready.
        /// </summary>
        public float StartMeleeReadyTime { get; }

        public float MoveSpeed { get; }

        /// <summary>
        /// Returns the hash code for the current player configuration.
        /// </summary>
        /// <returns>A hash code for the current player configuration.</returns>
        public override int GetHashCode()
        {
            return HashCode.Combine(
                MaxHealth,
                StartPosition,
                StartRangedReadyTime,
                StartMeleeReadyTime,
                MoveSpeed);
        }

        /// <summary>
        /// Determines whether the current instance is equal to another object.
        /// </summary>
        /// <param name="obj">The object to compare with the current instance.</param>
        /// <returns><see langword="true"/> if <paramref name="obj"/> is a <see cref="PlayerConfig"/> and is equal to the current instance; otherwise, <see langword="false"/>.</returns>
        public override bool Equals(object obj)
        {
            return obj is PlayerConfig other && Equals(other);
        }

        /// <summary>
        /// Determines whether the current instance is equal to another <see cref="PlayerConfig"/>.
        /// </summary>
        /// <param name="other">The <see cref="PlayerConfig"/> to compare with the current instance.</param>
        /// <returns><see langword="true"/> if the current instance is equal to <paramref name="other"/>; otherwise, <see langword="false"/>.</returns>
        public bool Equals(PlayerConfig other)
        {
            return MaxHealth == other.MaxHealth
                && StartPosition.Equals(other.StartPosition)
                && StartRangedReadyTime == other.StartRangedReadyTime
                && StartMeleeReadyTime == other.StartMeleeReadyTime
                && MoveSpeed == other.MoveSpeed;
        }
    }
}
