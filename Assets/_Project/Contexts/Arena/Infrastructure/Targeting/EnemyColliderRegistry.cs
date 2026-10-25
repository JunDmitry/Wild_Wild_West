using System;
using System.Collections.Generic;
using Game.Arena.Domain.Identity;
using UnityEngine;

namespace Game.Arena.Infrastructure.Targeting
{
    public sealed class EnemyColliderRegistry : IEnemyColliderRegistry, IEnemyColliderRegistration
    {
        private readonly Dictionary<Collider, EnemyId> _enemyByCollider = new();
        private readonly Dictionary<EnemyId, Collider> _colliderByEnemy = new();

        public void Register(EnemyId enemyId, Collider collider)
        {
            if (collider == null)
            {
                throw new ArgumentNullException(nameof(collider));
            }

            if (_colliderByEnemy.TryGetValue(enemyId, out Collider previous))
            {
                _colliderByEnemy.Remove(enemyId);
                RemoveColliderEntry(previous);
            }

            if (_enemyByCollider.TryGetValue(collider, out EnemyId previousOwner))
            {
                _colliderByEnemy.Remove(previousOwner);
            }

            _colliderByEnemy[enemyId] = collider;
            _enemyByCollider[collider] = enemyId;
        }

        public void Unregister(EnemyId enemyId)
        {
            if (_colliderByEnemy.TryGetValue(enemyId, out Collider collider) == false)
            {
                return;
            }

            _colliderByEnemy.Remove(enemyId);
            RemoveColliderEntry(collider);
        }

        public bool TryGetEnemyId(Collider collider, out EnemyId enemyId)
        {
            if (collider != null && _enemyByCollider.TryGetValue(collider, out enemyId))
            {
                return true;
            }

            enemyId = EnemyId.None;
            return false;
        }

        private void RemoveColliderEntry(Collider collider)
        {
            // A destroyed Unity object is reference-non-null; remove its key too.
            if (ReferenceEquals(collider, null) == false)
            {
                _enemyByCollider.Remove(collider);
            }
        }
    }
}
