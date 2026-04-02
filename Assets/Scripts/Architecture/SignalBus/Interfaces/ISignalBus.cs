using Assets.Scripts.Architecture.SignalBus.Interfaces;
using System;

namespace Assets.Scripts.Gameplay.PlayerFeature.Components
{
    public interface ISignalBus<TEventInterface> : IDisposable
        where TEventInterface : IEvent
    {
        IDisposable Subscribe<TEvent>(Action<TEvent> handler)
            where TEvent : class, TEventInterface;

        void Publish<TEvent>(TEvent eventData)
            where TEvent : class, TEventInterface;

        bool TryPublish<TEvent>(TEvent eventData)
            where TEvent : class, TEventInterface;
    }
}