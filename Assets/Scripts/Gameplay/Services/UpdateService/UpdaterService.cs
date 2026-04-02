using Assets.Scripts.Architecture.Repository.Events;
using Assets.Scripts.Architecture.SignalBus.Interfaces;
using Assets.Scripts.Gameplay.Common.Interfaces;
using Assets.Scripts.Gameplay.PlayerFeature.Components;
using System;
using System.Collections.Generic;

namespace Assets.Scripts.Gameplay.Services.UpdateService
{
    public class UpdaterService : IDisposable, IUpdaterService
    {
        private const int BufferSize = 64;

        private readonly HashSet<IUpdate> _updates;
        private readonly ISignalBus<IEvent> _signalBus;
        private readonly List<IDisposable> _disposables;

        private readonly List<IUpdate> _updatesForAdd;
        private readonly List<IUpdate> _updatesForRemove;

        public UpdaterService(ISignalBus<IEvent> signalBus)
        {
            _signalBus = signalBus;
            _disposables = new();

            _updates = new(BufferSize);
            _updatesForAdd = new();
            _updatesForRemove = new();

            Subscribe();
        }

        public void Dispose()
        {
            foreach (IDisposable disposable in _disposables)
            {
                disposable.Dispose();
            }

            _disposables.Clear();
        }

        public void Update(float deltaTime)
        {
            foreach (IUpdate updatable in _updates)
            {
                updatable.Update(deltaTime);
            }

            foreach (IUpdate updateForAdd in _updatesForAdd)
                _updates.Add(updateForAdd);

            foreach (IUpdate updateForRemove in _updatesForRemove)
                _updates.Remove(updateForRemove);

            if (_updatesForAdd.Count > 0)
                _updatesForAdd.Clear();

            if (_updatesForRemove.Count > 0)
                _updatesForRemove.Clear();
        }

        private void Subscribe()
        {
            _disposables.Add(_signalBus.Subscribe<ItemAddedIntoRepositoryEvent<IModel>>(e =>
            {
                if (e.AddedItem is IUpdate updatable)
                    _updatesForAdd.Add(updatable);
            }));
            _disposables.Add(_signalBus.Subscribe<ItemRemovedFromRepositoryEvent<IModel>>(e =>
            {
                if (e.RemovedItem is IUpdate updatable)
                    _updatesForRemove.Add(updatable);
            }));
        }
    }
}