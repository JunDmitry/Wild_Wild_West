using System;
using Zenject;

namespace Assets.Scripts.Gameplay.PlayerFeature.Components
{
    public class Resolver : IResolver
    {
        private readonly DiContainer _container;

        public Resolver(DiContainer container)
        {
            _container = container;
        }

        public T Resolve<T>()
        {
            return _container.Resolve<T>();
        }

        public object Resolve(Type type)
        {
            return _container.Resolve(type);
        }

        public T Instantiate<T>()
        {
            return _container.Instantiate<T>();
        }

        public T Instantiate<T>(params object[] extraArgs)
        {
            return _container.Instantiate<T>(extraArgs);
        }

        public object Instantiate(Type type)
        {
            return _container.Instantiate(type);
        }

        public object Instantiate(Type type, params object[] extraArgs)
        {
            return _container.Instantiate(type, extraArgs);
        }
    }
}