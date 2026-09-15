using System;
using Game.Arena.Domain.Geometry;
using Game.Arena.Domain.Interactions.Movement;
using Game.Arena.Domain.Vitality;

namespace Game.Arena.Domain.Configuration
{
    public sealed class PlayerDefinition
    {
        public PlayerDefinition(
            Position3D startPosition,
            Health initialHealth,
            MovementSpeed movementSpeed,
            CollisionRadius collisionRadius)
        {
            if (initialHealth.IsValid == false)
            {
                throw new ArgumentException("Health is invalid.", nameof(initialHealth));
            }

            if (initialHealth.IsDepleted)
            {
                throw new ArgumentException("Player cannot start defeated.", nameof(initialHealth));
            }

            if (movementSpeed.IsValid == false)
            {
                throw new ArgumentException("MovementSpeed is invalid.", nameof(movementSpeed));
            }

            if (collisionRadius.IsValid == false)
            {
                throw new ArgumentException("CollisionRadius is invalid.", nameof(collisionRadius));
            }

            StartPosition = startPosition;
            InitialHealth = initialHealth;
            MovementSpeed = movementSpeed;
            CollisionRadius = collisionRadius;
        }

        public Position3D StartPosition { get; }

        public Health InitialHealth { get; }

        public MovementSpeed MovementSpeed { get; }

        public CollisionRadius CollisionRadius { get; }
    }
}
