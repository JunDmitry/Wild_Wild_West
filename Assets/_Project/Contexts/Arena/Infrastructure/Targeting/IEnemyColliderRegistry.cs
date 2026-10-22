using Game.Arena.Domain.Identity;
using UnityEngine;

namespace Game.Arena.Infrastructure.Targeting
{
    public interface IEnemyColliderRegistry
    {
        // Resolves true only for colliders of live enemies; the scene registry
        // keeps the mapping current and removes colliders of defeated enemies.
        bool TryGetEnemyId(Collider collider, out EnemyId enemyId);
    }
}
