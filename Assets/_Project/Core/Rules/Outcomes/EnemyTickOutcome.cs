using System.Collections.Generic;
using Game.Core.Model.Entities;
using Game.Core.Model.States;

namespace Game.Core.Rules.Outcomes
{
    /// <summary>
    /// Represents the outcome of an enemy tick update.
    /// </summary>
    public readonly struct EnemyTickOutcome
    {
        /// <summary>
        /// Initializes a new instance of the <see cref="EnemyTickOutcome"/> struct.
        /// </summary>
        /// <param name="player">The updated player state.</param>
        /// <param name="enemies">The updated collection of enemy states keyed by entity identifier.</param>
        /// <param name="playerDamageTaken">The amount of damage taken by the player during the tick.</param>
        public EnemyTickOutcome(
            PlayerState player,
            Dictionary<EntityId, EnemyState> enemies,
            float playerDamageTaken)
        {
            Player = player;
            Enemies = enemies;
            PlayerDamageTaken = playerDamageTaken;
        }

        /// <summary>
        /// Gets the updated player state.
        /// </summary>
        public PlayerState Player { get; }

        /// <summary>
        /// Gets the updated collection of enemy states keyed by entity identifier.
        /// </summary>
        public Dictionary<EntityId, EnemyState> Enemies { get; }

        /// <summary>
        /// Gets the amount of damage taken by the player during the tick.
        /// </summary>
        public float PlayerDamageTaken { get; }
    }
}
