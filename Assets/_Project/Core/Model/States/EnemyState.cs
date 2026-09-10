using System;
using Game.Core.Model.Entities;
using Game.Core.Model.Enums;

namespace Game.Core.Model.States
{
    /// <summary>
    /// Represents the complete state of an enemy at a given moment, including identity, type, position, health, and attack cooldown.
    /// </summary>
    public readonly struct EnemyState : IEquatable<EnemyState>
    {
        /// <summary>
        /// Initializes a new instance of the <see cref="EnemyState"/> struct with the specified enemy data.
        /// </summary>
        /// <param name="id">The unique identifier of the enemy entity.</param>
        /// <param name="kind">The type/kind of the enemy.</param>
        /// <param name="position">The current 3D position of the enemy.</param>
        /// <param name="currentHealth">The enemy's current health value.</param>
        /// <param name="maxHealth">The maximum possible health value for the enemy.</param>
        /// <param name="attackReadyTime">The time remaining until the enemy's attack is ready (or 0 if ready).</param>
        public EnemyState(
            EntityId id,
            EnemyKind kind,
            Position3D position,
            float currentHealth,
            float maxHealth,
            float attackReadyTime)
        {
            Id = id;
            Kind = kind;
            Position = position;
            CurrentHealth = currentHealth;
            MaxHealth = maxHealth;
            AttackReadyTime = attackReadyTime;
        }

        /// <summary>
        /// Gets the unique identifier of the enemy entity.
        /// </summary>
        public EntityId Id { get; }

        /// <summary>
        /// Gets the type/kind of the enemy.
        /// </summary>
        public EnemyKind Kind { get; }

        /// <summary>
        /// Gets the current 3D position of the enemy.
        /// </summary>
        public Position3D Position { get; }

        /// <summary>
        /// Gets the enemy's current health value.
        /// </summary>
        public float CurrentHealth { get; }

        /// <summary>
        /// Gets the maximum possible health value for the enemy.
        /// </summary>
        public float MaxHealth { get; }

        /// <summary>
        /// Gets the time remaining until the enemy's attack is ready. A value of 0 indicates the attack is available.
        /// </summary>
        public float AttackReadyTime { get; }

        /// <summary>
        /// Returns the hash code for this <see cref="EnemyState"/> instance.
        /// </summary>
        /// <returns>A hash code computed from all state properties.</returns>
        public override int GetHashCode()
        {
            return HashCode.Combine(
                Id,
                Kind,
                Position,
                CurrentHealth,
                MaxHealth,
                AttackReadyTime);
        }

        /// <summary>
        /// Determines whether the specified object is equal to this instance.
        /// </summary>
        /// <param name="obj">The object to compare with this instance.</param>
        /// <returns>True if the objects represent the same enemy state; otherwise, false.</returns>
        public override bool Equals(object obj)
        {
            return obj is EnemyState other && Equals(other);
        }

        /// <summary>
        /// Determines whether the current <see cref="EnemyState"/> is equal to another <see cref="EnemyState"/>.
        /// Equality is based on all properties matching exactly.
        /// </summary>
        /// <param name="other">The <see cref="EnemyState"/> to compare with this instance.</param>
        /// <returns>True if all properties match; otherwise, false.</returns>
        public bool Equals(EnemyState other)
        {
            return Id.Equals(other.Id)
                && Kind == other.Kind
                && Position.Equals(other.Position)
                && CurrentHealth == other.CurrentHealth
                && MaxHealth == other.MaxHealth
                && AttackReadyTime == other.AttackReadyTime;
        }
    }
}
