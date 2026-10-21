using System;
using Game.Arena.Domain.Combat;
using Game.Arena.Domain.Concurrency;
using Game.Arena.Domain.Identity;

namespace Game.Arena.Domain.Events
{
    public readonly struct PlayerAttackCancelled : IArenaDomainEvent, IEquatable<PlayerAttackCancelled>
    {
        public PlayerAttackCancelled(
            ArenaRunId arenaRunId,
            AggregateRevision aggregateRevision,
            PlayerId playerId,
            AttackId attackId,
            AttackCancellationCause cause)
        {
            if (arenaRunId.IsNone)
            {
                throw new ArgumentException("ArenaRunId cannot be None.", nameof(arenaRunId));
            }

            if (playerId.IsNone)
            {
                throw new ArgumentException("PlayerId cannot be None.", nameof(playerId));
            }

            if (attackId.IsNone)
            {
                throw new ArgumentException("AttackId cannot be None.", nameof(attackId));
            }

            ArenaRunId = arenaRunId;
            AggregateRevision = aggregateRevision;
            PlayerId = playerId;
            AttackId = attackId;
            Cause = cause;
        }

        public ArenaRunId ArenaRunId { get; }

        public AggregateRevision AggregateRevision { get; }

        public PlayerId PlayerId { get; }

        public AttackId AttackId { get; }

        public AttackCancellationCause Cause { get; }

        public bool Equals(PlayerAttackCancelled other)
        {
            return ArenaRunId.Equals(other.ArenaRunId)
                && AggregateRevision.Equals(other.AggregateRevision)
                && PlayerId.Equals(other.PlayerId)
                && AttackId.Equals(other.AttackId)
                && Cause == other.Cause;
        }

        public override bool Equals(object obj)
        {
            return obj is PlayerAttackCancelled other && Equals(other);
        }

        public override int GetHashCode()
        {
            return HashCode.Combine(
                ArenaRunId,
                AggregateRevision,
                PlayerId,
                AttackId,
                Cause);
        }
    }
}
