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
        }
    }
}
