using System;
using System.Collections.Generic;
using Game.Core.Model.Configs;
using Game.Core.Model.Entities;
using Game.Core.Model.Enums;
using Game.Core.Model.Facts;
using Game.Core.Model.Simulation.Queries;
using Game.Core.Model.Simulation.Resolutions;
using Game.Core.Model.States;
using Game.Core.Rules.Outcomes;

namespace Game.Core.Rules
{
    public static class WeaponRules
    {
        public static WeaponSwitchOutcome TrySwitch(
            in PlayerState player,
            bool switchPressed)
        {
            if (switchPressed == false)
            {
                return new(player, false, player.SelectedWeapon);
            }

            WeaponKind next = player.SelectedWeapon == Model.Enums.WeaponKind.Ranged
                ? WeaponKind.Melee
                : WeaponKind.Ranged;

            PlayerState updated = new(
                player.Id,
                player.Position,
                player.CurrentHealth,
                player.MaxHealth,
                next,
                player.RangedReadyTime,
                player.MeleeReadyTime);

            return new(updated, true, next);
        }

        public static AttackExecutionOutcome TryPlayerAttack(
            in PlayerState player,
            IReadOnlyDictionary<EntityId, EnemyState> enemies,
            in AttackQuery plannedAttack,
            in AttackResolution resolution,
            in GameConfig config,
            float time)
        {
            if (plannedAttack.Kind == AttackQueryKind.None)
            {
                return new(
                    player,
                    enemies,
                    false,
                    player.SelectedWeapon,
                    false,
                    Array.Empty<EnemyDamagedFact>());
            }

            WeaponConfig weapon = player.SelectedWeapon == WeaponKind.Ranged
                ? config.RangedWeapon
                : config.MeleeWeapon;
            float nextReady = time + weapon.Cooldown;

            PlayerState updatedPlayer = WithReadyTime(player, player.SelectedWeapon, nextReady);
            Dictionary<EntityId, EnemyState> nextEnemies = new(enemies);
            List<EnemyDamagedFact> damagedFacts = new();
            bool anyHit = false;

            if (updatedPlayer.SelectedWeapon == WeaponKind.Ranged)
            {
                if (resolution.HasRangedHit && nextEnemies.TryGetValue(resolution.RangedHitId, out EnemyState target))
                {
                    anyHit = true;
                    ApplyHit(nextEnemies, target, weapon.Damage, damagedFacts);
                }
            }
            else
            {
                for (int i = 0; i < resolution.MeleeHitIds.Count; i++)
                {
                    EntityId id = resolution.MeleeHitIds[i];

                    if (nextEnemies.TryGetValue(id, out EnemyState target) == false)
                    {
                        continue;
                    }

                    anyHit = true;
                    ApplyHit(nextEnemies, target, weapon.Damage, damagedFacts);
                }
            }

            return new(
                updatedPlayer,
                nextEnemies,
                attackExecuted: true,
                player.SelectedWeapon,
                anyHit,
                damagedFacts);
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
            List<EnemyDamagedFact> damagedFacts)
        {
            float health = CombatRules.ApplyDamage(target.CurrentHealth, damage);
            EnemyState updated = new(
                target.Id,
                target.Kind,
                target.Position,
                health,
                target.MaxHealth,
                target.AttackReadyTime);

            damagedFacts.Add(new Model.Facts.EnemyDamagedFact(updated.Id, damage, health));
            enemies[target.Id] = updated;
        }
    }
}
