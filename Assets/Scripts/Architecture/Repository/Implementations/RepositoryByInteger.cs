using Assets.Scripts.Architecture.Repository.Events;
using Assets.Scripts.Architecture.Repository.Interfaces;
using Assets.Scripts.Architecture.SignalBus.Interfaces;
using Assets.Scripts.Gameplay.PlayerFeature.Components;
using System;
using System.Collections.Generic;

namespace Assets.Scripts.Architecture.Repository.Implementations
{
    public class RepositoryByInteger<TInterface> : IDisposable, IRepository<int, TInterface>
    {
        private readonly Dictionary<int, TInterface> _itemById;
        private readonly ISignalBus<IEvent> _signalBus;

        public RepositoryByInteger(ISignalBus<IEvent> signalBus)
        {
            _itemById = new();
            _signalBus = signalBus;
        }

        public void Dispose()
        {
            foreach (TInterface item in _itemById.Values)
                _signalBus.TryPublish(new ItemRemovedFromRepositoryEvent<TInterface>(item));

            _itemById.Clear();
        }

        public void AddItem<TClass>(int id, TClass @class)
            where TClass : class, TInterface
        {
            ThrowIf.Invalid(HasItem<TClass>(id), $"RepositoryByInteger<{typeof(TInterface).Name}> already contains {typeof(TClass).Name} with id {id}");

            _itemById[id] = @class;
            _signalBus.TryPublish(new ItemAddedIntoRepositoryEvent<TInterface>(@class));
        }

        public bool RemoveItem<TClass>(int id)
            where TClass : class, TInterface
        {
            if (HasItem<TClass>(id) == false)
                return false;

            TInterface @class = _itemById[id];
            _signalBus.TryPublish(new ItemRemovedFromRepositoryEvent<TInterface>(@class));
            _itemById.Remove(id);

            return true;
        }

        public bool TryGetItem<TClass>(int id, out TClass concreteClass)
            where TClass : class, TInterface
        {
            concreteClass = GetItem<TClass>(id);

            return concreteClass != null;
        }

        public bool HasItem<TClass>(int id)
            where TClass : class, TInterface
        {
            return GetItem<TClass>(id) != null;
        }

        private TClass GetItem<TClass>(int id)
            where TClass : class, TInterface
        {
            if (_itemById.TryGetValue(id, out TInterface @class) == false)
                return null;

            return @class as TClass;
        }
    }
}