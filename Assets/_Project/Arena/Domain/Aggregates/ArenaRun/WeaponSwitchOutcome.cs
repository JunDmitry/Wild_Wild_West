using System;
using Game.Arena.Domain.Combat;

namespace Game.Arena.Domain.Aggregates.ArenaRun
{
    public sealed class WeaponSwitchOutcome
    {
        private WeaponSwitchOutcome(
            WeaponSwitchStatus status,
            WeaponKind selectedWeapon,
            ArenaRunChange change)
        {
            Status = status;
            SelectedWeapon = selectedWeapon;
            Change = change ?? throw new ArgumentNullException(nameof(change));
        }

        public WeaponSwitchStatus Status { get; }

        public WeaponKind SelectedWeapon { get; }

        public ArenaRunChange Change { get; }

        public bool IsSwitched => Status == WeaponSwitchStatus.Switched;

        public static WeaponSwitchOutcome Switched(
            WeaponKind selectedWeapon,
            ArenaRunChange change)
        {
            return new WeaponSwitchOutcome(WeaponSwitchStatus.Switched, selectedWeapon, change);
        }

        public static WeaponSwitchOutcome RunIsNotPlaying(
            WeaponKind selectedWeapon,
            ArenaRunChange change)
        {
            return new WeaponSwitchOutcome(WeaponSwitchStatus.RunIsNotPlaying, selectedWeapon, change);
        }

        public static WeaponSwitchOutcome InteractionPending(
            WeaponKind selectedWeapon,
            ArenaRunChange change)
        {
            return new WeaponSwitchOutcome(WeaponSwitchStatus.InteractionPending, selectedWeapon, change);
        }

        public static WeaponSwitchOutcome AttackPending(
            WeaponKind selectedWeapon,
            ArenaRunChange change)
        {
            return new WeaponSwitchOutcome(WeaponSwitchStatus.AttackPending, selectedWeapon, change);
        }
    }
}
