using Assets.Scripts.Gameplay.Effect_Feature.Data;
using Assets.Scripts.Gameplay.Effect_Feature.Interfaces;

namespace Assets.Scripts.Gameplay.Effect_Feature.Factories
{
    public interface IEffectTargetFilterFactory
    {
        IEffectTargetFilter Create(EffectType includeEffectType);
    }
}