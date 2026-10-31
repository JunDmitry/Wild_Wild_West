using System;
using System.Collections.Generic;
using Game.Arena.Domain.Concurrency;
using Game.Arena.Domain.Events;

namespace Game.Arena.Domain.Aggregates.ArenaRun
{
    public sealed class ArenaRunChange
    {
        private IReadOnlyList<IArenaDomainEvent> _domainEvents;

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

            for (int index = 0; index < domainEvents.Count; index++)
            {
                if (domainEvents[index] == null)
                {
                    throw new ArgumentException("Domain events cannot contain null values.", nameof(domainEvents));
                }
            }

            Fill(revision, hasStateChange, domainEvents);
        }

        internal ArenaRunChange()
        {
            _domainEvents = Array.Empty<IArenaDomainEvent>();
        }

        public AggregateRevision Revision { get; private set; }
        public bool HasStateChange { get; private set; }
        public IReadOnlyList<IArenaDomainEvent> DomainEvents => _domainEvents;

        internal void Fill(
            AggregateRevision revision,
            bool hasStateChange,
            IReadOnlyList<IArenaDomainEvent> domainEvents)
        {
            Revision = revision;
            HasStateChange = hasStateChange;
            _domainEvents = domainEvents;
        }
    }
}
