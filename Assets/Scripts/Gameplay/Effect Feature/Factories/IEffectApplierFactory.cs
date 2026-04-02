using Assets.Scripts.Gameplay.Effect_Feature.Interfaces;

namespace Assets.Scripts.Gameplay.Effect_Feature.Factories
{
    public interface IEffectApplierFactory
    {
        TApplier Create<TApplier>() where TApplier : IEffectApplier;

        TApplier Create<TApplier>(object[] args) where TApplier : IEffectApplier;
    }
}