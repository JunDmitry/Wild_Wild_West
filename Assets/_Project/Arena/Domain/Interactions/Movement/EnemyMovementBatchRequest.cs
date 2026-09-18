using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using Game.Arena.Domain.Interactions.Contracts;

namespace Game.Arena.Domain.Interactions.Movement
{
    public sealed class EnemyMovementBatchRequest : IInteractionRequest
    {
        private readonly ReadOnlyCollection<EnemyMovementIntent> _intents;

        public EnemyMovementBatchRequest(
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

            EnemyMovementIntent[] copied = new EnemyMovementIntent[intents.Count];

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

                copied[index] = intent;
            }

            Correlation = correlation;
            _intents = Array.AsReadOnly(copied);
        }

        public InteractionCorrelation Correlation { get; }

        public InteractionKind Kind => InteractionKind.EnemyMovementBatch;

        public IReadOnlyList<EnemyMovementIntent> Intents => _intents;
    }
}
