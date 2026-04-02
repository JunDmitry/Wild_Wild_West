using Assets.Scripts.Architecture.SignalBus.Interfaces;
using Assets.Scripts.Gameplay.PlayerFeature.Components;
using System;
using System.Collections.Generic;
using System.Linq;

namespace Assets.Scripts.Architecture.SignalBus
{
    public class SignalBus<TEventInterface> : ISignalBus<TEventInterface>
        where TEventInterface : IEvent
    {
        private readonly Dictionary<Type, HashSet<Delegate>> _handlersByType;
        private readonly List<IDisposable> _disposables;

        public SignalBus()
        {
            _handlersByType = new();
            _disposables = new();
        }

        public void Dispose()
        {
            foreach (IDisposable disposable in _disposables)
            {
                disposable.Dispose();
            }

            _disposables.Clear();
        }

        public bool TryPublish<TEvent>(TEvent eventData)
            where TEvent : class, TEventInterface
        {
            Type eventType = typeof(TEvent);

            if (_handlersByType.ContainsKey(eventType) == false)
                return false;

            Publish(eventData);

            return true;
        }

        public void Publish<TEvent>(TEvent eventData)
            where TEvent : class, TEventInterface
        {
            Type eventType = typeof(TEvent);

            ThrowIf.Invalid(_handlersByType.TryGetValue(eventType, out HashSet<Delegate> handlers) == false,
                $"Subscribers count should be positive for {nameof(Publish)} event");

            List<Exception> exceptions = null;

            foreach (Action<TEvent> concreteHandler in handlers.Cast<Action<TEvent>>())
            {
                try
                {
                    concreteHandler(eventData);
                }
                catch (Exception ex)
                {
                    exceptions ??= new();
                    exceptions.Add(ex);
                }
            }

            if (exceptions != null)
                throw new AggregateException("One or more handler throw exception", exceptions);
        }

        public IDisposable Subscribe<TEvent>(Action<TEvent> handler)
            where TEvent : class, TEventInterface
        {
            ThrowIf.Null(handler, nameof(handler));

            Type eventType = typeof(TEvent);

            if (_handlersByType.TryGetValue(eventType, out HashSet<Delegate> handlers) == false)
            {
                handlers = new();
                _handlersByType[eventType] = handlers;
            }

            Delegate innerHandler = handler;
            IDisposable disposable = new DisposableDelegate(() => _handlersByType[eventType]?.Remove(innerHandler));
            handlers.Add(innerHandler);

            return disposable;
        }
    }
}