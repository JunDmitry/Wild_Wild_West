using System;
using Game.Arena.Domain.Combat;
using Game.Arena.Domain.Concurrency;
using Game.Arena.Domain.Geometry;
using Game.Arena.Domain.Identity;

namespace Game.Arena.Domain.Events
{
    public sealed class EnemySpawned : IPooledArenaDomainEvent, IEquatable<EnemySpawned>
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

        internal EnemySpawned()
        {
        }

        public ArenaRunId ArenaRunId { get; private set; }

        public AggregateRevision AggregateRevision { get; private set; }

        public EnemyId EnemyId { get; private set; }

        public EnemyKind EnemyKind { get; private set; }

        public Position3D Position { get; private set; }

        internal void Fill(
            ArenaRunId arenaRunId,
            AggregateRevision aggregateRevision,
            EnemyId enemyId,
            EnemyKind enemyKind,
            Position3D position)
        {
            ArenaRunId = arenaRunId;
            AggregateRevision = aggregateRevision;
            EnemyId = enemyId;
            EnemyKind = enemyKind;
            Position = position;
        }

        void IPooledArenaDomainEvent.Reset()
        {
            ArenaRunId = default;
            AggregateRevision = default;
            EnemyId = default;
            EnemyKind = default;
            Position = default;
        }

        public bool Equals(EnemySpawned other)
        {
            if (other is null)
            {
                return false;
            }

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
