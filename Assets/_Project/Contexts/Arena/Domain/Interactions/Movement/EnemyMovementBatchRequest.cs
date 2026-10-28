using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using Game.Arena.Domain.Identity;
using Game.Arena.Domain.Interactions.Contracts;

namespace Game.Arena.Domain.Interactions.Movement
{
    public sealed class EnemyMovementBatchRequest : IInteractionRequest
    {
        private readonly HashSet<EnemyId> _seen = new();
        private IReadOnlyList<EnemyMovementIntent> _intents;

        internal EnemyMovementBatchRequest()
        {
            _intents = Array.Empty<EnemyMovementIntent>();
        }

        public EnemyMovementBatchRequest(
            InteractionCorrelation correlation,
            IReadOnlyList<EnemyMovementIntent> intents)
        {
            Validate(correlation, intents);

            EnemyMovementIntent[] copied = new EnemyMovementIntent[intents.Count];

            for (int index = 0; index < intents.Count; index++)
            {
                copied[index] = intents[index];
            }

            Correlation = correlation;
            _intents = Array.AsReadOnly(copied);
        }

        public InteractionCorrelation Correlation { get; private set; }

        public InteractionKind Kind => InteractionKind.EnemyMovementBatch;

        public IReadOnlyList<EnemyMovementIntent> Intents => _intents;

        internal void Fill(
            InteractionCorrelation correlation,
            IReadOnlyList<EnemyMovementIntent> intents)
        {
            Validate(correlation, intents);

            Correlation = correlation;
            _intents = intents;
        }

        private void Validate(
            InteractionCorrelation correlation,
            IReadOnlyList<EnemyMovementIntent> intents)
        {
            if (correlation.ArenaRunId.IsNone)
            {
                throw new ArgumentException("ArenaRunId cannot be None.", nameof(correlation));
            }

            if (correlation.InteractionId.IsNone)
            {
                throw new ArgumentException("InteractionId cannot be None.", nameof(correlation));
            }

            if (intents == null)
            {
                throw new ArgumentNullException(nameof(intents));
            }

            if (intents.Count == 0)
            {
                throw new ArgumentException("At least one movement intent is required.", nameof(intents));
            }

            _seen.Clear();

            for (int index = 0; index < intents.Count; index++)
            {
                EnemyMovementIntent intent = intents[index];

                if (intent.EnemyId.IsNone)
                {
                    throw new ArgumentException("EnemyMovementIntent cannot contain None EnemyId.", nameof(intents));
                }

                if (intent.Intent.IsValid == false)
                {
                    throw new ArgumentException("EnemyMovementIntent contains invalid movement intent.", nameof(intents));
                }

                if (_seen.Add(intent.EnemyId) == false)
                {
                    throw new ArgumentException("EnemyMovementIntent contains duplicate EnemyId.", nameof(intents));
                }
            }
        }
    }
}
