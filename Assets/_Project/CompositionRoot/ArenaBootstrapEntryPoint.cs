using System;
using Game.Arena.Application.Ports;
using VContainer.Unity;

namespace Game.CompositionRoot
{
    /// <summary>
    /// Loads the arena scene once the persistent root scope exists (ADR-0032).
    /// The load goes through the Application port so the host owns the scene
    /// call (ADR-0029); the scene scope resolves the same port for the defeat
    /// reload. Registered by <see cref="BootstrapLifetimeScope"/> only when the
    /// running bootstrap scene asks for it.
    /// </summary>
    public sealed class ArenaBootstrapEntryPoint : IStartable
    {
        private readonly IArenaSceneReloader _sceneLoader;

        public ArenaBootstrapEntryPoint(IArenaSceneReloader sceneLoader)
        {
            _sceneLoader = sceneLoader
                ?? throw new ArgumentNullException(nameof(sceneLoader));
        }

        public void Start()
        {
            _sceneLoader.ReloadArenaScene();
        }
    }
}
