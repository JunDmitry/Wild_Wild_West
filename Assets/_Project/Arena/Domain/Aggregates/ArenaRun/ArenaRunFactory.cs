using Game.Arena.Domain.Geometry;
using Game.Arena.Domain.Identity;
using Game.Arena.Domain.Interactions.Movement;
using Game.Arena.Domain.Vitality;

namespace Game.Arena.Domain.Aggregates.ArenaRun
{
    public sealed class ArenaRunFactory
    {
        public ArenaRun Start(
            ArenaRunId arenaRunId,
            PlayerId playerId,
            Position3D playerStartPosition,
            Health playerHealth,
            MovementSpeed playerMovementSpeed,
            CollisionRadius playerCollisionRadius,
            ArenaBounds arenaBounds)
        {
            Player player = new(
                playerId,
                playerStartPosition,
                playerHealth,
                Combat.WeaponKind.Ranged,
                playerMovementSpeed,
                playerCollisionRadius);

            Wave wave = new(WaveNumber.First);

            return new(
                arenaRunId,
                player,
                wave,
                arenaBounds);
        }
    }
}
