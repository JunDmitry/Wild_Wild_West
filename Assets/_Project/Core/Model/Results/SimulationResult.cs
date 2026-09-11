using System;
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
        public SimulationResult(
            bool weaponSwitched,
            WeaponKind selectedWeapon,
            bool playerAttacked,
            WeaponKind attackWeapon,
            bool playerAttackHit,
            float playerDamageTaken,
            bool playerDefeated,
            IReadOnlyList<EnemySpawnedFact> enemiesSpawned,
            IReadOnlyList<EnemyDamagedFact> enemiesDamaged,
            IReadOnlyList<EntityId> enemiesDefeated,
            bool wavePhaseChanged,
            WavePhase wavePhase,
            bool waveStarted,
            int waveNumber,
            bool gamePhaseChanged,
            GamePhase gamePhase,
            bool restartRequested)
        {
            WeaponSwitched = weaponSwitched;
            SelectedWeapon = selectedWeapon;
            PlayerAttacked = playerAttacked;
            AttackWeapon = attackWeapon;
            PlayerAttackHit = playerAttackHit;
            PlayerDamageTaken = playerDamageTaken;
            PlayerDefeated = playerDefeated;
            EnemiesSpawned = enemiesSpawned ?? Array.Empty<EnemySpawnedFact>();
            EnemiesDamaged = enemiesDamaged ?? Array.Empty<EnemyDamagedFact>();
            EnemiesDefeated = enemiesDefeated ?? Array.Empty<EntityId>();
            WavePhaseChanged = wavePhaseChanged;
            WavePhase = wavePhase;
            WaveStarted = waveStarted;
            WaveNumber = waveNumber;
            GamePhaseChanged = gamePhaseChanged;
            GamePhase = gamePhase;
            RestartRequested = restartRequested;
        }

        public static SimulationResult Empty { get; } = new(
            false,
            default,
            false,
            default,
            false,
            0f,
            false,
            Array.Empty<EnemySpawnedFact>(),
            Array.Empty<EnemyDamagedFact>(),
            Array.Empty<EntityId>(),
            false,
            default,
            false,
            0,
            false,
            default,
            false);

        /// <summary>
        /// Gets a value indicating whether the weapon was switched.
        /// </summary>
        public bool WeaponSwitched { get; }

        /// <summary>
        /// Gets the kind of weapon that was selected.
        /// </summary>
        public WeaponKind SelectedWeapon { get; }

        /// <summary>
        /// Gets a value indicating whether the player attacked.
        /// </summary>
        public bool PlayerAttacked { get; }

        /// <summary>
        /// Gets the kind of weapon used for the attack.
        /// </summary>
        public WeaponKind AttackWeapon { get; }

        /// <summary>
        /// Gets a value indicating whether the player's attack hit.
        /// </summary>
        public bool PlayerAttackHit { get; }

        /// <summary>
        /// Gets the total amount of damage taken by the player.
        /// </summary>
        public float PlayerDamageTaken { get; }

        /// <summary>
        /// Gets a value indicating whether the player took any damage.
        /// </summary>
        public bool PlayerDamaged => PlayerDamageTaken > 0;

        /// <summary>
        /// Gets a value indicating whether the player was defeated.
        /// </summary>
        public bool PlayerDefeated { get; }

        /// <summary>
        /// Gets the list of enemies that were spawned.
        /// </summary>
        public IReadOnlyList<EnemySpawnedFact> EnemiesSpawned { get; }

        /// <summary>
        /// Gets the list of enemies that took damage.
        /// </summary>
        public IReadOnlyList<EnemyDamagedFact> EnemiesDamaged { get; }

        /// <summary>
        /// Gets the list of identifiers of enemies that were defeated.
        /// </summary>
        public IReadOnlyList<EntityId> EnemiesDefeated { get; }

        /// <summary>
        /// Gets a value indicating whether the wave phase changed.
        /// </summary>
        public bool WavePhaseChanged { get; }

        /// <summary>
        /// Gets the new wave phase.
        /// </summary>
        public WavePhase WavePhase { get; }

        /// <summary>
        /// Gets a value indicating whether a wave started.
        /// </summary>
        public bool WaveStarted { get; }

        /// <summary>
        /// Gets the number of the started wave.
        /// </summary>
        public int WaveNumber { get; }

        /// <summary>
        /// Gets a value indicating whether the game phase changed.
        /// </summary>
        public bool GamePhaseChanged { get; }

        /// <summary>
        /// Gets the new game phase.
        /// </summary>
        public GamePhase GamePhase { get; }

        /// <summary>
        /// Gets a value indicating whether a restart was requested.
        /// </summary>
        public bool RestartRequested { get; }

        public static SimulationResult Idle(WeaponKind selectedWeapon, bool restartRequested)
        {
            return new(
                false,
                selectedWeapon,
                false,
                default,
                false,
                0f,
                false,
                Array.Empty<EnemySpawnedFact>(),
                Array.Empty<EnemyDamagedFact>(),
                Array.Empty<EntityId>(),
                false,
                default,
                false,
                0,
                false,
                default,
                restartRequested);
        }
    }
}
