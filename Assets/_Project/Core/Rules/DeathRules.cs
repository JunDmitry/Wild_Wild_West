using System;
using System.Collections.Generic;
using Game.Core.Model.Entities;
using Game.Core.Model.Enums;
using Game.Core.Model.Results;
using Game.Core.Model.States;
using Game.Core.Rules.Builders;
using Game.Core.Rules.Mathematics;

namespace Game.Core.Rules
{
    public static class DeathRules
    {
        public static (PlayerState Player, Dictionary<EntityId, EnemyState> Enemies, WaveState Wave) Apply(
            in PlayerState player,
            IReadOnlyDictionary<EntityId, EnemyState> enemies,
            in WaveState wave,
            SimulationResultBuilder resultBuilder)
        {
            Dictionary<EntityId, EnemyState> nextEnemies = new();
            WaveState nextWave = wave;

            int defeatedEnemiesCount = 0;
            BossStatus status = nextWave.BossStatus;

            foreach (KeyValuePair<EntityId, EnemyState> pair in enemies)
            {
                EnemyState enemy = pair.Value;

                if (CombatRules.IsDefeated(enemy.CurrentHealth) == false)
                {
                    nextEnemies[pair.Key] = enemy;
                    continue;
                }

                resultBuilder.AddEnemyDefeated(enemy.Id);

                if (enemy.Kind == EnemyKind.Regular)
                {
                    defeatedEnemiesCount++;
                }
                else
                {
                    status = BossStatus.Defeated;
                }
            }

            if (defeatedEnemiesCount > 0 || status != nextWave.BossStatus)
            {
                nextWave = new(
                    nextWave.Number,
                    nextWave.Phase,
                    nextWave.RegularToSpawn,
                    Math3D.Max(0, nextWave.RegularAlive - defeatedEnemiesCount),
                    status);
            }

            if (CombatRules.IsDefeated(player.CurrentHealth))
            {
                resultBuilder.MarkPlayerDefeated();
            }

            return (player, nextEnemies, nextWave);
        }
    }
}
