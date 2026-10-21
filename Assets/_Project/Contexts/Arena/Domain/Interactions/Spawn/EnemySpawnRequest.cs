using System;
using Game.Arena.Domain.Combat;
using Game.Arena.Domain.Geometry;
using Game.Arena.Domain.Interactions.Contracts;

namespace Game.Arena.Domain.Interactions.Spawn
{
    public readonly struct EnemySpawnRequest : IInteractionRequest, IEquatable<EnemySpawnRequest>
    {
        public EnemySpawnRequest(
            InteractionCorrelation correlation,
            EnemyKind enemyKind,
            CollisionRadius collisionRadius)
        {
            if (correlation.ArenaRunId.IsNone)
            {
                throw new ArgumentException("ArenaRunId cannot be None.", nameof(correlation));
            }

            if (correlation.InteractionId.IsNone)
            {
                throw new ArgumentException("InteractionId cannot be None.", nameof(correlation));
            }

            if (collisionRadius.IsValid == false)
            {
                throw new ArgumentException("CollisionRadius is invalid.", nameof(collisionRadius));
            }

            Correlation = correlation;
            EnemyKind = enemyKind;
            CollisionRadius = collisionRadius;
        }

        public InteractionCorrelation Correlation { get; }

        public InteractionKind Kind => InteractionKind.EnemySpawn;

        public EnemyKind EnemyKind { get; }

        public CollisionRadius CollisionRadius { get; }

        public bool Equals(EnemySpawnRequest other)
        {
            return Correlation.Equals(other.Correlation)
                && EnemyKind == other.EnemyKind
                && CollisionRadius.Equals(other.CollisionRadius);
        }

        public override bool Equals(object obj)
        {
            return obj is EnemySpawnRequest other && Equals(other);
        }

        public override int GetHashCode()
        {
            return HashCode.Combine(Correlation, EnemyKind, CollisionRadius);
        }

        public static bool operator ==(EnemySpawnRequest left, EnemySpawnRequest right)
        {
            return left.Equals(right);
        }

        public static bool operator !=(EnemySpawnRequest left, EnemySpawnRequest right)
        {
            return !left.Equals(right);
        }
    }
}
