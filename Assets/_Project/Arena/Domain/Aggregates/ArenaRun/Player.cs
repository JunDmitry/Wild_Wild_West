using Game.Arena.Domain.Geometry;
using Game.Arena.Domain.Identity;
using Game.Arena.Domain.Interactions.Movement;

namespace Game.Arena.Domain.Aggregates.ArenaRun
{
    internal sealed class Player
    {
        public Player(
            PlayerId id,
            Position3D position,
            MovementSpeed movementSpeed,
            CollisionRadius collisionRadius)
        {
            if (id.IsNone)
            {
                throw new System.ArgumentException("PlayerId cannot be None.", nameof(id));
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
            MovementSpeed = movementSpeed;
            CollisionRadius = collisionRadius;
        }

        public PlayerId Id { get; }
        public Position3D Position { get; private set; }
        public MovementSpeed MovementSpeed { get; }
        public CollisionRadius CollisionRadius { get; }

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
