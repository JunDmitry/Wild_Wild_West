using System;
using Game.Arena.Domain.Combat;
using Game.Arena.Domain.Concurrency;
using Game.Arena.Domain.Identity;
using Game.Arena.Domain.Time;

namespace Game.Arena.Domain.Events
{
    public sealed class PlayerAttackCompleted : IPooledArenaDomainEvent, IEquatable<PlayerAttackCompleted>
    {
        internal PlayerAttackCompleted()
        {
        }

        public PlayerAttackCompleted(
            ArenaRunId arenaRunId,
            AggregateRevision aggregateRevision,
            PlayerId playerId,
            AttackId attackId,
            WeaponKind weaponKind,
            AttackOutcome outcome,
            GameTimePoint completedAt)
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
            WeaponKind = weaponKind;
            Outcome = outcome;
            CompletedAt = completedAt;
        }

        public ArenaRunId ArenaRunId { get; private set; }

        public AggregateRevision AggregateRevision { get; private set; }

        public PlayerId PlayerId { get; private set; }

        public AttackId AttackId { get; private set; }

        public WeaponKind WeaponKind { get; private set; }

        public AttackOutcome Outcome { get; private set; }

        public GameTimePoint CompletedAt { get; private set; }

        internal void Fill(
            ArenaRunId arenaRunId,
            AggregateRevision aggregateRevision,
            PlayerId playerId,
            AttackId attackId,
            WeaponKind weaponKind,
            AttackOutcome outcome,
            GameTimePoint completedAt)
        {
            ArenaRunId = arenaRunId;
            AggregateRevision = aggregateRevision;
            PlayerId = playerId;
            AttackId = attackId;
            WeaponKind = weaponKind;
            Outcome = outcome;
            CompletedAt = completedAt;
        }

        void IPooledArenaDomainEvent.Reset()
        {
            ArenaRunId = default;
            AggregateRevision = default;
            PlayerId = default;
            AttackId = default;
            WeaponKind = default;
            Outcome = default;
            CompletedAt = default;
        }

        public bool Equals(PlayerAttackCompleted other)
        {
            if (other is null)
            {
                return false;
            }

            return ArenaRunId.Equals(other.ArenaRunId)
                && AggregateRevision.Equals(other.AggregateRevision)
                && PlayerId.Equals(other.PlayerId)
                && AttackId.Equals(other.AttackId)
                && WeaponKind == other.WeaponKind
                && Outcome == other.Outcome
                && CompletedAt.Equals(other.CompletedAt);
        }

        public override bool Equals(object obj)
        {
            return obj is PlayerAttackCompleted other && Equals(other);
        }

        public override int GetHashCode()
        {
            return HashCode.Combine(
                ArenaRunId,
                AggregateRevision,
                PlayerId,
                AttackId,
                WeaponKind,
                Outcome,
                CompletedAt);
        }
    }
}
