using System;
using Game.Arena.Domain.Combat;
using Game.Arena.Domain.Concurrency;
using Game.Arena.Domain.Identity;

namespace Game.Arena.Domain.Events
{
    public readonly struct EnemyDefeated : IArenaDomainEvent, IEquatable<EnemyDefeated>
    {
        public EnemyDefeated(
            ArenaRunId arenaRunId,
            AggregateRevision aggregateRevision,
            EnemyId enemyId,
            EnemyKind enemyKind)
        {
            if (arenaRunId.IsNone)
            {
                throw new ArgumentException("ArenaRunId cannot be None.", nameof(arenaRunId));
            }

            if (enemyId.IsNone)
            {
                throw new ArgumentException("EnemyId cannot be None.", nameof(enemyId));
            }

            ArenaRunId = arenaRunId;
            AggregateRevision = aggregateRevision;
            EnemyId = enemyId;
            EnemyKind = enemyKind;
        }

        public ArenaRunId ArenaRunId { get; }

        public AggregateRevision AggregateRevision { get; }

        public EnemyId EnemyId { get; }

        public EnemyKind EnemyKind { get; }

        public bool Equals(EnemyDefeated other)
        {
            return ArenaRunId.Equals(other.ArenaRunId)
                && AggregateRevision.Equals(other.AggregateRevision)
                && EnemyId.Equals(other.EnemyId)
                && EnemyKind == other.EnemyKind;
        }

        public override bool Equals(object obj)
        {
            return obj is EnemyDefeated other && Equals(other);
        }

        public override int GetHashCode()
        {
            return HashCode.Combine(
                ArenaRunId,
                AggregateRevision,
                EnemyId,
                EnemyKind);
        }
    }
}
