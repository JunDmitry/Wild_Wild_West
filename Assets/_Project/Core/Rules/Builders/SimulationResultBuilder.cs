using System.Collections.Generic;
using Game.Core.Model.Entities;
using Game.Core.Model.Enums;
using Game.Core.Model.Facts;
using Game.Core.Model.Results;

namespace Game.Core.Rules.Builders
{
    public sealed class SimulationResultBuilder
    {
        private readonly List<EnemySpawnedFact> _enemiesSpawned = new();
        private readonly List<EnemyDamagedFact> _enemiesDamaged = new();
        private readonly List<EntityId> _enemiesDefeated = new();

        private bool _weaponSwitched;
        private WeaponKind _selectedWeapon;

        private bool _playerAttacked;
        private WeaponKind _attackWeapon;
        private bool _playerAttackHit;

        private float _playerDamageTaken;
        private bool _playerDefeated;

        private bool _wavePhaseChanged;
        private WavePhase _wavePhase;

        private bool _waveStarted;
        private int _waveNumber;

        private bool _gamePhaseChanged;
        private GamePhase _gamePhase;

        private bool _restartRequested;

        public bool PlayerDefeated => _playerDefeated;

        internal void MarkWeaponSwitched(WeaponKind kind)
        {
            _weaponSwitched = true;
            _selectedWeapon = kind;
        }

        internal void MarkPlayerAttacked(WeaponKind kind, bool hit)
        {
            _playerAttacked = true;
            _attackWeapon = kind;

            if (hit)
            {
                _playerAttackHit = true;
            }
        }

        internal void AddPlayerDamage(float amount)
        {
            if (amount > 0)
            {
                _playerDamageTaken += amount;
            }
        }

        internal void MarkPlayerDefeated()
        {
            _playerDefeated = true;
        }

        internal void AddEnemySpawned(EnemySpawnedFact fact)
        {
            _enemiesSpawned.Add(fact);
        }

        internal void AddEnemyDamaged(EnemyDamagedFact fact)
        {
            _enemiesDamaged.Add(fact);
        }

        internal void AddEnemyDefeated(EntityId id)
        {
            _enemiesDefeated.Add(id);
        }

        internal void MarkWavePhase(WavePhase phase)
        {
            _wavePhaseChanged = true;
            _wavePhase = phase;
        }

        internal void MarkWaveStarted(int number)
        {
            _waveStarted = true;
            _waveNumber = number;
        }

        internal void MarkGamePhase(GamePhase phase)
        {
            _gamePhaseChanged = true;
            _gamePhase = phase;
        }

        internal void MarkRestartRequested()
        {
            _restartRequested = true;
        }

        public SimulationResult Build()
        {
            return new SimulationResult(
                _weaponSwitched,
                _selectedWeapon,
                _playerAttacked,
                _attackWeapon,
                _playerAttackHit,
                _playerDamageTaken,
                _playerDefeated,
                _enemiesSpawned.ToArray(),
                _enemiesDamaged.ToArray(),
                _enemiesDefeated.ToArray(),
                _wavePhaseChanged,
                _wavePhase,
                _waveStarted,
                _waveNumber,
                _gamePhaseChanged,
                _gamePhase,
                _restartRequested);
        }
    }
}
