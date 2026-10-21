using Game.Arena.Domain.Geometry;
using Game.Arena.Domain.Interactions.Spawn;

namespace Game.Arena.Application.Ports
{
    public interface IEnemySpawnPlacementResolver
    {
        Position3D ResolveSpawnPosition(EnemySpawnRequest request);
    }
}
