using Assets.Scripts.Gameplay.Effect_Feature.Data;

namespace Assets.Scripts.Gameplay.Effect_Feature.Interfaces
{
    public interface IEffect
    {
        EffectType Type { get; }
        float Value { get; }
    }
}