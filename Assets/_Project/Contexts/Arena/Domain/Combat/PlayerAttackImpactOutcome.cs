using System;
using Game.Arena.Domain.Aggregates.ArenaRun;
using Game.Arena.Domain.Interactions.Attack;

namespace Game.Arena.Domain.Combat
{
    public sealed class PlayerAttackImpactOutcome
    {
        internal PlayerAttackImpactOutcome()
        {
        }

        private PlayerAttackImpactOutcome(
            PlayerAttackImpactStatus status,
            PlayerAttackImpactRejectionReason rejectionReason,
            ArenaRunChange change)
        {
            Validate(status, rejectionReason);
            Status = status;
            RejectionReason = rejectionReason;
            Change = change ?? throw new ArgumentNullException(nameof(change));
        }

        public PlayerAttackImpactStatus Status { get; private set; }
        public PlayerAttackImpactRejectionReason RejectionReason { get; private set; }
        public ArenaRunChange Change { get; private set; }

        public bool IsAccepted => Status != PlayerAttackImpactStatus.Rejected;

        internal void Fill(
            PlayerAttackImpactStatus status,
            PlayerAttackImpactRejectionReason rejectionReason,
            ArenaRunChange change)
        {
            Validate(status, rejectionReason);
            Status = status;
            RejectionReason = rejectionReason;
            Change = change ?? throw new ArgumentNullException(nameof(change));
        }

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

        private static void Validate(
            PlayerAttackImpactStatus status,
            PlayerAttackImpactRejectionReason rejectionReason)
        {
            bool rejected = status == PlayerAttackImpactStatus.Rejected;
            bool hasReason = rejectionReason != PlayerAttackImpactRejectionReason.None;

            if (rejected != hasReason)
            {
                throw new ArgumentException(nameof(rejectionReason));
            }
        }
    }
}
