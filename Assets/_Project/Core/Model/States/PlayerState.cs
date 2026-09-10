using System;
using Game.Core.Model.Entities;
using Game.Core.Model.Enums;

namespace Game.Core.Model.States
{
    /// <summary>
    /// Represents the complete state of a player at a given moment, including identity, position, health, selected weapon, and cooldown timers.
    /// </summary>
    public readonly struct PlayerState : IEquatable<PlayerState>
    {
        /// <summary>
        /// Initializes a new instance of the <see cref="PlayerState"/> struct with the specified player data.
        /// </summary>
        /// <param name="id">The unique identifier of the player entity.</param>
        /// <param name="position">The current 3D position of the player.</param>
        /// <param name="currentHealth">The player's current health value.</param>
        /// <param name="maxHealth">The maximum possible health value for the player.</param>
        /// <param name="selectedWeapon">The weapon currently equipped by the player.</param>
        /// <param name="rangedReadyTime">The time remaining until the ranged attack is ready (or 0 if ready).</param>
        /// <param name="meleeReadyTime">The time remaining until the melee attack is ready (or 0 if ready).</param>
        public PlayerState(
            EntityId id,
            Position3D position,
            float currentHealth,
            float maxHealth,
            WeaponKind selectedWeapon,
            float rangedReadyTime,
            float meleeReadyTime)
        {
            Id = id;
            Position = position;
            CurrentHealth = currentHealth;
            MaxHealth = maxHealth;
            SelectedWeapon = selectedWeapon;
            RangedReadyTime = rangedReadyTime;
            MeleeReadyTime = meleeReadyTime;
        }

        /// <summary>
        /// Gets the unique identifier of the player entity.
        /// </summary>
        public EntityId Id { get; }

        /// <summary>
        /// Gets the current 3D position of the player.
        /// </summary>
        public Position3D Position { get; }

        /// <summary>
        /// Gets the player's current health value.
        /// </summary>
        public float CurrentHealth { get; }

        /// <summary>
        /// Gets the maximum possible health value for the player.
        /// </summary>
        public float MaxHealth { get; }

        /// <summary>
        /// Gets the weapon currently equipped by the player.
        /// </summary>
        public WeaponKind SelectedWeapon { get; }

        /// <summary>
        /// Gets the time remaining until the ranged attack is ready. A value of 0 indicates the attack is available.
        /// </summary>
        public float RangedReadyTime { get; }

        /// <summary>
        /// Gets the time remaining until the melee attack is ready. A value of 0 indicates the attack is available.
        /// </summary>
        public float MeleeReadyTime { get; }

        /// <summary>
        /// Returns the hash code for this <see cref="PlayerState"/> instance.
        /// </summary>
        /// <returns>A hash code computed from all state properties.</returns>
        public override int GetHashCode()
        {
            return HashCode.Combine(
                Id,
                Position,
                CurrentHealth,
                MaxHealth,
                SelectedWeapon,
                RangedReadyTime,
                MeleeReadyTime);
        }

        /// <summary>
        /// Determines whether the specified object is equal to this instance.
        /// </summary>
        /// <param name="obj">The object to compare with this instance.</param>
        /// <returns>True if the objects represent the same player state; otherwise, false.</returns>
        public override bool Equals(object obj)
        {
            return obj is PlayerState other && Equals(other);
        }

        /// <summary>
        /// Determines whether the current <see cref="PlayerState"/> is equal to another <see cref="PlayerState"/>.
        /// Equality is based on all properties matching exactly.
        /// </summary>
        /// <param name="other">The <see cref="PlayerState"/> to compare with this instance.</param>
        /// <returns>True if all properties match; otherwise, false.</returns>
        public bool Equals(PlayerState other)
        {
            return Id.Equals(other.Id)
                && Position.Equals(other.Position)
                && CurrentHealth == other.CurrentHealth
                && MaxHealth == other.MaxHealth
                && SelectedWeapon == other.SelectedWeapon
                && RangedReadyTime == other.RangedReadyTime
                && MeleeReadyTime == other.MeleeReadyTime;
        }
    }
}
