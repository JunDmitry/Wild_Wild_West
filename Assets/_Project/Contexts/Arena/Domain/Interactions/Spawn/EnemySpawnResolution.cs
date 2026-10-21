using System;
using Game.Arena.Domain.Geometry;
using Game.Arena.Domain.Identity;
using Game.Arena.Domain.Interactions.Contracts;

namespace Game.Arena.Domain.Interactions.Spawn
{
    public readonly struct EnemySpawnResolution : IInteractionResolution, IEquatable<EnemySpawnResolution>
    {
        public EnemySpawnResolution(
            InteractionCorrelation correlation,
            EnemyId enemyId,
            Position3D spawnPosition)
        {
            if (correlation.ArenaRunId.IsNone)
            {
                throw new ArgumentException("ArenaRunId cannot be None.", nameof(correlation));
            }

            if (correlation.InteractionId.IsNone)
            {
                throw new ArgumentException("InteractionId cannot be None.", nameof(correlation));
            }

            Correlation = correlation;
            EnemyId = enemyId;
            SpawnPosition = spawnPosition;
        }

        public InteractionCorrelation Correlation { get; }

        public InteractionKind Kind => InteractionKind.EnemySpawn;

        public EnemyId EnemyId { get; }

        public Position3D SpawnPosition { get; }

        public bool Equals(EnemySpawnResolution other)
        {
            return Correlation.Equals(other.Correlation)
                && EnemyId.Equals(other.EnemyId)
                && SpawnPosition.Equals(other.SpawnPosition);
        }

        public override bool Equals(object obj)
        {
            return obj is EnemySpawnResolution other && Equals(other);
        }

        public override int GetHashCode()
        {
            return HashCode.Combine(Correlation, EnemyId, SpawnPosition);
        }

        public static bool operator ==(EnemySpawnResolution left, EnemySpawnResolution right)
        {
            return left.Equals(right);
        }

        public static bool operator !=(EnemySpawnResolution left, EnemySpawnResolution right)
        {
            return !left.Equals(right);
        }
    }
}
