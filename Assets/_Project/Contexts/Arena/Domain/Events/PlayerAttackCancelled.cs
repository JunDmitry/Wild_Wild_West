using System;
using Game.Arena.Domain.Combat;
using Game.Arena.Domain.Concurrency;
using Game.Arena.Domain.Identity;

namespace Game.Arena.Domain.Events
{
    public sealed class PlayerAttackCancelled : IPooledArenaDomainEvent, IEquatable<PlayerAttackCancelled>
    {
        internal PlayerAttackCancelled()
        {
        }

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

        public ArenaRunId ArenaRunId { get; private set; }

        public AggregateRevision AggregateRevision { get; private set; }

        public PlayerId PlayerId { get; private set; }

        public AttackId AttackId { get; private set; }

        public AttackCancellationCause Cause { get; private set; }

        internal void Fill(
            ArenaRunId arenaRunId,
            AggregateRevision aggregateRevision,
            PlayerId playerId,
            AttackId attackId,
            AttackCancellationCause cause)
        {
            ArenaRunId = arenaRunId;
            AggregateRevision = aggregateRevision;
            PlayerId = playerId;
            AttackId = attackId;
            Cause = cause;
        }

        void IPooledArenaDomainEvent.Reset()
        {
            ArenaRunId = default;
            AggregateRevision = default;
            PlayerId = default;
            AttackId = default;
            Cause = default;
        }

        public bool Equals(PlayerAttackCancelled other)
        {
            if (other is null)
            {
                return false;
            }

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
