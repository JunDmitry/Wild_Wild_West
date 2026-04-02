using Assets.Scripts.Gameplay.Effect_Feature.Data;
using Assets.Scripts.Gameplay.Effect_Feature.Events;
using Assets.Scripts.Gameplay.Effect_Feature.Interfaces;
using Assets.Scripts.Gameplay.PlayerFeature.Components;
using System;
using System.Collections.Generic;

namespace Assets.Scripts.Gameplay.Effect_Feature.Implementations
{
    public class EffectTargetFilter : IEffectTargetFilter, IDisposable
    {
        private readonly EffectType _includeEffectType;
        private readonly List<IDisposable> _disposables;

        private readonly Dictionary<int, HashSet<EffectApplicationContext>> _effectsByTargetId;

        public EffectTargetFilter(EffectType includeEffectType, ISignalBus<IEffectEvent> signalBus)
        {
            ThrowIf.Null(signalBus, nameof(signalBus));

            _includeEffectType = includeEffectType;
            _disposables = new();
            _effectsByTargetId = new();

            SubscribeToEvents(signalBus);
        }

        public IEnumerable<EffectApplicationContext> GetEffectTargets()
        {
            foreach (int targetId in _effectsByTargetId.Keys)
            {
                if (_effectsByTargetId[targetId].Count <= 0)
                    continue;

                foreach (EffectApplicationContext effect in _effectsByTargetId[targetId])
                    yield return effect;
            }
        }

        public int GetEffectTargets(EffectApplicationContext[] effectApplicationBuffer)
        {
            int count = 0;

            foreach (EffectApplicationContext effect in GetEffectTargets())
            {
                if (count >= effectApplicationBuffer.Length)
                    break;

                effectApplicationBuffer[count] = effect;
                count++;
            }

            return count;
        }

        public void Dispose()
        {
            foreach (IDisposable disposable in _disposables)
                disposable.Dispose();

            _disposables.Clear();
        }

        private void SubscribeToEvents(ISignalBus<IEffectEvent> signalBus)
        {
            _disposables.Add(signalBus.Subscribe<EffectAppliedEvent>(OnEffectAppliedEvent));
            _disposables.Add(signalBus.Subscribe<EffectUnappliedEvent>(OnEffectUnappliedEvent));
        }

        private void OnEffectAppliedEvent(EffectAppliedEvent effectAppliedEvent)
        {
            EffectApplicationContext effectApplicationContext = effectAppliedEvent.EffectApplicationContext;

            if (effectApplicationContext.Effect.Type != _includeEffectType)
                return;

            if (_effectsByTargetId.TryGetValue(effectApplicationContext.TargetId, out HashSet<EffectApplicationContext> effects) == false)
            {
                effects = new();
                _effectsByTargetId[effectApplicationContext.TargetId] = effects;
            }

            effects.Add(effectApplicationContext);
        }

        private void OnEffectUnappliedEvent(EffectUnappliedEvent effectUnappliedEvent)
        {
            EffectApplicationContext effectApplicationContext = effectUnappliedEvent.EffectApplicationContext;

            if (effectApplicationContext.Effect.Type != _includeEffectType)
                return;

            if (_effectsByTargetId.TryGetValue(effectApplicationContext.TargetId, out HashSet<EffectApplicationContext> effects) == false)
                return;

            effects.Remove(effectApplicationContext);

            if (effects.Count == 0)
                _effectsByTargetId.Remove(effectApplicationContext.TargetId);
        }
    }
}