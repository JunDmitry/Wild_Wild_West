using System;
using Game.Arena.Domain.Aggregates.ArenaRun;

namespace Game.Arena.Domain.Interactions.Movement
{
    public sealed class PlayerMovementResolutionOutcome
    {
        internal PlayerMovementResolutionOutcome()
        {
        }

        private PlayerMovementResolutionOutcome(
            PlayerMovementResolutionStatus status,
            PlayerMovementResolutionRejectionReason rejectionReason,
            ArenaRunChange change)
        {
            Validate(status, rejectionReason);
            Status = status;
            RejectionReason = rejectionReason;
            Change = change ?? throw new ArgumentNullException(nameof(change));
        }

        public PlayerMovementResolutionStatus Status { get; private set; }

        public PlayerMovementResolutionRejectionReason RejectionReason { get; private set; }

        public ArenaRunChange Change { get; private set; }

        public bool IsAccepted => Status != PlayerMovementResolutionStatus.Rejected;

        internal void Fill(
            PlayerMovementResolutionStatus status,
            PlayerMovementResolutionRejectionReason rejectionReason,
            ArenaRunChange change)
        {
            Validate(status, rejectionReason);
            Status = status;
            RejectionReason = rejectionReason;
            Change = change ?? throw new ArgumentNullException(nameof(change));
        }

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

        private static void Validate(
            PlayerMovementResolutionStatus status,
            PlayerMovementResolutionRejectionReason rejectionReason)
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
        }
    }
}
