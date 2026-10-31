using System;
using Game.Arena.Domain.Concurrency;
using Game.Arena.Domain.Identity;

namespace Game.Arena.Domain.Events
{
    public sealed class PlayerDefeated : IPooledArenaDomainEvent, IEquatable<PlayerDefeated>
    {
        public PlayerDefeated(
            ArenaRunId arenaRunId,
            AggregateRevision aggregateRevision,
            PlayerId playerId)
        {
            if (arenaRunId.IsNone)
            {
                throw new ArgumentException("ArenaRunId cannot be None.", nameof(arenaRunId));
            }

            if (playerId.IsNone)
            {
                throw new ArgumentException("PlayerId cannot be None.", nameof(playerId));
            }

            ArenaRunId = arenaRunId;
            AggregateRevision = aggregateRevision;
            PlayerId = playerId;
        }

        internal PlayerDefeated()
        {
        }

        public ArenaRunId ArenaRunId { get; private set; }

        public AggregateRevision AggregateRevision { get; private set; }

        public PlayerId PlayerId { get; private set; }

        internal void Fill(
            ArenaRunId arenaRunId,
            AggregateRevision aggregateRevision,
            PlayerId playerId)
        {
            ArenaRunId = arenaRunId;
            AggregateRevision = aggregateRevision;
            PlayerId = playerId;
        }

        void IPooledArenaDomainEvent.Reset()
        {
            ArenaRunId = default;
            AggregateRevision = default;
            PlayerId = default;
        }

        public bool Equals(PlayerDefeated other)
        {
            if (other is null)
            {
                return false;
            }

            return ArenaRunId.Equals(other.ArenaRunId)
                && AggregateRevision.Equals(other.AggregateRevision)
                && PlayerId.Equals(other.PlayerId);
        }

        public override bool Equals(object obj)
        {
            return obj is PlayerDefeated other && Equals(other);
        }

        public override int GetHashCode()
        {
            return HashCode.Combine(ArenaRunId, AggregateRevision, PlayerId);
        }
    }
}
