using System.Collections.Generic;
using Game.Arena.Domain.Identity;
using Game.Arena.Domain.Interactions.Attack;

namespace Game.Arena.Application.Ports
{
    public interface IPlayerAttackTargetingResolver
    {
        IReadOnlyList<EnemyId> Resolve(PlayerAttackImpactRequest request);
    }
}
