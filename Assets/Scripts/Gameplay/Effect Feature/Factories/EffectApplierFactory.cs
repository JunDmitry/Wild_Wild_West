using Assets.Scripts.Gameplay.Effect_Feature.Interfaces;
using Assets.Scripts.Gameplay.PlayerFeature.Components;

namespace Assets.Scripts.Gameplay.Effect_Feature.Factories
{
    public class EffectApplierFactory : IEffectApplierFactory
    {
        private readonly IResolver _resolver;

        public EffectApplierFactory(IResolver resolver)
        {
            _resolver = resolver;
        }

        public TApplier Create<TApplier>() where TApplier : IEffectApplier
        {
            return _resolver.Instantiate<TApplier>();
        }

        public TApplier Create<TApplier>(params object[] extraArgs) where TApplier : IEffectApplier
        {
            return _resolver.Instantiate<TApplier>(extraArgs);
        }
    }
}