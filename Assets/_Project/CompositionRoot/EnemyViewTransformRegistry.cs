using System;
using Game.Arena.Domain.Identity;
using Game.Arena.Infrastructure.Movement;
using Game.Arena.Presentation.Enemies;
using UnityEngine;

namespace Game.CompositionRoot
{
    /// <summary>
    /// Bridges the presentation view registry to the Infrastructure movement
    /// port. Composition is the only layer allowed to see both sides (ADR-0007,
    /// ADR-0031): Infrastructure must not reference Presentation.
    /// </summary>
    public sealed class EnemyViewTransformRegistry : IEnemyTransformRegistry
    {
        private readonly EnemyViewRegistry _views;

        public EnemyViewTransformRegistry(EnemyViewRegistry views)
        {
            _views = views != null
                ? views
                : throw new ArgumentNullException(nameof(views));
        }

        public bool TryGetTransform(EnemyId enemyId, out Transform transform)
        {
            if (_views.TryGetView(enemyId, out EnemyView view) && view != null)
            {
                transform = view.Body;

                return transform != null;
            }

            transform = null;

            return false;
        }
    }
}
