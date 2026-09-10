using System;
using Game.Core.Model.Enums;

namespace Game.Core.Model.Configs
{
    /// <summary>
    /// Represents the configuration of an enemy.
    /// </summary>
    public readonly struct EnemyConfig : IEquatable<EnemyConfig>
    {
        /// <summary>
        /// Initializes a new instance of the <see cref="EnemyConfig"/> struct.
        /// </summary>
        /// <param name="kind">The kind of the enemy.</param>
        /// <param name="maxHealth">The maximum health of the enemy.</param>
        public EnemyConfig(
            EnemyKind kind,
            int maxHealth,
            float moveSpeed,
            float attackDamage,
            float attackRange,
            float attackCooldown)
        {
            Kind = kind;
            MaxHealth = maxHealth;
            MoveSpeed = moveSpeed;
            AttackDamage = attackDamage;
            AttackRange = attackRange;
            AttackCooldown = attackCooldown;
        }

        /// <summary>
        /// Gets the kind of the enemy.
        /// </summary>
        public EnemyKind Kind { get; }

        /// <summary>
        /// Gets the maximum health of the enemy.
        /// </summary>
        public int MaxHealth { get; }
        public float MoveSpeed { get; }
        public float AttackDamage { get; }
        public float AttackRange { get; }
        public float AttackCooldown { get; }

        /// <summary>
        /// Returns the hash code for the current enemy configuration.
        /// </summary>
        /// <returns>A hash code for the current enemy configuration.</returns>
        public override int GetHashCode()
        {
            return HashCode.Combine(
                Kind,
                MaxHealth,
                MoveSpeed,
                AttackDamage,
                AttackRange,
                AttackCooldown);
        }

        /// <summary>
        /// Determines whether the current instance is equal to another object.
        /// </summary>
        /// <param name="obj">The object to compare with the current instance.</param>
        /// <returns><see langword="true"/> if <paramref name="obj"/> is an <see cref="EnemyConfig"/> and is equal to the current instance; otherwise, <see langword="false"/>.</returns>
        public override bool Equals(object obj)
        {
            return obj is EnemyConfig other && Equals(other);
        }

        /// <summary>
        /// Determines whether the current instance is equal to another <see cref="EnemyConfig"/>.
        /// </summary>
        /// <param name="other">The <see cref="EnemyConfig"/> to compare with the current instance.</param>
        /// <returns><see langword="true"/> if the current instance is equal to <paramref name="other"/>; otherwise, <see langword="false"/>.</returns>
        public bool Equals(EnemyConfig other)
        {
            return Kind == other.Kind
                && MaxHealth == other.MaxHealth
                && MoveSpeed == other.MoveSpeed
                && AttackDamage == other.AttackDamage
                && AttackRange == other.AttackRange
                && AttackCooldown == other.AttackCooldown;
        }
    }
}
