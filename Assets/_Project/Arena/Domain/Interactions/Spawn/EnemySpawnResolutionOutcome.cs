using System;
using Game.Arena.Domain.Aggregates.ArenaRun;

namespace Game.Arena.Domain.Interactions.Spawn
{
    public sealed class EnemySpawnResolutionOutcome
    {
        private EnemySpawnResolutionOutcome(
            EnemySpawnResolutionStatus status,
            EnemySpawnResolutionRejectionReason rejectionReason,
            ArenaRunChange change)
        {
            bool rejected = status == EnemySpawnResolutionStatus.Rejected;
            bool hasReason = rejectionReason != EnemySpawnResolutionRejectionReason.None;

            if (rejected != hasReason)
            {
                throw new ArgumentException(nameof(rejectionReason));
            }

            Status = status;
            RejectionReason = rejectionReason;
            Change = change ?? throw new ArgumentNullException(nameof(change));
        }

        public EnemySpawnResolutionStatus Status { get; }

        public EnemySpawnResolutionRejectionReason RejectionReason { get; }

        public ArenaRunChange Change { get; }

        public bool IsSpawned => Status == EnemySpawnResolutionStatus.Spawned;

        public static EnemySpawnResolutionOutcome Spawned(ArenaRunChange change)
        {
            return new EnemySpawnResolutionOutcome(EnemySpawnResolutionStatus.Spawned, EnemySpawnResolutionRejectionReason.None, change);
        }

        public static EnemySpawnResolutionOutcome Rejected(EnemySpawnResolutionRejectionReason reason, ArenaRunChange change)
        {
            return new EnemySpawnResolutionOutcome(EnemySpawnResolutionStatus.Rejected, reason, change);
        }
    }
}
