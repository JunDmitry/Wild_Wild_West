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
        [SerializeField] private bool _loadArenaOnStart = true;

        private ArenaGameConfiguration _configuration;

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

        /// <summary>
        /// Gets or sets the configuration this scope registers. It is null in
        /// production, which builds <see cref="ArenaGameConfiguration.CreateDefault"/>;
        /// a rig may supply its own catalogue before the scope activates.
        /// </summary>
        public ArenaGameConfiguration Configuration
        {
            get => _configuration;
            set => _configuration = value;
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
                _configuration ?? ArenaGameConfiguration.CreateDefault());

            if (_loadArenaOnStart)
            {
                builder.RegisterEntryPoint<ArenaBootstrapEntryPoint>(Lifetime.Singleton);
            }
        }
    }
}
