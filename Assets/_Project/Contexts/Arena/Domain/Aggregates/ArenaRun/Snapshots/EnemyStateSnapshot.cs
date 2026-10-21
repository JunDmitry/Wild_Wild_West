using System;
using Game.Arena.Domain.Combat;
using Game.Arena.Domain.Geometry;
using Game.Arena.Domain.Identity;
using Game.Arena.Domain.Time;
using Game.Arena.Domain.Vitality;

namespace Game.Arena.Domain.Aggregates.ArenaRun
{
    public sealed class EnemyStateSnapshot
    {
        public EnemyStateSnapshot(
            EnemyId enemyId,
            EnemyKind kind,
            Position3D position,
            Health health,
            CollisionRadius collisionRadius,
            GameTimePoint attackReadyAt,
            PendingEnemyAttack pendingAttack)
        {
            if (enemyId.IsNone)
            {
                throw new ArgumentException("EnemyId cannot be None.", nameof(enemyId));
            }

            EnemyId = enemyId;
            Kind = kind;
            Position = position;
            Health = health;
            CollisionRadius = collisionRadius;
            AttackReadyAt = attackReadyAt;
            PendingAttack = pendingAttack;
            HasPendingAttack = pendingAttack.Id.IsNone == false;
        }

        public EnemyId EnemyId { get; }

        public EnemyKind Kind { get; }

        public Position3D Position { get; }

        public Health Health { get; }

        public CollisionRadius CollisionRadius { get; }

        public GameTimePoint AttackReadyAt { get; }

        public bool HasPendingAttack { get; }

        public PendingEnemyAttack PendingAttack { get; }
    }
}
