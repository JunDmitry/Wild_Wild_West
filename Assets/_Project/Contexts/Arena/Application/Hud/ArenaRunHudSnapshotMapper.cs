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

        public ArenaRunHudSnapshot Map(ArenaRunSnapshot source)
        {
            if (source == null)
            {
                throw new ArgumentNullException(nameof(source));
            }

            Health health = source.Player.Health;

            return new ArenaRunHudSnapshot(
                HealthLabel,
                health.Current,
                health.Maximum,
                EquippedWeaponLabel,
                MapWeapon(source.Player.SelectedWeapon),
                source.Status == ArenaRunStatus.Defeat);
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
