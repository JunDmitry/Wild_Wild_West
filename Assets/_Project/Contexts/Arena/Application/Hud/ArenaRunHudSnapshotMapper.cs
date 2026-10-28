using System;
using Game.Arena.Application.ReadModels;
using Game.Arena.Domain.Aggregates.ArenaRun;
using Game.Arena.Domain.Combat;
using Game.Arena.Domain.Vitality;

namespace Game.Arena.Application.Hud
{
    public sealed class ArenaRunHudSnapshotMapper
    {
        private const string HealthLabel = "Health";
        private const string EquippedWeaponLabel = "Equipped weapon";
        private const string BossLabel = "Boss";

        public ArenaRunHudSnapshot Map(ArenaRunSnapshot source)
        {
            if (source == null)
            {
                throw new ArgumentNullException(nameof(source));
            }

            ArenaRunHudSnapshot target = new();
            Map(source, target);

            return target;
        }

        internal void Map(ArenaRunSnapshot source, ArenaRunHudSnapshot target)
        {
            if (source == null)
            {
                throw new ArgumentNullException(nameof(source));
            }

            if (target == null)
            {
                throw new ArgumentNullException(nameof(target));
            }

            Health health = source.Player.Health;
            EnemySnapshot boss = FindPresentedBoss(source);

            target.Fill(
                HealthLabel,
                health.Current,
                health.Maximum,
                EquippedWeaponLabel,
                MapWeapon(source.Player.SelectedWeapon),
                source.Wave.Number.Value,
                source.WaveTotal.Value,
                source.Status == ArenaRunStatus.Defeat,
                BossLabel,
                boss == null ? 0 : boss.Health.Current,
                boss == null ? 0 : boss.Health.Maximum,
                boss != null);
        }

        private static EnemySnapshot FindPresentedBoss(ArenaRunSnapshot source)
        {
            for (int index = 0; index < source.Enemies.Count; index++)
            {
                EnemySnapshot enemy = source.Enemies[index];

                if (enemy.Kind != EnemyKind.Boss)
                {
                    continue;
                }

                if (source.Bounds.Contains(enemy.Position, enemy.CollisionRadius) == false)
                {
                    continue;
                }

                return enemy;
            }

            return null;
        }

        private static string MapWeapon(WeaponKind weapon)
        {
            switch (weapon)
            {
                case WeaponKind.Ranged:
                    return "Firearm";
                case WeaponKind.Melee:
                    return "Melee Weapon";
                default:
                    throw new ArgumentOutOfRangeException(
                        nameof(weapon),
                        weapon,
                        "Unknown weapon kind.");
            }
        }
    }
}
