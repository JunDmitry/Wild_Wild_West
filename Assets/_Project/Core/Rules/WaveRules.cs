using System.Collections.Generic;
using System.Linq;
using Game.Core.Model.Configs;
using Game.Core.Model.Entities;
using Game.Core.Model.Enums;
using Game.Core.Model.Initializers;
using Game.Core.Model.Results;
using Game.Core.Model.States;

namespace Game.Core.Rules
{
    public static class WaveRules
    {
        public static (WaveState Wave, Dictionary<EntityId, EnemyState> Enemies, EntityId NextId) Tick(
            in WaveState wave,
            IReadOnlyDictionary<EntityId, EnemyState> enemies,
            in GameConfig config,
            in FrameContext context,
            in EntityId nextEntityId,
            in SimulationResult result)
        {
            WaveState nextWave = wave;
            Dictionary<EntityId, EnemyState> nextEnemies = new(enemies);
            EntityId id = nextEntityId;

            switch (wave.Phase)
            {
                case WavePhase.RegularCombat:
                    (nextWave, nextEnemies, id) = TickRegular(wave, nextEnemies, config, context, id, result);
                    break;

                case WavePhase.BossCombat:
                    (nextWave, nextEnemies, id) = TickBoss(wave, nextEnemies, config, context, id, result);
                    break;

                case WavePhase.Completed:
                    break;
            }

            return (nextWave, nextEnemies, id);
        }

        public static WaveState StartNextWave(GameConfig config, int nextNumber)
        {
            return WaveInit.FromConfig(config, nextNumber);
        }

        private static (WaveState nextWave, Dictionary<EntityId, EnemyState> nextEnemies, EntityId id) TickRegular(
            WaveState wave,
            Dictionary<EntityId, EnemyState> enemies,
            in GameConfig config,
            in FrameContext context,
            EntityId nextId,
            in SimulationResult result)
        {
            if (wave.RegularToSpawn > 0 && context.HasSpawnPosition)
            {
                EntityId id = nextId;
                nextId = new EntityId(nextId.Value + 1);

                EnemyState enemy = new(
                    id,
                    EnemyKind.Regular,
                    context.NextSpawnPosition,
                    config.RegularEnemy.MaxHealth,
                    config.RegularEnemy.MaxHealth,
                    attackReadyTime: 0f);
                enemies[id] = enemy;
                result.AddEnemySpawned(new Model.Facts.EnemySpawnedFact(id, enemy.Kind, enemy.Position));

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
                wave = new WaveState(
                    wave.Number,
                    WavePhase.BossCombat,
                    wave.RegularToSpawn,
                    wave.RegularAlive,
                    BossStatus.NotSpawned);
                result.MarkWavePhase(WavePhase.BossCombat);
            }

            return (wave, enemies, nextId);
        }

        private static (WaveState nextWave, Dictionary<EntityId, EnemyState> nextEnemies, EntityId id) TickBoss(
            WaveState wave,
            Dictionary<EntityId, EnemyState> enemies,
            in GameConfig config,
            in FrameContext context,
            EntityId nextId,
            in SimulationResult result)
        {
            if (wave.BossStatus == BossStatus.NotSpawned && context.HasSpawnPosition)
            {
                EntityId id = nextId;
                nextId = new EntityId(nextId.Value + 1);

                EnemyState boss = new(
                    id,
                    EnemyKind.Boss,
                    context.NextSpawnPosition,
                    config.BossEnemy.MaxHealth,
                    config.BossEnemy.MaxHealth,
                    attackReadyTime: 0f);
                enemies[id] = boss;
                result.AddEnemySpawned(new Model.Facts.EnemySpawnedFact(id, boss.Kind, boss.Position));

                wave = new WaveState(
                    wave.Number,
                    WavePhase.BossCombat,
                    wave.RegularToSpawn,
                    wave.RegularAlive,
                    BossStatus.Alive);
            }

            if (wave.BossStatus == BossStatus.Defeated)
            {
                wave = new WaveState(
                    wave.Number,
                    WavePhase.Completed,
                    wave.RegularToSpawn,
                    wave.RegularAlive,
                    BossStatus.Defeated);

                result.MarkWavePhase(WavePhase.Completed);
            }

            return (wave, enemies, nextId);
        }
    }
}
