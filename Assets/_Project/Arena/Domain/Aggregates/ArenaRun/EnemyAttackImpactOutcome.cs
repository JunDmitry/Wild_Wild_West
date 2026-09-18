using System;

namespace Game.Arena.Domain.Aggregates.ArenaRun
{
    public sealed class EnemyAttackImpactOutcome
    {
        private EnemyAttackImpactOutcome(
            EnemyAttackImpactStatus status,
            int resolvedAttackCount,
            int hitCount,
            ArenaRunChange change)
        {
            if (change == null)
            {
                throw new ArgumentNullException(nameof(change));
            }

            if (resolvedAttackCount < 0)
            {
                throw new ArgumentOutOfRangeException(nameof(resolvedAttackCount));
            }

            if (hitCount < 0 || hitCount > resolvedAttackCount)
            {
                throw new ArgumentOutOfRangeException(nameof(hitCount));
            }

            Status = status;
            ResolvedAttackCount = resolvedAttackCount;
            HitCount = hitCount;
            Change = change;
        }

        public EnemyAttackImpactStatus Status { get; }

        public int ResolvedAttackCount { get; }

        public int HitCount { get; }

        public ArenaRunChange Change { get; }

        public bool IsResolved => Status == EnemyAttackImpactStatus.Resolved;

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

        public static EnemyAttackImpactOutcome DefeatNotSupported(ArenaRunChange change)
        {
            return new EnemyAttackImpactOutcome(
                EnemyAttackImpactStatus.DefeatNotSupported,
                0,
                0,
                change);
        }
    }
}
