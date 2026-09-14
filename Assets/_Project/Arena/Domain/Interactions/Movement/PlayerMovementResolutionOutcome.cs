using System;
using Game.Arena.Domain.Aggregates.ArenaRun;

namespace Game.Arena.Domain.Interactions.Movement
{
    public sealed class PlayerMovementResolutionOutcome
    {
        private PlayerMovementResolutionOutcome(
            PlayerMovementResolutionStatus status,
            PlayerMovementResolutionRejectionReason rejectionReason,
            ArenaRunChange change)
        {
            if (status == PlayerMovementResolutionStatus.Rejected
                && rejectionReason == PlayerMovementResolutionRejectionReason.None)
            {
                throw new ArgumentException(nameof(rejectionReason));
            }

            if (status != PlayerMovementResolutionStatus.Rejected
                && rejectionReason != PlayerMovementResolutionRejectionReason.None)
            {
                throw new ArgumentException(nameof(rejectionReason));
            }

            Status = status;
            RejectionReason = rejectionReason;
            Change = change ?? throw new ArgumentNullException(nameof(change));
        }

        public PlayerMovementResolutionStatus Status { get; }

        public PlayerMovementResolutionRejectionReason RejectionReason { get; }

        public ArenaRunChange Change { get; }

        public bool IsAccepted => Status != PlayerMovementResolutionStatus.Rejected;

        public static PlayerMovementResolutionOutcome Applied(
            ArenaRunChange change)
        {
            return new PlayerMovementResolutionOutcome(
                PlayerMovementResolutionStatus.Applied,
                PlayerMovementResolutionRejectionReason.None,
                change);
        }

        public static PlayerMovementResolutionOutcome AcceptedWithoutStateChange(
            ArenaRunChange change)
        {
            return new PlayerMovementResolutionOutcome(
                PlayerMovementResolutionStatus.AcceptedWithoutStateChange,
                PlayerMovementResolutionRejectionReason.None,
                change);
        }

        public static PlayerMovementResolutionOutcome Rejected(
            PlayerMovementResolutionRejectionReason reason,
            ArenaRunChange change)
        {
            return new PlayerMovementResolutionOutcome(
                PlayerMovementResolutionStatus.Rejected,
                reason,
                change);
        }
    }
}
