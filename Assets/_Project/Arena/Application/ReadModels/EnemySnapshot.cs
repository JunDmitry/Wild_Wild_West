using Game.Arena.Domain.Combat;
using Game.Arena.Domain.Geometry;
using Game.Arena.Domain.Identity;
using Game.Arena.Domain.Time;
using Game.Arena.Domain.Vitality;

namespace Game.Arena.Application.ReadModels
{
    public sealed class EnemySnapshot
    {
        public EnemySnapshot(
            EnemyId enemyId,
            EnemyKind kind,
            Position3D position,
            Health health,
            CollisionRadius collisionRadius,
            GameTimePoint attackReadyAt,
            bool hasPendingAttack,
            PendingEnemyAttack pendingAttack)
        {
            EnemyId = enemyId;
            Kind = kind;
            Position = position;
            Health = health;
            CollisionRadius = collisionRadius;
            AttackReadyAt = attackReadyAt;
            HasPendingAttack = hasPendingAttack;
            PendingAttack = pendingAttack;
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
