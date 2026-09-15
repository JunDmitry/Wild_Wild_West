using System;
using Game.Arena.Domain.Combat;
using Game.Arena.Domain.Configuration;
using Game.Arena.Domain.Geometry;
using Game.Arena.Domain.Identity;

namespace Game.Arena.Domain.Aggregates.ArenaRun
{
    internal sealed class Enemy
    {
        public Enemy(EnemyId id, EnemyDefinition definition, Position3D position)
        {
            if (id.IsNone)
            {
                throw new ArgumentException("EnemyId cannot be None.", nameof(id));
            }

            if (definition == null)
            {
                throw new ArgumentNullException(nameof(definition));
            }

            Id = id;
            Kind = definition.Kind;
            Position = position;
            Health = definition.InitialHealth;
            CollisionRadius = definition.CollisionRadius;
        }

        public EnemyId Id { get; }

        public EnemyKind Kind { get; }

        public Position3D Position { get; private set; }

        public Vitality.Health Health { get; private set; }

        public CollisionRadius CollisionRadius { get; }
    }
}
