using System;
using System.Collections.Generic;
using Game.Core.Model.Entities;
using Game.Core.Model.Enums;
using Game.Core.Model.States;
using Game.Core.Rules.Mathematics;
using Game.Core.Rules.Outcomes;

namespace Game.Core.Rules
{
    public static class DeathRules
    {
        public static DeathResolutionOutcome Apply(
            in PlayerState player,
            IReadOnlyDictionary<EntityId, EnemyState> enemies,
            in WaveState wave)
        {
            Dictionary<EntityId, EnemyState> survivedEnemies = new();
            List<EntityId> defeatedEnemyIds = new();

            int defeatedRegularsCount = 0;
            BossStatus bossStatus = wave.BossStatus;

            foreach (KeyValuePair<EntityId, EnemyState> pair in enemies)
            {
                EnemyState enemy = pair.Value;

                if (CombatRules.IsDefeated(enemy.CurrentHealth) == false)
                {
                    survivedEnemies[pair.Key] = enemy;
                    continue;
                }

                defeatedEnemyIds.Add(enemy.Id);

                if (enemy.Kind == EnemyKind.Regular)
                {
                    defeatedRegularsCount++;
                }
                else
                {
                    bossStatus = BossStatus.Defeated;
                }
            }

            WaveState nextWave = wave;

            if (defeatedRegularsCount > 0 || bossStatus != nextWave.BossStatus)
            {
                nextWave = new(
                    nextWave.Number,
                    nextWave.Phase,
                    nextWave.RegularToSpawn,
                    Math3D.Max(0, nextWave.RegularAlive - defeatedRegularsCount),
                    bossStatus);
            }

            bool playerDefeated = CombatRules.IsDefeated(player.CurrentHealth);

            return new(
                player,
                survivedEnemies,
                nextWave,
                playerDefeated,
                defeatedEnemyIds);
        }
    }
}
