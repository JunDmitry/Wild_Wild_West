using System;
using Game.Arena.Domain.Concurrency;
using Game.Arena.Domain.Identity;

namespace Game.Arena.Domain.Events
{
    public readonly struct PlayerDefeated : IArenaDomainEvent, IEquatable<PlayerDefeated>
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

        public ArenaRunId ArenaRunId { get; }

        public AggregateRevision AggregateRevision { get; }

        public PlayerId PlayerId { get; }

        public bool Equals(PlayerDefeated other)
        {
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
