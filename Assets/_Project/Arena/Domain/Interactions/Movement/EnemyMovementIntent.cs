using System;
using Game.Arena.Domain.Geometry;
using Game.Arena.Domain.Identity;
using Game.Arena.Domain.Movement;

namespace Game.Arena.Domain.Interactions.Movement
{
    public readonly struct EnemyMovementIntent : IEquatable<EnemyMovementIntent>
    {
        public EnemyMovementIntent(
            EnemyId enemyId,
            PlanarMovementIntent intent,
            CollisionRadius collisionRadius)
        {
            if (enemyId.IsNone)
            {
                throw new ArgumentException("EnemyId cannot be None.", nameof(enemyId));
            }

            if (intent.IsValid == false)
            {
                throw new ArgumentException("Movement intent is invalid.", nameof(intent));
            }

            if (collisionRadius.IsValid == false)
            {
                throw new ArgumentException("CollisionRadius is invalid.", nameof(collisionRadius));
            }

            EnemyId = enemyId;
            Intent = intent;
            CollisionRadius = collisionRadius;
        }

        public EnemyId EnemyId { get; }

        public PlanarMovementIntent Intent { get; }

        public CollisionRadius CollisionRadius { get; }

        public bool Equals(EnemyMovementIntent other)
        {
            return EnemyId.Equals(other.EnemyId)
                && Intent.Equals(other.Intent)
                && CollisionRadius.Equals(other.CollisionRadius);
        }

        public override bool Equals(object obj)
        {
            return obj is EnemyMovementIntent other && Equals(other);
        }

        public override int GetHashCode()
        {
            return HashCode.Combine(EnemyId, Intent, CollisionRadius);
        }

        public static bool operator ==(EnemyMovementIntent left, EnemyMovementIntent right)
        {
            return left.Equals(right);
        }

        public static bool operator !=(EnemyMovementIntent left, EnemyMovementIntent right)
        {
            return !left.Equals(right);
        }
    }
}
