using System;
using Game.Arena.Domain.Concurrency;
using Game.Arena.Domain.Identity;

namespace Game.Arena.Domain.Events
{
    public readonly struct ArenaRunDefeated : IArenaDomainEvent, IEquatable<ArenaRunDefeated>
    {
        public ArenaRunDefeated(
            ArenaRunId arenaRunId,
            AggregateRevision aggregateRevision)
        {
            if (arenaRunId.IsNone)
            {
                throw new ArgumentException("ArenaRunId cannot be None.", nameof(arenaRunId));
            }

            ArenaRunId = arenaRunId;
            AggregateRevision = aggregateRevision;
        }

        public ArenaRunId ArenaRunId { get; }

        public AggregateRevision AggregateRevision { get; }

        public bool Equals(ArenaRunDefeated other)
        {
            return ArenaRunId.Equals(other.ArenaRunId)
                && AggregateRevision.Equals(other.AggregateRevision);
        }

        public override bool Equals(object obj)
        {
            return obj is ArenaRunDefeated other && Equals(other);
        }

        public override int GetHashCode()
        {
            return HashCode.Combine(
                ArenaRunId,
                AggregateRevision);
        }
    }
}
