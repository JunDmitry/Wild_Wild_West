using System.Collections.Generic;
using Game.Core.Model.Configs;
using Game.Core.Model.Entities;
using Game.Core.Model.Enums;
using Game.Core.Model.Facts;
using Game.Core.Model.Initializers;
using Game.Core.Model.Simulation.Resolutions;
using Game.Core.Model.States;
using Game.Core.Rules.Outcomes;

namespace Game.Core.Rules
{
    public static class WaveRules
    {
        public static WaveExecutionOutcome Tick(
            in WaveState wave,
            IReadOnlyDictionary<EntityId, EnemyState> enemies,
            in GameConfig config,
            in SpawnResolution spawnResolution)
        {
            WaveExecutionOutcome result = default;
            Dictionary<EntityId, EnemyState> nextEnemies = new(enemies);

            switch (wave.Phase)
            {
                case WavePhase.RegularCombat:
                    result = TickRegular(
                        wave,
                        nextEnemies,
                        config,
                        spawnResolution);
                    break;

                case WavePhase.BossCombat:
                    result = TickBoss(
                        wave,
                        nextEnemies,
                        config,
                        spawnResolution);
                    break;

                case WavePhase.Completed:
                    break;
            }

            return result;
        }

        public static WaveState StartNextWave(GameConfig config, int nextNumber)
        {
            return WaveInit.FromConfig(config, nextNumber);
        }

        private static WaveExecutionOutcome TickRegular(
            WaveState wave,
            Dictionary<EntityId, EnemyState> enemies,
            in GameConfig config,
            in SpawnResolution spawnResolution)
        {
            WaveConfig waveConfig = config.Waves[wave.Number - 1];
            bool enemySpawned = true;
            EnemySpawnedFact spawnedFact = default;
            bool phaseChanged = false;
            WavePhase newPhase = wave.Phase;

            if (wave.RegularToSpawn > 0 && spawnResolution.HasSpawn)
            {
                EnemyState enemy = new(
                    spawnResolution.EntityId,
                    EnemyKind.Regular,
                    spawnResolution.Position,
                    config.RegularEnemy.MaxHealth,
                    config.RegularEnemy.MaxHealth,
                    attackReadyTime: 0f);

                enemies[spawnResolution.EntityId] = enemy;
                enemySpawned = true;
                spawnedFact = new Model.Facts.EnemySpawnedFact(
                                    enemy.Id,
                                    enemy.Kind,
                                    enemy.Position);

                wave = new WaveState(
                    wave.Number,
                    wave.Phase,
                    wave.RegularToSpawn - 1,
                    wave.RegularAlive + 1,
                    wave.BossStatus);
            }

            if (wave.RegularToSpawn == 0
                && wave.RegularAlive == 0
                && wave.BossStatus == BossStatus.NotSpawned)
            {
                WavePhase candidatePhase;
                BossStatus candidateStatus;

                if (waveConfig.HasBoss)
                {
                    candidatePhase = WavePhase.BossCombat;
                    candidateStatus = BossStatus.NotSpawned;
                }
                else
                {
                    candidatePhase = WavePhase.Completed;
                    candidateStatus = BossStatus.Defeated;
                }

                wave = new WaveState(
                    wave.Number,
                    candidatePhase,
                    wave.RegularToSpawn,
                    wave.RegularAlive,
                    candidateStatus);
                phaseChanged = true;
                newPhase = candidatePhase;
            }

            return new(
                wave,
                enemies,
                enemySpawned,
                spawnedFact,
                phaseChanged,
                newPhase);
        }

        private static WaveExecutionOutcome TickBoss(
            WaveState wave,
            Dictionary<EntityId, EnemyState> enemies,
            in GameConfig config,
            in SpawnResolution spawnResolution)
        {
            bool enemySpawned = true;
            EnemySpawnedFact spawnedFact = default;
            bool phaseChanged = false;
            WavePhase newPhase = wave.Phase;

            if (wave.BossStatus == BossStatus.NotSpawned && spawnResolution.HasSpawn)
            {
                EnemyState boss = new(
                    spawnResolution.EntityId,
                    EnemyKind.Boss,
                    spawnResolution.Position,
                    config.BossEnemy.MaxHealth,
                    config.BossEnemy.MaxHealth,
                    attackReadyTime: 0f);

                enemies[spawnResolution.EntityId] = boss;
                enemySpawned = true;
                spawnedFact = new Model.Facts.EnemySpawnedFact(
                    boss.Id,
                    boss.Kind,
                    boss.Position);

                wave = new WaveState(
                    wave.Number,
                    WavePhase.BossCombat,
                    wave.RegularToSpawn,
                    wave.RegularAlive,
                    BossStatus.Alive);
            }

            if (wave.BossStatus == BossStatus.Defeated)
            {
                newPhase = WavePhase.Completed;
                phaseChanged = true;
                wave = new WaveState(
                    wave.Number,
                    WavePhase.Completed,
                    wave.RegularToSpawn,
                    wave.RegularAlive,
                    BossStatus.Defeated);
            }

            return new(
                wave,
                enemies,
                enemySpawned,
                spawnedFact,
                phaseChanged,
                newPhase);
        }
    }
}
