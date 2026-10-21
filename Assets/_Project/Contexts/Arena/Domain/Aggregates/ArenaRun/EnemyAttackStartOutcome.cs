using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using Game.Arena.Domain.Combat;

namespace Game.Arena.Domain.Aggregates.ArenaRun
{
    public sealed class EnemyAttackStartOutcome
    {
        private readonly ReadOnlyCollection<PendingEnemyAttack> _startedAttacks;

        private EnemyAttackStartOutcome(
            EnemyAttackStartStatus status,
            IReadOnlyList<PendingEnemyAttack> startedAttacks,
            ArenaRunChange change)
        {
            if (startedAttacks == null)
            {
                throw new ArgumentNullException(nameof(startedAttacks));
            }

            if (change == null)
            {
                throw new ArgumentNullException(nameof(change));
            }

            PendingEnemyAttack[] copied = new PendingEnemyAttack[startedAttacks.Count];

            for (int index = 0; index < startedAttacks.Count; index++)
            {
                copied[index] = startedAttacks[index];
            }

            Status = status;
            _startedAttacks = Array.AsReadOnly(copied);
            Change = change;
        }

        public EnemyAttackStartStatus Status { get; }
        public ArenaRunChange Change { get; }
        public IReadOnlyList<PendingEnemyAttack> StartedAttacks => _startedAttacks;
        public bool IsStarted => Status == EnemyAttackStartStatus.Started;

        public static EnemyAttackStartOutcome Started(IReadOnlyList<PendingEnemyAttack> startedAttacks, ArenaRunChange change)
        {
            if (startedAttacks == null)
            {
                throw new ArgumentNullException(nameof(startedAttacks));
            }

            if (startedAttacks.Count == 0)
            {
                throw new ArgumentException("At least one attack is required.", nameof(startedAttacks));
            }

            return new EnemyAttackStartOutcome(EnemyAttackStartStatus.Started, startedAttacks, change);
        }

        public static EnemyAttackStartOutcome NoEligibleEnemies(ArenaRunChange change)
        {
            return new EnemyAttackStartOutcome(EnemyAttackStartStatus.NoEligibleEnemies, Array.Empty<PendingEnemyAttack>(), change);
        }

        public static EnemyAttackStartOutcome RunIsNotPlaying(ArenaRunChange change)
        {
            return new EnemyAttackStartOutcome(EnemyAttackStartStatus.RunIsNotPlaying, Array.Empty<PendingEnemyAttack>(), change);
        }

        public static EnemyAttackStartOutcome InteractionPending(ArenaRunChange change)
        {
            return new EnemyAttackStartOutcome(EnemyAttackStartStatus.InteractionPending, Array.Empty<PendingEnemyAttack>(), change);
        }
    }
}
