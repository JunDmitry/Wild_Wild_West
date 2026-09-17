using Game.Arena.Domain.Combat;

namespace Game.Arena.Domain.Aggregates.ArenaRun
{
    public sealed class PlayerAttackStartOutcome
    {
        private PlayerAttackStartOutcome(
            PlayerAttackStartStatus status,
            PendingAttack attack,
            ArenaRunChange change)
        {
            Status = status;
            Attack = attack;
            Change = change ?? throw new System.ArgumentNullException(nameof(change));
        }

        public PlayerAttackStartStatus Status { get; }
        public PendingAttack Attack { get; }
        public ArenaRunChange Change { get; }

        public bool IsStarted => Status == PlayerAttackStartStatus.Started;

        public static PlayerAttackStartOutcome Started(PendingAttack attack, ArenaRunChange change)
        {
            return new PlayerAttackStartOutcome(PlayerAttackStartStatus.Started, attack, change);
        }

        public static PlayerAttackStartOutcome RunIsNotPlaying(ArenaRunChange change)
        {
            return new PlayerAttackStartOutcome(PlayerAttackStartStatus.RunIsNotPlaying, default, change);
        }

        public static PlayerAttackStartOutcome InteractionPending(ArenaRunChange change)
        {
            return new PlayerAttackStartOutcome(PlayerAttackStartStatus.InteractionPending, default, change);
        }

        public static PlayerAttackStartOutcome AttackAlreadyPending(ArenaRunChange change)
        {
            return new PlayerAttackStartOutcome(PlayerAttackStartStatus.AttackAlreadyPending, default, change);
        }

        public static PlayerAttackStartOutcome WeaponNotReady(ArenaRunChange change)
        {
            return new PlayerAttackStartOutcome(PlayerAttackStartStatus.WeaponNotReady, default, change);
        }
    }
}
