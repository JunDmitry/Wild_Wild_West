using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using Game.Arena.Domain.Concurrency;
using Game.Arena.Domain.Events;

namespace Game.Arena.Domain.Aggregates.ArenaRun
{
    public sealed class ArenaRunChange
    {
        private readonly ReadOnlyCollection<IDomainEvent> _domainEvents;

        public ArenaRunChange(AggregateRevision revision, IReadOnlyList<IDomainEvent> domainEvents)
        {
            if (domainEvents == null)
            {
                throw new ArgumentNullException(nameof(domainEvents));
            }

            IDomainEvent[] events = new IDomainEvent[domainEvents.Count];

            for (int index = 0; index < domainEvents.Count; index++)
            {
                IDomainEvent domainEvent = domainEvents[index];

                events[index] = domainEvent ?? throw new ArgumentException("Domain events cannot contain null values.", nameof(domainEvents));
            }

            Revision = revision;
            _domainEvents = Array.AsReadOnly(events);
        }

        public AggregateRevision Revision { get; }

        public IReadOnlyList<IDomainEvent> DomainEvents => _domainEvents;
    }
}
