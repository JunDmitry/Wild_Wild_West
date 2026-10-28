using System;

namespace Game.Arena.Domain.Aggregates.ArenaRun
{
    public sealed class EnemyAttackImpactOutcome
    {
        internal EnemyAttackImpactOutcome()
        {
        }

        private EnemyAttackImpactOutcome(
            EnemyAttackImpactStatus status,
            int resolvedAttackCount,
            int hitCount,
            ArenaRunChange change)
        {
            Validate(resolvedAttackCount, hitCount);
            Status = status;
            ResolvedAttackCount = resolvedAttackCount;
            HitCount = hitCount;
            Change = change ?? throw new ArgumentNullException(nameof(change));
        }

        public EnemyAttackImpactStatus Status { get; private set; }

        public int ResolvedAttackCount { get; private set; }

        public int HitCount { get; private set; }

        public ArenaRunChange Change { get; private set; }

        public bool IsResolved => Status == EnemyAttackImpactStatus.Resolved;

        internal void Fill(
            EnemyAttackImpactStatus status,
            int resolvedAttackCount,
            int hitCount,
            ArenaRunChange change)
        {
            Validate(resolvedAttackCount, hitCount);
            Status = status;
            ResolvedAttackCount = resolvedAttackCount;
            HitCount = hitCount;
            Change = change ?? throw new ArgumentNullException(nameof(change));
        }

        public static EnemyAttackImpactOutcome Resolved(
            int resolvedAttackCount,
            int hitCount,
            ArenaRunChange change)
        {
            if (resolvedAttackCount == 0)
            {
                throw new ArgumentOutOfRangeException(nameof(resolvedAttackCount));
            }

            return new EnemyAttackImpactOutcome(
                EnemyAttackImpactStatus.Resolved,
                resolvedAttackCount,
                hitCount,
                change);
        }

        public static EnemyAttackImpactOutcome NoAttacksDue(ArenaRunChange change)
        {
            return new EnemyAttackImpactOutcome(
                EnemyAttackImpactStatus.NoAttacksDue,
                0,
                0,
                change);
        }

        public static EnemyAttackImpactOutcome RunIsNotPlaying(ArenaRunChange change)
        {
            return new EnemyAttackImpactOutcome(
                EnemyAttackImpactStatus.RunIsNotPlaying,
                0,
                0,
                change);
        }

        public static EnemyAttackImpactOutcome InteractionPending(ArenaRunChange change)
        {
            return new EnemyAttackImpactOutcome(
                EnemyAttackImpactStatus.InteractionPending,
                0,
                0,
                change);
        }

        private static void Validate(int resolvedAttackCount, int hitCount)
        {
            if (resolvedAttackCount < 0)
            {
                throw new ArgumentOutOfRangeException(nameof(resolvedAttackCount));
            }

            if (hitCount < 0 || hitCount > resolvedAttackCount)
            {
                throw new ArgumentOutOfRangeException(nameof(hitCount));
            }
        }
    }
}
