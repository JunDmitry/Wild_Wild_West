using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using Game.Arena.Domain.Interactions.Contracts;

namespace Game.Arena.Domain.Interactions.Movement
{
    public sealed class EnemyMovementBatchResolution : IInteractionResolution
    {
        private readonly ReadOnlyCollection<EnemyMovementBatchResolutionEntry> _entries;

        public EnemyMovementBatchResolution(InteractionCorrelation correlation, IReadOnlyList<EnemyMovementBatchResolutionEntry> entries)
        {
            if (correlation.ArenaRunId.IsNone)
            {
                throw new ArgumentException("ArenaRunId cannot be None.", nameof(correlation));
            }

            if (correlation.InteractionId.IsNone)
            {
                throw new ArgumentException("InteractionId cannot be None.", nameof(correlation));
            }

            if (entries == null)
            {
                throw new ArgumentNullException(nameof(entries));
            }

            EnemyMovementBatchResolutionEntry[] copied =
                new EnemyMovementBatchResolutionEntry[entries.Count];

            for (int index = 0; index < entries.Count; index++)
            {
                copied[index] = entries[index];
            }

            Correlation = correlation;
            _entries = Array.AsReadOnly(copied);
        }

        public InteractionCorrelation Correlation { get; }

        public InteractionKind Kind => InteractionKind.EnemyMovementBatch;

        public IReadOnlyList<EnemyMovementBatchResolutionEntry> Entries => _entries;
    }
}
