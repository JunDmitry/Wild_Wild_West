using System;
using Game.Arena.Domain.Aggregates.ArenaRun;
using Game.Arena.Domain.Interactions.Attack;

namespace Game.Arena.Domain.Combat
{
    public sealed class PlayerAttackImpactOutcome
    {
        private PlayerAttackImpactOutcome(
            PlayerAttackImpactStatus status,
            PlayerAttackImpactRejectionReason rejectionReason,
            ArenaRunChange change)
        {
            if (change == null)
            {
                throw new ArgumentNullException(nameof(change));
            }

            bool rejected = status == PlayerAttackImpactStatus.Rejected;
            bool hasReason = rejectionReason != PlayerAttackImpactRejectionReason.None;

            if (rejected != hasReason)
            {
                throw new ArgumentException(nameof(rejectionReason));
            }

            Status = status;
            RejectionReason = rejectionReason;
            Change = change;
        }

        public PlayerAttackImpactStatus Status { get; }
        public PlayerAttackImpactRejectionReason RejectionReason { get; }
        public ArenaRunChange Change { get; }

        public bool IsAccepted => Status != PlayerAttackImpactStatus.Rejected;

        public static PlayerAttackImpactOutcome Hit(ArenaRunChange change)
        {
            return new PlayerAttackImpactOutcome(PlayerAttackImpactStatus.Hit, PlayerAttackImpactRejectionReason.None, change);
        }

        public static PlayerAttackImpactOutcome Missed(ArenaRunChange change)
        {
            return new PlayerAttackImpactOutcome(PlayerAttackImpactStatus.Missed, PlayerAttackImpactRejectionReason.None, change);
        }

        public static PlayerAttackImpactOutcome Rejected(PlayerAttackImpactRejectionReason reason, ArenaRunChange change)
        {
            return new PlayerAttackImpactOutcome(PlayerAttackImpactStatus.Rejected, reason, change);
        }
    }
}
