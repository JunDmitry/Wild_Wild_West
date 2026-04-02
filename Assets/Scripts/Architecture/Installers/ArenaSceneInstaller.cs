using Assets.Scripts.Architecture.Presentation.Factory;
using Assets.Scripts.Architecture.Repository.Implementations;
using Assets.Scripts.Architecture.SignalBus;
using Assets.Scripts.Architecture.UI.Factory;
using Assets.Scripts.Gameplay.Common.Interfaces;
using Assets.Scripts.Gameplay.PlayerFeature.Components;
using Zenject;

public class ArenaSceneInstaller : MonoInstaller
{
    public override void InstallBindings()
    {
        Container.BindInterfacesAndSelfTo<RepositoryByInteger<IModel>>().AsSingle();
        Container.Bind(typeof(ISignalBus<>)).To(typeof(SignalBus<>)).AsSingle();

        BindFactories();
    }

    private void BindFactories()
    {
        Container.Bind<IPresenterFactory>().To<PresenterFactory>().AsSingle();
        Container.Bind<IViewFactory>().To<ViewFactory>().AsSingle();
    }
}