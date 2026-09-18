using System;
using Game.Arena.Domain.Combat;
using Game.Arena.Domain.Configuration;
using Game.Arena.Domain.Geometry;
using Game.Arena.Domain.Identity;
using Game.Arena.Domain.Time;
using Game.Arena.Domain.Vitality;

namespace Game.Arena.Domain.Aggregates.ArenaRun
{
    internal sealed class Enemy
    {
        private readonly EnemyDefinition _definition;

        private PendingEnemyAttack _pendingAttack;
        private GameTimePoint _attackReadyAt;

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
            _definition = definition;
            _attackReadyAt = new GameTimePoint(0d);
        }

        public EnemyId Id { get; }

        public EnemyKind Kind { get; }

        public Position3D Position { get; private set; }

        public Vitality.Health Health { get; private set; }

        public CollisionRadius CollisionRadius { get; }
        public EnemyDefinition Definition => _definition;
        public GameTimePoint AttackReadyAt => _attackReadyAt;
        public bool HasPendingAttack => _pendingAttack.Id.IsNone == false;
        public PendingEnemyAttack PendingAttack => _pendingAttack;
        public bool IsDefeated => Health.IsDepleted;

        public void TakeDamage(DamageAmount damage)
        {
            Health = Health.Reduce(damage);
        }

        public bool IsAttackReady(GameTimePoint now)
        {
            return now >= _attackReadyAt;
        }

        public bool CanStartAttack(GameTimePoint now)
        {
            if (IsDefeated)
            {
                return false;
            }

            if (HasPendingAttack)
            {
                return false;
            }

            return IsAttackReady(now);
        }

        public PendingEnemyAttack StartAttack(AttackId attackId, GameTimePoint now)
        {
            if (attackId.IsNone)
            {
                throw new ArgumentException("AttackId cannot be None.", nameof(attackId));
            }

            if (CanStartAttack(now) == false)
            {
                throw new InvalidOperationException("Enemy cannot start attack.");
            }

            PendingEnemyAttack attack = new(
                attackId,
                Id,
                now,
                now + _definition.AttackWindupDuration);

            _pendingAttack = attack;
            _attackReadyAt = now + _definition.AttackCooldown;

            return attack;
        }

        public void CompleteAttack()
        {
            if (HasPendingAttack == false)
            {
                throw new InvalidOperationException("Enemy has no pending attack.");
            }

            _pendingAttack = default;
        }

        public bool MoveTo(Position3D position)
        {
            if (Position == position)
            {
                return false;
            }

            Position = position;

            return true;
        }
    }
}
