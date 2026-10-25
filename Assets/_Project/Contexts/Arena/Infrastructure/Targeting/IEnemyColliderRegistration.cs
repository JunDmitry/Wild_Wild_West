using Game.Arena.Domain.Identity;
using UnityEngine;

namespace Game.Arena.Infrastructure.Targeting
{
    /// <summary>
    /// Registration surface of the live-enemy collider map. Populated from the
    /// presentation layer's view lifetime through callbacks subscribed at
    /// composition; consumed for targeting through <see cref="IEnemyColliderRegistry"/>.
    /// </summary>
    public interface IEnemyColliderRegistration
    {
        void Register(EnemyId enemyId, Collider collider);

        void Unregister(EnemyId enemyId);
    }
}
