using Game.Arena.Domain.Combat;
using Game.Arena.Domain.Configuration;
using Game.Arena.Domain.Geometry;
using Game.Arena.Domain.Identity;
using Game.Arena.Domain.Interactions.Movement;
using Game.Arena.Domain.Time;
using Game.Arena.Domain.Vitality;

namespace Game.Arena.Domain.Aggregates.ArenaRun
{
    internal sealed class Player
    {
        private GameTimePoint _rangedReadyAt;
        private GameTimePoint _meleeReadyAt;
        private PendingPlayerAttack _pendingAttack;

        public Player(
            PlayerId id,
            Position3D position,
            Health health,
            WeaponKind selectedWeapon,
            MovementSpeed movementSpeed,
            CollisionRadius collisionRadius)
        {
            if (id.IsNone)
            {
                throw new System.ArgumentException("PlayerId cannot be None.", nameof(id));
            }

            if (health.IsValid == false)
            {
                throw new System.ArgumentException("Health is invalid.", nameof(health));
            }

            if (health.IsDepleted)
            {
                throw new System.ArgumentException("Player cannot start defeated.", nameof(health));
            }

            if (movementSpeed.IsValid == false)
            {
                throw new System.ArgumentException("MovementSpeed is invalid.", nameof(movementSpeed));
            }

            if (collisionRadius.IsValid == false)
            {
                throw new System.ArgumentException("CollisionRadius is invalid.", nameof(collisionRadius));
            }

            Id = id;
            Position = position;
            Health = health;
            SelectedWeapon = selectedWeapon;
            MovementSpeed = movementSpeed;
            CollisionRadius = collisionRadius;
            _rangedReadyAt = new GameTimePoint(0f);
            _meleeReadyAt = new GameTimePoint(0f);
        }

        public PlayerId Id { get; }
        public Position3D Position { get; private set; }
        public Health Health { get; private set; }
        public WeaponKind SelectedWeapon { get; private set; }
        public MovementSpeed MovementSpeed { get; }
        public CollisionRadius CollisionRadius { get; }
        public bool HasPendingAttack => _pendingAttack.Id.IsNone == false;
        public PendingPlayerAttack PendingAttack => _pendingAttack;

        public GameTimePoint ReadyAt(WeaponKind kind)
        {
            if (kind == WeaponKind.Ranged)
            {
                return _rangedReadyAt;
            }

            return _meleeReadyAt;
        }

        public bool IsWeaponReady(WeaponKind kind, GameTimePoint now)
        {
            return now >= ReadyAt(kind);
        }

        public void MarkWeaponUsed(WeaponKind kind, GameTimePoint now, GameDuration cooldown)
        {
            GameTimePoint readyAt = now + cooldown;

            if (kind == WeaponKind.Ranged)
            {
                _rangedReadyAt = readyAt;
                return;
            }

            _meleeReadyAt = readyAt;
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

        public bool CanStartAttack(GameTimePoint now)
        {
            if (HasPendingAttack)
            {
                return false;
            }

            return IsWeaponReady(SelectedWeapon, now);
        }

        public PendingPlayerAttack StartAttack(AttackId attackId, WeaponDefinition weapon, GameTimePoint now)
        {
            if (weapon == null)
            {
                throw new System.ArgumentNullException(nameof(weapon));
            }

            if (weapon.Kind != SelectedWeapon)
            {
                throw new System.ArgumentException("Weapon does not match selected weapon.", nameof(weapon));
            }

            if (attackId.IsNone)
            {
                throw new System.ArgumentException("AttackId cannot be None.", nameof(attackId));
            }

            if (CanStartAttack(now) == false)
            {
                throw new System.InvalidOperationException("Player cannot start attack.");
            }

            PendingPlayerAttack attack = new(
                attackId,
                Id,
                SelectedWeapon,
                now,
                now + weapon.WindupDuration);

            _pendingAttack = attack;
            MarkWeaponUsed(SelectedWeapon, now, weapon.Cooldown);

            return attack;
        }

        public void CompleteAttack()
        {
            if (HasPendingAttack == false)
            {
                throw new System.InvalidOperationException("Player has no pending attack.");
            }

            _pendingAttack = default;
        }

        public void CancelAttack()
        {
            if (HasPendingAttack == false)
            {
                throw new System.InvalidOperationException("Player has no pending attack.");
            }

            _pendingAttack = default;
        }

        public void SwitchWeapon()
        {
            if (SelectedWeapon == WeaponKind.Ranged)
            {
                SelectedWeapon = WeaponKind.Melee;
                return;
            }

            SelectedWeapon = WeaponKind.Ranged;
        }

        public DamageApplication TakeDamage(DamageAmount damage)
        {
            DamageApplication application = Health.ApplyDamage(damage);
            Health = application.RemainingHealth;

            return application;
        }
    }
}
