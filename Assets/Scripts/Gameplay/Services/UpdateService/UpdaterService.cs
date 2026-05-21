using Assets.Scripts.Architecture.Repository.Events;
using Assets.Scripts.Architecture.SignalBus.Interfaces;
using Assets.Scripts.Gameplay.Common.Interfaces;
using Assets.Scripts.Gameplay.PlayerFeature.Components;
using System;

namespace Assets.Scripts.Gameplay.Services.UpdateService
{
    public class UpdaterService : IDisposable, IUpdaterService, ITickable, IFixedTickable, ILateTickable
    {
        private readonly ISignalBus<IEvent> _signalBus;

        public UpdaterService(ISignalBus<IEvent> signalBus)
        {
            _signalBus = signalBus;

            Subscribe();
        }

        public event Action<float> Ticked;
        public event Action<float> FixedTicked;
        public event Action<float> LateTicked;

        public void Dispose()
        {
            Ticked = null;
            FixedTicked = null;
            LateTicked = null;
        }

        public void Update(float deltaTime)
        {
            Ticked?.Invoke(deltaTime);
        }

        public void FixedUpdate(float fixedDeltaTime)
        {
            FixedTicked?.Invoke(fixedDeltaTime);
        }

        public void LateUpdate(float deltaTime)
        {
            LateTicked?.Invoke(deltaTime);
        }

        private void Subscribe()
        {
            _signalBus.Subscribe<ItemAddedIntoRepositoryEvent<IModel>>(e =>
            {
                if (e.AddedItem is IUpdate updatable)
                    Ticked += updatable.Update;
            });
            _signalBus.Subscribe<ItemRemovedFromRepositoryEvent<IModel>>(e =>
            {
                if (e.RemovedItem is IUpdate updatable)
                    Ticked -= updatable.Update;
            });
        }
    }

    public interface ITickable
    {
        event Action<float> Ticked;
    }

    public interface ILateTickable
    {
        event Action<float> LateTicked;
    }

    public interface IFixedTickable
    {
        event Action<float> FixedTicked;
    }
}