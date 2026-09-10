using System.Collections.Generic;
using Game.Core.Model.Entities;
using Game.Core.Model.Enums;
using Game.Core.Model.Facts;

namespace Game.Core.Model.Results
{
    /// <summary>
    /// Represents the result of a simulation step, containing facts about events that occurred.
    /// </summary>
    public sealed class SimulationResult
    {
        private readonly List<EnemySpawnedFact> _enemiesSpawned = new();
        private readonly List<EnemyDamagedFact> _enemiesDamaged = new();
        private readonly List<EntityId> _enemiesDefeated = new();

        /// <summary>
        /// Gets a value indicating whether the weapon was switched.
        /// </summary>
        public bool WeaponSwitched { get; private set; }

        /// <summary>
        /// Gets the kind of weapon that was selected.
        /// </summary>
        public WeaponKind SelectedWeapon { get; private set; }

        /// <summary>
        /// Gets a value indicating whether the player attacked.
        /// </summary>
        public bool PlayerAttacked { get; private set; }

        /// <summary>
        /// Gets the kind of weapon used for the attack.
        /// </summary>
        public WeaponKind AttackWeapon { get; private set; }

        /// <summary>
        /// Gets a value indicating whether the player's attack hit.
        /// </summary>
        public bool PlayerAttackHit { get; private set; }

        /// <summary>
        /// Gets the total amount of damage taken by the player.
        /// </summary>
        public float PlayerDamageTaken { get; private set; }

        /// <summary>
        /// Gets a value indicating whether the player took any damage.
        /// </summary>
        public bool PlayerDamaged => PlayerDamageTaken > 0;

        /// <summary>
        /// Gets a value indicating whether the player was defeated.
        /// </summary>
        public bool PlayerDefeated { get; private set; }

        /// <summary>
        /// Gets the list of enemies that were spawned.
        /// </summary>
        public IReadOnlyList<EnemySpawnedFact> EnemiesSpawned => _enemiesSpawned;

        /// <summary>
        /// Gets the list of enemies that took damage.
        /// </summary>
        public IReadOnlyList<EnemyDamagedFact> EnemiesDamaged => _enemiesDamaged;

        /// <summary>
        /// Gets the list of identifiers of enemies that were defeated.
        /// </summary>
        public IReadOnlyList<EntityId> EnemiesDefeated => _enemiesDefeated;

        /// <summary>
        /// Gets a value indicating whether the wave phase changed.
        /// </summary>
        public bool WavePhaseChanged { get; private set; }

        /// <summary>
        /// Gets the new wave phase.
        /// </summary>
        public WavePhase WavePhase { get; private set; }

        /// <summary>
        /// Gets a value indicating whether a wave started.
        /// </summary>
        public bool WaveStarted { get; private set; }

        /// <summary>
        /// Gets the number of the started wave.
        /// </summary>
        public int WaveNumber { get; private set; }

        /// <summary>
        /// Gets a value indicating whether the game phase changed.
        /// </summary>
        public bool GamePhaseChanged { get; private set; }

        /// <summary>
        /// Gets the new game phase.
        /// </summary>
        public GamePhase GamePhase { get; private set; }

        /// <summary>
        /// Gets a value indicating whether a restart was requested.
        /// </summary>
        public bool RestartRequested { get; private set; }

        /// <summary>
        /// Marks that the weapon was switched.
        /// </summary>
        /// <param name="kind">The kind of weapon that was selected.</param>
        public void MarkWeaponSwitched(WeaponKind kind)
        {
            WeaponSwitched = true;
            SelectedWeapon = kind;
        }

        /// <summary>
        /// Marks that the player attacked.
        /// </summary>
        /// <param name="kind">The kind of weapon used for the attack.</param>
        /// <param name="hit">A value indicating whether the attack hit.</param>
        public void MarkPlayerAttacked(WeaponKind kind, bool hit)
        {
            PlayerAttacked = true;
            AttackWeapon = kind;

            if (hit)
            {
                PlayerAttackHit = true;
            }
        }

        /// <summary>
        /// Adds damage taken by the player.
        /// </summary>
        /// <param name="amount">The amount of damage taken.</param>
        public void AddPlayerDamage(float amount)
        {
            if (amount > 0)
            {
                PlayerDamageTaken += amount;
            }
        }

        /// <summary>
        /// Marks that the player was defeated.
        /// </summary>
        public void MarkPlayerDefeated()
        {
            PlayerDefeated = true;
        }

        /// <summary>
        /// Adds a fact about an enemy that was spawned.
        /// </summary>
        /// <param name="fact">The enemy spawned fact.</param>
        public void AddEnemySpawned(EnemySpawnedFact fact)
        {
            _enemiesSpawned.Add(fact);
        }

        /// <summary>
        /// Adds a fact about an enemy that took damage.
        /// </summary>
        /// <param name="fact">The enemy damaged fact.</param>
        public void AddEnemyDamaged(EnemyDamagedFact fact)
        {
            _enemiesDamaged.Add(fact);
        }

        /// <summary>
        /// Adds an enemy identifier to the list of defeated enemies.
        /// </summary>
        /// <param name="id">The identifier of the defeated enemy.</param>
        public void AddEnemyDefeated(EntityId id)
        {
            _enemiesDefeated.Add(id);
        }

        /// <summary>
        /// Marks that the wave phase changed.
        /// </summary>
        /// <param name="phase">The new wave phase.</param>
        public void MarkWavePhase(WavePhase phase)
        {
            WavePhaseChanged = true;
            WavePhase = phase;
        }

        /// <summary>
        /// Marks that a wave started.
        /// </summary>
        /// <param name="number">The number of the started wave.</param>
        public void MarkWaveStarted(int number)
        {
            WaveStarted = true;
            WaveNumber = number;
        }

        /// <summary>
        /// Marks that the game phase changed.
        /// </summary>
        /// <param name="phase">The new game phase.</param>
        public void MarkGamePhase(GamePhase phase)
        {
            GamePhaseChanged = true;
            GamePhase = phase;
        }

        /// <summary>
        /// Marks that a restart was requested.
        /// </summary>
        public void MarkRestartRequested()
        {
            RestartRequested = true;
        }
    }
}
