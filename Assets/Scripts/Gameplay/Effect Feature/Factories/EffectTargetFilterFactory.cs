using Assets.Scripts.Gameplay.Effect_Feature.Data;
using Assets.Scripts.Gameplay.Effect_Feature.Events;
using Assets.Scripts.Gameplay.Effect_Feature.Implementations;
using Assets.Scripts.Gameplay.Effect_Feature.Interfaces;
using Assets.Scripts.Gameplay.PlayerFeature.Components;

namespace Assets.Scripts.Gameplay.Effect_Feature.Factories
{
    public class EffectTargetFilterFactory : IEffectTargetFilterFactory
    {
        private readonly ISignalBus<IEffectEvent> _signalBus;

        public EffectTargetFilterFactory(ISignalBus<IEffectEvent> signalBus)
        {
            ThrowIf.Null(signalBus, nameof(signalBus));

            _signalBus = signalBus;
        }

        public IEffectTargetFilter Create(EffectType includeEffectType)
        {
            return new EffectTargetFilter(includeEffectType, _signalBus);
        }
    }
}