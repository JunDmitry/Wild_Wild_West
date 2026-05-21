using Assets.Scripts.Architecture.Presentation.Factory;
using Assets.Scripts.Architecture.Repository.Implementations;
using Assets.Scripts.Architecture.SignalBus;
using Assets.Scripts.Architecture.UI.Factory;
using Assets.Scripts.Gameplay.Common.Interfaces;
using Assets.Scripts.Gameplay.PlayerFeature.Components;
using Assets.Scripts.Gameplay.Services.UpdateService;
using Zenject;

public class ArenaSceneInstaller : MonoInstaller
{
    public override void InstallBindings()
    {
        BindLifecycleSystems();
        BindTimeServices();
        BindRepositories();
        BindEventSystems();
        BindFactories();
        BindUiServices();
    }

    private void BindLifecycleSystems()
    {
        Container.BindInterfacesAndSelfTo<UpdaterService>().AsSingle();
        Container.BindInterfacesAndSelfTo<LifetimeService>().AsSingle();
    }

    private void BindTimeServices()
    {
        Container.BindInterfacesAndSelfTo<TimeService>().AsSingle();
    }

    private void BindRepositories()
    {
        Container.BindInterfacesAndSelfTo<RepositoryByInteger<IModel>>().AsSingle();
        Container.BindInterfacesAndSelfTo<RepositoryByInteger<IComponentTagsContainer>>().AsSingle();
    }

    private void BindEventSystems()
    {
        Container.Bind(typeof(ISignalBus<>)).To(typeof(SignalBus<>)).AsSingle();
    }

    private void BindFactories()
    {
        Container.Bind<IPresenterFactory>().To<PresenterFactory>().AsSingle();
        Container.Bind<IViewFactory>().To<ViewFactory>().AsSingle();
    }

    private void BindUiServices()
    {
        Container.BindInterfacesAndSelfTo<ContextConverterService>().AsSingle();
    }
}