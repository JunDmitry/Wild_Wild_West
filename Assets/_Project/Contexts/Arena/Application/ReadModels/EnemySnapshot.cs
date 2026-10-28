using Game.Arena.Domain.Combat;
using Game.Arena.Domain.Geometry;
using Game.Arena.Domain.Identity;
using Game.Arena.Domain.Time;
using Game.Arena.Domain.Vitality;

namespace Game.Arena.Application.ReadModels
{
    public sealed class EnemySnapshot
    {
        internal EnemySnapshot()
        {
        }

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
            Fill(
                enemyId,
                kind,
                position,
                health,
                collisionRadius,
                attackReadyAt,
                hasPendingAttack,
                pendingAttack);
        }

        public EnemyId EnemyId { get; private set; }

        public EnemyKind Kind { get; private set; }

        public Position3D Position { get; private set; }

        public Health Health { get; private set; }

        public CollisionRadius CollisionRadius { get; private set; }

        public GameTimePoint AttackReadyAt { get; private set; }

        public bool HasPendingAttack { get; private set; }

        public PendingEnemyAttack PendingAttack { get; private set; }

        internal void Fill(
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
    }
}
