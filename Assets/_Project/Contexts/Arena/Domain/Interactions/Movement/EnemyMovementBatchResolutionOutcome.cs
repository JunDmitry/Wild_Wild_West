using System;
using Game.Arena.Domain.Aggregates.ArenaRun;

namespace Game.Arena.Domain.Interactions.Movement
{
    public sealed class EnemyMovementBatchResolutionOutcome
    {
        internal EnemyMovementBatchResolutionOutcome()
        {
        }

        private EnemyMovementBatchResolutionOutcome(
            EnemyMovementBatchResolutionStatus status,
            EnemyMovementBatchRejectionReason rejectionReason,
            ArenaRunChange change)
        {
            Validate(status, rejectionReason);
            Status = status;
            RejectionReason = rejectionReason;
            Change = change ?? throw new ArgumentNullException(nameof(change));
        }

        public EnemyMovementBatchResolutionStatus Status { get; private set; }

        public EnemyMovementBatchRejectionReason RejectionReason { get; private set; }

        public ArenaRunChange Change { get; private set; }

        public bool IsAccepted => Status != EnemyMovementBatchResolutionStatus.Rejected;

        internal void Fill(
            EnemyMovementBatchResolutionStatus status,
            EnemyMovementBatchRejectionReason rejectionReason,
            ArenaRunChange change)
        {
            Validate(status, rejectionReason);
            Status = status;
            RejectionReason = rejectionReason;
            Change = change ?? throw new ArgumentNullException(nameof(change));
        }

        public static EnemyMovementBatchResolutionOutcome Applied(ArenaRunChange change)
        {
            return new EnemyMovementBatchResolutionOutcome(EnemyMovementBatchResolutionStatus.Applied, EnemyMovementBatchRejectionReason.None, change);
        }

        public static EnemyMovementBatchResolutionOutcome AcceptedWithoutStateChange(ArenaRunChange change)
        {
            return new EnemyMovementBatchResolutionOutcome(EnemyMovementBatchResolutionStatus.AcceptedWithoutStateChange, EnemyMovementBatchRejectionReason.None, change);
        }

        public static EnemyMovementBatchResolutionOutcome Rejected(
            EnemyMovementBatchRejectionReason reason,
            ArenaRunChange change)
        {
            return new EnemyMovementBatchResolutionOutcome(EnemyMovementBatchResolutionStatus.Rejected, reason, change);
        }

        private static void Validate(
            EnemyMovementBatchResolutionStatus status,
            EnemyMovementBatchRejectionReason rejectionReason)
        {
            bool rejected = status == EnemyMovementBatchResolutionStatus.Rejected;
            bool hasReason = rejectionReason != EnemyMovementBatchRejectionReason.None;

            if (rejected != hasReason)
            {
                throw new ArgumentException(nameof(rejectionReason));
            }
        }
    }
}
