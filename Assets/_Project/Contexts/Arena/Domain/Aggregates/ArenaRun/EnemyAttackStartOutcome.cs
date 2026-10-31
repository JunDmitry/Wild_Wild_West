using System;
using System.Collections.Generic;
using Game.Arena.Domain.Combat;

namespace Game.Arena.Domain.Aggregates.ArenaRun
{
    public sealed class EnemyAttackStartOutcome
    {
        private IReadOnlyList<PendingEnemyAttack> _startedAttacks;

        internal EnemyAttackStartOutcome()
        {
            _startedAttacks = Array.Empty<PendingEnemyAttack>();
        }

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

        public EnemyAttackStartStatus Status { get; private set; }
        public ArenaRunChange Change { get; private set; }
        public IReadOnlyList<PendingEnemyAttack> StartedAttacks => _startedAttacks;
        public bool IsStarted => Status == EnemyAttackStartStatus.Started;

        internal void Fill(
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

            Status = status;
            _startedAttacks = startedAttacks;
            Change = change;
        }

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
