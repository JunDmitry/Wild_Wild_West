using System;
using Game.Arena.Domain.Combat;
using Game.Arena.Domain.Geometry;
using Game.Arena.Domain.Interactions.Movement;
using Game.Arena.Domain.Time;
using Game.Arena.Domain.Vitality;

namespace Game.Arena.Domain.Configuration
{
    public sealed class EnemyDefinition
    {
        public EnemyDefinition(
            EnemyKind kind,
            Health initialHealth,
            MovementSpeed movementSpeed,
            CollisionRadius collisionRadius,
            DamageAmount attackDamage,
            Distance attackRange,
            GameDuration attackCooldown,
            GameDuration attackWindupDuration)
        {
            if (initialHealth.IsValid == false)
            {
                throw new ArgumentException("Health is invalid.", nameof(initialHealth));
            }

            if (initialHealth.IsDepleted)
            {
                throw new ArgumentException("Enemy cannot start defeated.", nameof(initialHealth));
            }

            if (movementSpeed.IsValid == false)
            {
                throw new ArgumentException("MovementSpeed is invalid.", nameof(movementSpeed));
            }

            if (collisionRadius.IsValid == false)
            {
                throw new ArgumentException("CollisionRadius is invalid.", nameof(collisionRadius));
            }

            if (attackDamage.Points <= 0)
            {
                throw new ArgumentException("DamageAmount is invalid.", nameof(attackDamage));
            }

            if (attackRange.IsZero)
            {
                throw new ArgumentOutOfRangeException(nameof(attackRange));
            }

            if (attackCooldown.Seconds <= 0d)
            {
                throw new ArgumentOutOfRangeException(nameof(attackCooldown));
            }

            Kind = kind;
            InitialHealth = initialHealth;
            MovementSpeed = movementSpeed;
            CollisionRadius = collisionRadius;
            AttackDamage = attackDamage;
            AttackRange = attackRange;
            AttackCooldown = attackCooldown;
            AttackWindupDuration = attackWindupDuration;
        }

        public EnemyKind Kind { get; }
        public Health InitialHealth { get; }
        public MovementSpeed MovementSpeed { get; }
        public CollisionRadius CollisionRadius { get; }
        public DamageAmount AttackDamage { get; }
        public Distance AttackRange { get; }
        public GameDuration AttackCooldown { get; }
        public GameDuration AttackWindupDuration { get; }
    }
}
