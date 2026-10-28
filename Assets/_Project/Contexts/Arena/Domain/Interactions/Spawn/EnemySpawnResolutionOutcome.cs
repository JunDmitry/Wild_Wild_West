using System;
using Game.Arena.Domain.Aggregates.ArenaRun;

namespace Game.Arena.Domain.Interactions.Spawn
{
    public sealed class EnemySpawnResolutionOutcome
    {
        internal EnemySpawnResolutionOutcome()
        {
        }

        private EnemySpawnResolutionOutcome(
            EnemySpawnResolutionStatus status,
            EnemySpawnResolutionRejectionReason rejectionReason,
            ArenaRunChange change)
        {
            Validate(status, rejectionReason);
            Status = status;
            RejectionReason = rejectionReason;
            Change = change ?? throw new ArgumentNullException(nameof(change));
        }

        public EnemySpawnResolutionStatus Status { get; private set; }

        public EnemySpawnResolutionRejectionReason RejectionReason { get; private set; }

        public ArenaRunChange Change { get; private set; }

        public bool IsSpawned => Status == EnemySpawnResolutionStatus.Spawned;

        internal void Fill(
            EnemySpawnResolutionStatus status,
            EnemySpawnResolutionRejectionReason rejectionReason,
            ArenaRunChange change)
        {
            Validate(status, rejectionReason);
            Status = status;
            RejectionReason = rejectionReason;
            Change = change ?? throw new ArgumentNullException(nameof(change));
        }

        public static EnemySpawnResolutionOutcome Spawned(ArenaRunChange change)
        {
            return new EnemySpawnResolutionOutcome(EnemySpawnResolutionStatus.Spawned, EnemySpawnResolutionRejectionReason.None, change);
        }

        public static EnemySpawnResolutionOutcome Rejected(EnemySpawnResolutionRejectionReason reason, ArenaRunChange change)
        {
            return new EnemySpawnResolutionOutcome(EnemySpawnResolutionStatus.Rejected, reason, change);
        }

        private static void Validate(
            EnemySpawnResolutionStatus status,
            EnemySpawnResolutionRejectionReason rejectionReason)
        {
            bool rejected = status == EnemySpawnResolutionStatus.Rejected;
            bool hasReason = rejectionReason != EnemySpawnResolutionRejectionReason.None;

            if (rejected != hasReason)
            {
                throw new ArgumentException(nameof(rejectionReason));
            }
        }
    }
}
