using System.Collections.Generic;
using Game.Core.Model.Entities;
using Game.Core.Model.Enums;
using Game.Core.Model.Facts;
using Game.Core.Model.States;

namespace Game.Core.Rules.Outcomes
{
    /// <summary>
    /// Represents the outcome of a wave execution step.
    /// </summary>
    public readonly struct WaveExecutionOutcome
    {
        /// <summary>
        /// Initializes a new instance of the <see cref="WaveExecutionOutcome"/> struct.
        /// </summary>
        /// <param name="wave">The updated wave state.</param>
        /// <param name="enemies">The updated collection of enemy states keyed by entity identifier.</param>
        /// <param name="enemySpawned">A value indicating whether an enemy was spawned.</param>
        /// <param name="spawnedFact">The fact describing the spawned enemy, if any.</param>
        /// <param name="phaseChanged">A value indicating whether the wave phase changed.</param>
        /// <param name="newPhase">The new wave phase if it changed; otherwise, the previous phase.</param>
        public WaveExecutionOutcome(
            WaveState wave,
            Dictionary<EntityId, EnemyState> enemies,
            bool enemySpawned,
            EnemySpawnedFact spawnedFact,
            bool phaseChanged,
            WavePhase newPhase)
        {
            Wave = wave;
            Enemies = enemies;
            EnemySpawned = enemySpawned;
            SpawnedFact = spawnedFact;
            PhaseChanged = phaseChanged;
            NewPhase = newPhase;
        }

        /// <summary>
        /// Gets the updated wave state.
        /// </summary>
        public WaveState Wave { get; }

        /// <summary>
        /// Gets the updated collection of enemy states keyed by entity identifier.
        /// </summary>
        public Dictionary<EntityId, EnemyState> Enemies { get; }

        /// <summary>
        /// Gets a value indicating whether an enemy was spawned.
        /// </summary>
        public bool EnemySpawned { get; }

        /// <summary>
        /// Gets the fact describing the spawned enemy, if any.
        /// </summary>
        public EnemySpawnedFact SpawnedFact { get; }

        /// <summary>
        /// Gets a value indicating whether the wave phase changed.
        /// </summary>
        public bool PhaseChanged { get; }

        /// <summary>
        /// Gets the new wave phase if it changed; otherwise, the previous phase.
        /// </summary>
        public WavePhase NewPhase { get; }
    }
}
