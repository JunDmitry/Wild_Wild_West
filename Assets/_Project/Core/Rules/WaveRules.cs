using System.Collections.Generic;
using System.Linq;
using Game.Core.Model.Configs;
using Game.Core.Model.Entities;
using Game.Core.Model.Enums;
using Game.Core.Model.Initializers;
using Game.Core.Model.Results;
using Game.Core.Model.Spawning;
using Game.Core.Model.States;
using Game.Core.Rules.Builders;

namespace Game.Core.Rules
{
    public static class WaveRules
    {
        public static (WaveState Wave, Dictionary<EntityId, EnemyState> Enemies) Tick(
            in WaveState wave,
            IReadOnlyDictionary<EntityId, EnemyState> enemies,
            in GameConfig config,
            in FrameContext context,
            SimulationResultBuilder resultBuilder)
        {
            WaveState nextWave = wave;
            Dictionary<EntityId, EnemyState> nextEnemies = new(enemies);

            switch (wave.Phase)
            {
                case WavePhase.RegularCombat:
                    (nextWave, nextEnemies) = TickRegular(
                        wave,
                        nextEnemies,
                        config,
                        context,
                        resultBuilder);
                    break;

                case WavePhase.BossCombat:
                    (nextWave, nextEnemies) = TickBoss(
                        wave,
                        nextEnemies,
                        config,
                        context,
                        resultBuilder);
                    break;

                case WavePhase.Completed:
                    break;
            }

            return (nextWave, nextEnemies);
        }

        public static WaveState StartNextWave(GameConfig config, int nextNumber)
        {
            return WaveInit.FromConfig(config, nextNumber);
        }

        private static (WaveState nextWave, Dictionary<EntityId, EnemyState> nextEnemies) TickRegular(
            WaveState wave,
            Dictionary<EntityId, EnemyState> enemies,
            in GameConfig config,
            in FrameContext context,
            SimulationResultBuilder resultBuilder)
        {
            WaveConfig waveConfig = config.Waves[wave.Number - 1];

            if (wave.RegularToSpawn > 0 && TryGetSpawnReservation(context, out SpawnReservation spawnReservation))
            {
                EnemyState enemy = new(
                    spawnReservation.EntityId,
                    EnemyKind.Regular,
                    spawnReservation.Position,
                    config.RegularEnemy.MaxHealth,
                    config.RegularEnemy.MaxHealth,
                    attackReadyTime: 0f);

                enemies[spawnReservation.EntityId] = enemy;

                resultBuilder.AddEnemySpawned(new Model.Facts.EnemySpawnedFact(
                    enemy.Id,
                    enemy.Kind,
                    enemy.Position));

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
                resultBuilder.MarkWavePhase(candidatePhase);
            }

            return (wave, enemies);
        }

        private static (WaveState nextWave, Dictionary<EntityId, EnemyState> nextEnemies) TickBoss(
            WaveState wave,
            Dictionary<EntityId, EnemyState> enemies,
            in GameConfig config,
            in FrameContext context,
            SimulationResultBuilder resultBuilder)
        {
            if (wave.BossStatus == BossStatus.NotSpawned && TryGetSpawnReservation(context, out SpawnReservation spawnReservation))
            {
                EnemyState boss = new(
                    spawnReservation.EntityId,
                    EnemyKind.Boss,
                    spawnReservation.Position,
                    config.BossEnemy.MaxHealth,
                    config.BossEnemy.MaxHealth,
                    attackReadyTime: 0f);

                enemies[spawnReservation.EntityId] = boss;

                resultBuilder.AddEnemySpawned(new Model.Facts.EnemySpawnedFact(
                    boss.Id,
                    boss.Kind,
                    boss.Position));

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

                resultBuilder.MarkWavePhase(WavePhase.Completed);
            }

            return (wave, enemies);
        }

        private static bool TryGetSpawnReservation(in FrameContext context, out SpawnReservation reservation)
        {
            if (context.SpawnReservations.Count <= 0)
            {
                reservation = default;
                return false;
            }

            reservation = context.SpawnReservations[0];

            return true;
        }
    }
}
