using Game.Core.Model.Entities;

namespace Game.Core.Model.Facts
{
    /// <summary>
    /// Represents a fact describing that an enemy has taken damage.
    /// </summary>
    public readonly struct EnemyDamagedFact
    {
        /// <summary>
        /// Initializes a new instance of the <see cref="EnemyDamagedFact"/> struct.
        /// </summary>
        /// <param name="id">The identifier of the damaged enemy.</param>
        /// <param name="damageAmount">The amount of damage dealt.</param>
        /// <param name="currentHealth">The remaining health of the enemy after the damage was applied.</param>
        public EnemyDamagedFact(
            EntityId id,
            float damageAmount,
            float currentHealth)
        {
            Id = id;
            DamageAmount = damageAmount;
            CurrentHealth = currentHealth;
        }

        /// <summary>
        /// Gets the identifier of the damaged enemy.
        /// </summary>
        public EntityId Id { get; }

        /// <summary>
        /// Gets the amount of damage dealt.
        /// </summary>
        public float DamageAmount { get; }

        /// <summary>
        /// Gets the remaining health of the enemy after the damage was applied.
        /// </summary>
        public float CurrentHealth { get; }
    }
}
