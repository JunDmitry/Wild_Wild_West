using System.Collections.Generic;
using Game.Core.Model.Configs;
using Game.Core.Model.Entities;
using Game.Core.Model.Enums;
using Game.Core.Model.Results;
using Game.Core.Model.States;
using Game.Core.Rules.Builders;

namespace Game.Core.Rules
{
    public static class WeaponRules
    {
        public static PlayerState TrySwitch(
            in PlayerState player,
            bool switchPressed,
            SimulationResultBuilder resultBuilder)
        {
            if (switchPressed == false)
            {
                return player;
            }

            WeaponKind next = player.SelectedWeapon == Model.Enums.WeaponKind.Ranged
                ? WeaponKind.Melee
                : WeaponKind.Ranged;

            resultBuilder.MarkWeaponSwitched(next);

            return new PlayerState(
                player.Id,
                player.Position,
                player.CurrentHealth,
                player.MaxHealth,
                next,
                player.RangedReadyTime,
                player.MeleeReadyTime);
        }

        public static (PlayerState Player, IReadOnlyDictionary<EntityId, EnemyState> Enemies) TryPlayerAttack(
            in PlayerState player,
            IReadOnlyDictionary<EntityId, EnemyState> enemies,
            in FrameInput input,
            in FrameContext context,
            in GameConfig config,
            float time,
            SimulationResultBuilder resultBuilder)
        {
            if (input.AttackPressed == false)
            {
                return (player, enemies);
            }

            WeaponConfig weapon = player.SelectedWeapon == WeaponKind.Ranged
                ? config.RangedWeapon
                : config.MeleeWeapon;
            float readyTime = player.SelectedWeapon == WeaponKind.Ranged
                ? player.RangedReadyTime
                : player.MeleeReadyTime;

            if (time < readyTime)
            {
                return (player, enemies);
            }

            float nextReady = time + weapon.Cooldown;
            PlayerState updatedPlayer = WithReadyTime(player, player.SelectedWeapon, nextReady);
            bool anyHit = false;
            Dictionary<EntityId, EnemyState> nextEnemies = new(enemies);

            if (updatedPlayer.SelectedWeapon == WeaponKind.Ranged)
            {
                if (context.HasRangedHit && nextEnemies.TryGetValue(context.RangedHitId, out EnemyState target))
                {
                    anyHit = true;
                    ApplyHit(nextEnemies, target, weapon.Damage, resultBuilder);
                }
            }
            else
            {
                for (int i = 0; i < context.MeleeHitIds.Count; i++)
                {
                    EntityId id = context.MeleeHitIds[i];

                    if (nextEnemies.TryGetValue(id, out EnemyState target) == false)
                    {
                        continue;
                    }

                    anyHit = true;
                    ApplyHit(nextEnemies, target, weapon.Damage, resultBuilder);
                }
            }

            resultBuilder.MarkPlayerAttacked(updatedPlayer.SelectedWeapon, anyHit);

            return (updatedPlayer, nextEnemies);
        }

        private static PlayerState WithReadyTime(in PlayerState player, WeaponKind kind, float readyTime)
        {
            float ranged = player.RangedReadyTime;
            float melee = player.MeleeReadyTime;

            if (kind == WeaponKind.Ranged)
            {
                ranged = readyTime;
            }
            else
            {
                melee = readyTime;
            }

            return new(
                player.Id,
                player.Position,
                player.CurrentHealth,
                player.MaxHealth,
                player.SelectedWeapon,
                ranged,
                melee);
        }

        private static void ApplyHit(
            Dictionary<EntityId, EnemyState> enemies,
            in EnemyState target,
            float damage,
            SimulationResultBuilder resultBuilder)
        {
            float health = CombatRules.ApplyDamage(target.CurrentHealth, damage);
            EnemyState updated = new(
                target.Id,
                target.Kind,
                target.Position,
                health,
                target.MaxHealth,
                target.AttackReadyTime);

            resultBuilder.AddEnemyDamaged(new Model.Facts.EnemyDamagedFact(updated.Id, damage, health));
            enemies[target.Id] = updated;
        }
    }
}
