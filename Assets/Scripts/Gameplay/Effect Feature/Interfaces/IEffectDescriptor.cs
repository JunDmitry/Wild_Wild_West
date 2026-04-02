using Assets.Scripts.Gameplay.Effect_Feature.Factories;

namespace Assets.Scripts.Gameplay.Effect_Feature.Interfaces
{
    public interface IEffectDescriptor
    {
        IEffect CreateEffect(IEffectFactory effectFactory);
    }
}