using UnityEngine;
using VContainer;
using VContainer.Unity;

namespace Game.CompositionRoot
{
    /// <summary>
    /// The persistent root scope (ADR-0014, ADR-0030). It survives Arena scene
    /// reloads and owns the process-lifetime graph.
    /// </summary>
    public sealed class BootstrapLifetimeScope : LifetimeScope
    {
        [SerializeField]
        private bool _loadArenaOnStart = true;

        /// <summary>
        /// Gets or sets whether building this scope loads the configured arena
        /// scene (ADR-0032). Production scenes leave it on; in-memory rigs turn
        /// it off so a test never performs a destructive scene load.
        /// </summary>
        public bool LoadArenaOnStart
        {
            get => _loadArenaOnStart;
            set => _loadArenaOnStart = value;
        }

        protected override void Awake()
        {
            DontDestroyOnLoad(gameObject);
            base.Awake();
        }

        protected override void Configure(IContainerBuilder builder)
        {
            ArenaRegistrations.RegisterProcessLifetime(
                builder,
                ArenaGameConfiguration.CreateDefault());

            if (_loadArenaOnStart)
            {
                builder.RegisterEntryPoint<ArenaBootstrapEntryPoint>(Lifetime.Singleton);
            }
        }
    }
}
