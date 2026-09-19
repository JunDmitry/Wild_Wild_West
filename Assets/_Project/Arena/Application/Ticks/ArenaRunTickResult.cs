using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using Game.Arena.Domain.Concurrency;
using Game.Arena.Domain.Events;
using Game.Arena.Domain.Identity;

namespace Game.Arena.Application.Ticks
{
    public sealed class ArenaRunTickResult
    {
        private readonly ReadOnlyCollection<IArenaDomainEvent> _domainEvents;
        private readonly ReadOnlyCollection<ArenaRunTickStage> _executedStages;

        public ArenaRunTickResult(
            ArenaRunId arenaRunId,
            AggregateRevision finalRevision,
            IReadOnlyList<IArenaDomainEvent> domainEvents,
            IReadOnlyList<ArenaRunTickStage> executedStages)
        {
            if (arenaRunId.IsNone)
            {
                throw new ArgumentException("ArenaRunId cannot be None.", nameof(arenaRunId));
            }

            if (domainEvents == null)
            {
                throw new ArgumentNullException(nameof(domainEvents));
            }

            if (executedStages == null)
            {
                throw new ArgumentNullException(nameof(executedStages));
            }

            IArenaDomainEvent[] copiedEvents = CopyEvents(arenaRunId, finalRevision, domainEvents);
            ArenaRunTickStage[] copiedStages = CopyStages(executedStages);

            ArenaRunId = arenaRunId;
            FinalRevision = finalRevision;
            _domainEvents = Array.AsReadOnly(copiedEvents);
            _executedStages = Array.AsReadOnly(copiedStages);
        }

        public ArenaRunId ArenaRunId { get; }
        public AggregateRevision FinalRevision { get; }
        public IReadOnlyList<IArenaDomainEvent> DomainEvents => _domainEvents;
        public IReadOnlyList<ArenaRunTickStage> ExecutedStages => _executedStages;

        private static IArenaDomainEvent[] CopyEvents(ArenaRunId arenaRunId, AggregateRevision finalRevision, IReadOnlyList<IArenaDomainEvent> domainEvents)
        {
            IArenaDomainEvent[] copiedEvents = new IArenaDomainEvent[domainEvents.Count];
            AggregateRevision previousRevision = AggregateRevision.Initial;

            for (int index = 0; index < domainEvents.Count; index++)
            {
                IArenaDomainEvent domainEvent = domainEvents[index];

                if (domainEvent == null)
                {
                    throw new ArgumentException("Domain events cannot contain null.", nameof(domainEvents));
                }

                if (domainEvent.ArenaRunId != arenaRunId)
                {
                    throw new ArgumentException("Domain event belongs to another ArenaRun.", nameof(domainEvents));
                }

                if (domainEvent.AggregateRevision > finalRevision)
                {
                    throw new ArgumentException("Domain event revision exceeds the final revision.", nameof(domainEvents));
                }

                if (domainEvent.AggregateRevision < previousRevision)
                {
                    throw new ArgumentException("Domain event revisions must not decrease.", nameof(domainEvents));
                }

                copiedEvents[index] = domainEvent;
                previousRevision = domainEvent.AggregateRevision;
            }

            return copiedEvents;
        }

        private static ArenaRunTickStage[] CopyStages(IReadOnlyList<ArenaRunTickStage> executedStages)
        {
            ArenaRunTickStage[] copiedStages = new ArenaRunTickStage[executedStages.Count];

            for (int index = 0; index < executedStages.Count; index++)
            {
                ArenaRunTickStage stage = executedStages[index];

                if (Enum.IsDefined(typeof(ArenaRunTickStage), stage) == false)
                {
                    throw new ArgumentException("Executed stages contain an unknown value.", nameof(executedStages));
                }

                copiedStages[index] = stage;
            }

            return copiedStages;
        }
    }
}
