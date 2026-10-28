using System;
using Game.Arena.Domain.Concurrency;
using Game.Arena.Domain.Identity;

namespace Game.Arena.Domain.Events
{
    public sealed class ArenaRunDefeated : IPooledArenaDomainEvent, IEquatable<ArenaRunDefeated>
    {
        internal ArenaRunDefeated()
        {
        }

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

        public ArenaRunId ArenaRunId { get; private set; }

        public AggregateRevision AggregateRevision { get; private set; }

        internal void Fill(
            ArenaRunId arenaRunId,
            AggregateRevision aggregateRevision)
        {
            ArenaRunId = arenaRunId;
            AggregateRevision = aggregateRevision;
        }

        void IPooledArenaDomainEvent.Reset()
        {
            ArenaRunId = default;
            AggregateRevision = default;
        }

        public bool Equals(ArenaRunDefeated other)
        {
            if (other is null)
            {
                return false;
            }

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
