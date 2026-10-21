using System;
using Game.Arena.Domain.Aggregates.ArenaRun;

namespace Game.Arena.Domain.Interactions.Movement
{
    public sealed class EnemyMovementBatchResolutionOutcome
    {
        private EnemyMovementBatchResolutionOutcome(
            EnemyMovementBatchResolutionStatus status,
            EnemyMovementBatchRejectionReason rejectionReason,
            ArenaRunChange change)
        {
            if (change == null)
            {
                throw new ArgumentNullException(nameof(change));
            }

            bool rejected = status == EnemyMovementBatchResolutionStatus.Rejected;
            bool hasReason = rejectionReason != EnemyMovementBatchRejectionReason.None;

            if (rejected != hasReason)
            {
                throw new ArgumentException(nameof(rejectionReason));
            }

            Status = status;
            RejectionReason = rejectionReason;
            Change = change;
        }

        public EnemyMovementBatchResolutionStatus Status { get; }

        public EnemyMovementBatchRejectionReason RejectionReason { get; }

        public ArenaRunChange Change { get; }

        public bool IsAccepted => Status != EnemyMovementBatchResolutionStatus.Rejected;

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
    }
}
