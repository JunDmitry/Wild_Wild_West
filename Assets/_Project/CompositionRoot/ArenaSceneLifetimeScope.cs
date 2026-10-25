using System;
using VContainer;
using VContainer.Unity;

namespace Game.CompositionRoot
{
    /// <summary>
    /// The per-scene child scope (ADR-0014, ADR-0030). It does not build itself:
    /// the scene binder calls <see cref="Bind"/> and then <see cref="LifetimeScope.Build"/>
    /// so a missing scene seam fails loudly instead of resolving half a graph.
    /// </summary>
    public sealed class ArenaSceneLifetimeScope : LifetimeScope
    {
        private ArenaSceneObjects _sceneObjects;

        protected override void Awake()
        {
            // Run-time assignment beats the serialized default, so the scope
            // never builds itself before the binder supplies its scene seam.
            autoRun = false;
            base.Awake();
        }

        public void Bind(ArenaSceneObjects sceneObjects)
        {
            if (_sceneObjects != null)
            {
                throw new InvalidOperationException(
                    "The Arena scene scope is already bound.");
            }

            _sceneObjects = sceneObjects
                ?? throw new ArgumentNullException(nameof(sceneObjects));
        }

        protected override void Configure(IContainerBuilder builder)
        {
            if (_sceneObjects == null)
            {
                throw new InvalidOperationException(
                    "Call Bind with ArenaSceneObjects before building the Arena scene scope.");
            }

            ArenaRegistrations.RegisterSceneLifetime(builder, _sceneObjects);
        }
    }
}
