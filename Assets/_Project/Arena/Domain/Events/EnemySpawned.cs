using System;
using Game.Arena.Domain.Combat;
using Game.Arena.Domain.Concurrency;
using Game.Arena.Domain.Geometry;
using Game.Arena.Domain.Identity;

namespace Game.Arena.Domain.Events
{
    public readonly struct EnemySpawned : IArenaDomainEvent, IEquatable<EnemySpawned>
    {
        public EnemySpawned(
            ArenaRunId arenaRunId,
            AggregateRevision aggregateRevision,
            EnemyId enemyId,
            EnemyKind enemyKind,
            Position3D position)
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
            Position = position;
        }

        public ArenaRunId ArenaRunId { get; }

        public AggregateRevision AggregateRevision { get; }

        public EnemyId EnemyId { get; }

        public EnemyKind EnemyKind { get; }

        public Position3D Position { get; }

        public bool Equals(EnemySpawned other)
        {
            return ArenaRunId.Equals(other.ArenaRunId)
                && AggregateRevision.Equals(other.AggregateRevision)
                && EnemyId.Equals(other.EnemyId)
                && EnemyKind == other.EnemyKind
                && Position.Equals(other.Position);
        }

        public override bool Equals(object obj)
        {
            return obj is EnemySpawned other && Equals(other);
        }

        public override int GetHashCode()
        {
            return HashCode.Combine(ArenaRunId, AggregateRevision, EnemyId, EnemyKind, Position);
        }
    }
}
