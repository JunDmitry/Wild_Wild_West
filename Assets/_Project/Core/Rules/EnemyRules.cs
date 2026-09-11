using System.Collections.Generic;
using Game.Core.Model.Configs;
using Game.Core.Model.Entities;
using Game.Core.Model.Enums;
using Game.Core.Model.States;
using Game.Core.Rules.Mathematics;
using Game.Core.Rules.Outcomes;

namespace Game.Core.Rules
{
    public static class EnemyRules
    {
        private const float Epsilon = .0001f;

        public static EnemyTickOutcome TickAll(
            in PlayerState player,
            IReadOnlyDictionary<EntityId, EnemyState> enemies,
            in GameConfig config,
            float time,
            float delta)
        {
            Dictionary<EntityId, EnemyState> nextEnemies = new(enemies.Count);
            PlayerState nextPlayer = player;
            float totalDamageToPlayer = 0f;

            foreach (KeyValuePair<EntityId, EnemyState> pair in enemies)
            {
                EnemyState enemy = pair.Value;

                if (CombatRules.IsDefeated(enemy.CurrentHealth))
                {
                    nextEnemies[pair.Key] = enemy;
                    continue;
                }

                EnemyConfig enemyConfig = enemy.Kind == EnemyKind.Boss
                    ? config.BossEnemy
                    : config.RegularEnemy;
                float distance = Math3D.DistanceXZ(enemy.Position, nextPlayer.Position);

                if (distance > enemyConfig.AttackRange)
                {
                    enemy = MoveToward(enemy, nextPlayer.Position, enemyConfig.MoveSpeed, delta);
                }
                else if (time >= enemy.AttackReadyTime)
                {
                    enemy = new EnemyState(
                        enemy.Id,
                        enemy.Kind,
                        enemy.Position,
                        enemy.CurrentHealth,
                        enemy.MaxHealth,
                        time + enemyConfig.AttackCooldown);

                    float health = CombatRules.ApplyDamage(nextPlayer.CurrentHealth, enemyConfig.AttackDamage);
                    float dealt = nextPlayer.CurrentHealth - health;
                    totalDamageToPlayer += dealt;

                    nextPlayer = new(
                        nextPlayer.Id,
                        nextPlayer.Position,
                        health,
                        nextPlayer.MaxHealth,
                        nextPlayer.SelectedWeapon,
                        nextPlayer.RangedReadyTime,
                        nextPlayer.MeleeReadyTime);
                }

                nextEnemies[pair.Key] = enemy;
            }

            return new(
                nextPlayer,
                nextEnemies,
                totalDamageToPlayer);
        }

        private static EnemyState MoveToward(
            in EnemyState enemy,
            Position3D target,
            float speed,
            float delta)
        {
            Direction3D raw = new(target.X - enemy.Position.X, 0f, target.Z - enemy.Position.Z);
            Direction3D direction = Math3D.NormalizeXZ(raw);

            if (Math3D.LengthXZ(direction) <= Epsilon)
            {
                return enemy;
            }

            Position3D position = Math3D.OffsetXZ(enemy.Position, direction, speed * delta);

            return new EnemyState(
                enemy.Id,
                enemy.Kind,
                position,
                enemy.CurrentHealth,
                enemy.MaxHealth,
                enemy.AttackReadyTime);
        }
    }
}
