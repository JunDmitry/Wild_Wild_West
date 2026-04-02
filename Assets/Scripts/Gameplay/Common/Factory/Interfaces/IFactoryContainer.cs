using System;

namespace Assets.Scripts.Gameplay.PlayerFeature.Components
{
    public interface IFactoryContainer
    {
        TFactory GetFactory<TFactory>() where TFactory : class, IFactory;

        IFactory GetFactory(Type type);
    }
}