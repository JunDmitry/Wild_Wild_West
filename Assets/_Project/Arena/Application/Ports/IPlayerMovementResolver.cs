using Game.Arena.Domain.Geometry;
using Game.Arena.Domain.Interactions.Movement;

namespace Game.Arena.Application.Ports
{
    public interface IPlayerMovementResolver
    {
        Position3D Resolve(PlayerMovementRequest request);
    }
}
