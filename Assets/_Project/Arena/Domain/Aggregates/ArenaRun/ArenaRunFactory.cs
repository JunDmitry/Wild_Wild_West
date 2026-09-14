using Game.Arena.Domain.Geometry;
using Game.Arena.Domain.Identity;
using Game.Arena.Domain.Interactions.Movement;

namespace Game.Arena.Domain.Aggregates.ArenaRun
{
    public sealed class ArenaRunFactory
    {
        public ArenaRun Start(
            ArenaRunId arenaRunId,
            PlayerId playerId,
            Position3D playerStartPosition,
            MovementSpeed playerMovementSpeed,
            CollisionRadius playerCollisionRadius,
            ArenaBounds arenaBounds)
        {
            Player player = new(playerId, playerStartPosition, playerMovementSpeed, playerCollisionRadius);
            Wave wave = new(WaveNumber.First);

            return new(arenaRunId, player, wave, arenaBounds);
        }
    }
}
