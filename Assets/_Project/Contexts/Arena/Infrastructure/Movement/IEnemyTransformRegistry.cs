using Game.Arena.Domain.Identity;
using UnityEngine;

namespace Game.Arena.Infrastructure.Movement
{
    public interface IEnemyTransformRegistry
    {
        // Resolves true only for transforms of live enemies; the scene
        // implementation (delivery plan T-6.8) keeps the mapping current and
        // removes the transform of a defeated enemy.
        bool TryGetTransform(EnemyId enemyId, out Transform transform);

        // Resolves the Enemy view's target collider, the same collider the
        // scene registry maps back to the Enemy. It is the movement cast origin
        // (ADR-0039).
        bool TryGetCollider(EnemyId enemyId, out Collider collider);
    }
}
