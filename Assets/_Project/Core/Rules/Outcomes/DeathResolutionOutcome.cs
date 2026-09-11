using System;
using System.Collections.Generic;
using Game.Core.Model.Entities;
using Game.Core.Model.States;

namespace Game.Core.Rules.Outcomes
{
    /// <summary>
    /// Represents the outcome of resolving deaths for the player and enemies.
    /// </summary>
    public readonly struct DeathResolutionOutcome
    {
        /// <summary>
        /// Initializes a new instance of the <see cref="DeathResolutionOutcome"/> struct.
        /// </summary>
        /// <param name="player">The updated player state.</param>
        /// <param name="enemies">The updated collection of enemy states keyed by entity identifier.</param>
        /// <param name="wave">The updated wave state.</param>
        /// <param name="playerDefeated">A value indicating whether the player was defeated.</param>
        /// <param name="defeatedEnemyIds">The identifiers of enemies that were defeated.</param>
        public DeathResolutionOutcome(
            PlayerState player,
            Dictionary<EntityId, EnemyState> enemies,
            WaveState wave,
            bool playerDefeated,
            IReadOnlyList<EntityId> defeatedEnemyIds)
        {
            Player = player;
            Enemies = enemies;
            Wave = wave;
            PlayerDefeated = playerDefeated;
            DefeatedEnemyIds = defeatedEnemyIds ?? Array.Empty<EntityId>();
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
        /// Gets the updated wave state.
        /// </summary>
        public WaveState Wave { get; }

        /// <summary>
        /// Gets a value indicating whether the player was defeated.
        /// </summary>
        public bool PlayerDefeated { get; }

        /// <summary>
        /// Gets the identifiers of enemies that were defeated.
        /// </summary>
        public IReadOnlyList<EntityId> DefeatedEnemyIds { get; }
    }
}
