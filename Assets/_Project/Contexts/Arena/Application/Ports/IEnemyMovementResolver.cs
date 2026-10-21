using System.Collections.Generic;
using Game.Arena.Domain.Interactions.Movement;

namespace Game.Arena.Application.Ports
{
    public interface IEnemyMovementResolver
    {
        IReadOnlyList<EnemyMovementBatchResolutionEntry> Resolve(EnemyMovementBatchRequest request);
    }
}
