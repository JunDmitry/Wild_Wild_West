using System;
using System.Collections.Generic;
using Zenject;

namespace Assets.Scripts.Gameplay.PlayerFeature.Components
{
    public class FactoryContainer : IFactoryContainer
    {
        private static readonly Type s_factoryInterface = typeof(IFactory);

        private readonly Dictionary<Type, IFactory> _container;
        private readonly DiContainer _diContainer;

        public FactoryContainer(DiContainer diContainer)
        {
            _diContainer = diContainer;
            _container = new();
        }

        public TFactory GetFactory<TFactory>()
            where TFactory : class, IFactory
        {
            Type type = typeof(TFactory);

            return (TFactory)GetFactory(type);
        }

        public IFactory GetFactory(Type type)
        {
            ThrowIf.Invalid(s_factoryInterface.IsAssignableFrom(type) == false, $"Factory type should implement '{nameof(IFactory)}' interface");

            if (_container.TryGetValue(type, out IFactory factory))
            {
                factory = (IFactory)_diContainer.Instantiate(type);
                _container[type] = factory;
            }

            return factory;
        }
    }
}