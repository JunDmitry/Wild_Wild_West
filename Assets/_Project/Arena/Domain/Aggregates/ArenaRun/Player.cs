using Game.Arena.Domain.Combat;
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

        public void SwitchWeapon()
        {
            if (SelectedWeapon == WeaponKind.Ranged)
            {
                SelectedWeapon = WeaponKind.Melee;
                return;
            }

            SelectedWeapon = WeaponKind.Ranged;
        }

        public void TakeDamage(DamageAmount damage)
        {
            Health = Health.Reduce(damage);
        }
    }
}
