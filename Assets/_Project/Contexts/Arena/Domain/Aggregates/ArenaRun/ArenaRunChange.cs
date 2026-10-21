using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using Game.Arena.Domain.Concurrency;
using Game.Arena.Domain.Events;

namespace Game.Arena.Domain.Aggregates.ArenaRun
{
    public sealed class ArenaRunChange
    {
        private readonly ReadOnlyCollection<IArenaDomainEvent> _eventsSnapshot;

        public ArenaRunChange(
            AggregateRevision revision,
            IReadOnlyList<IArenaDomainEvent> domainEvents)
            : this(revision, false, domainEvents)
        {
        }

        public ArenaRunChange(AggregateRevision revision, bool hasStateChange, IReadOnlyList<IArenaDomainEvent> domainEvents)
        {
            if (domainEvents == null)
            {
                throw new ArgumentNullException(nameof(domainEvents));
            }

            IArenaDomainEvent[] events = new IArenaDomainEvent[domainEvents.Count];

            for (int index = 0; index < domainEvents.Count; index++)
            {
                IArenaDomainEvent domainEvent = domainEvents[index];

                events[index] = domainEvent ?? throw new ArgumentException("Domain events cannot contain null values.", nameof(domainEvents));
            }

            Revision = revision;
            HasStateChange = hasStateChange;
            _eventsSnapshot = Array.AsReadOnly(events);
        }

        public AggregateRevision Revision { get; }
        public bool HasStateChange { get; }
        public IReadOnlyList<IArenaDomainEvent> DomainEvents => _eventsSnapshot;
    }
}
