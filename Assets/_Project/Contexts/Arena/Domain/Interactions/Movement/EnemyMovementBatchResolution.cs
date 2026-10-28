using System;
using System.Collections.Generic;
using Game.Arena.Domain.Interactions.Contracts;

namespace Game.Arena.Domain.Interactions.Movement
{
    public sealed class EnemyMovementBatchResolution : IInteractionResolution
    {
        private readonly List<EnemyMovementBatchResolutionEntry> _entriesBuffer;

        public EnemyMovementBatchResolution(InteractionCorrelation correlation, IReadOnlyList<EnemyMovementBatchResolutionEntry> entries)
        {
            _entriesBuffer = new List<EnemyMovementBatchResolutionEntry>(entries == null ? 0 : entries.Count);
            Entries = _entriesBuffer.AsReadOnly();
            Fill(correlation, entries);
        }

        /// <summary>
        /// Creates an empty shell for the per-tick reuse path (ADR-0035
        /// requirement 9). Fill it before handing it to the aggregate.
        /// </summary>
        public EnemyMovementBatchResolution(int capacity)
        {
            if (capacity < 0)
            {
                throw new ArgumentOutOfRangeException(nameof(capacity));
            }

            _entriesBuffer = new List<EnemyMovementBatchResolutionEntry>(capacity);
            Entries = _entriesBuffer.AsReadOnly();
        }

        public InteractionCorrelation Correlation { get; private set; }

        public InteractionKind Kind => InteractionKind.EnemyMovementBatch;

        public IReadOnlyList<EnemyMovementBatchResolutionEntry> Entries { get; }

        public void Fill(
            InteractionCorrelation correlation,
            IReadOnlyList<EnemyMovementBatchResolutionEntry> entries)
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

            Correlation = correlation;
            _entriesBuffer.Clear();

            for (int index = 0; index < entries.Count; index++)
            {
                _entriesBuffer.Add(entries[index]);
            }
        }
    }
}
