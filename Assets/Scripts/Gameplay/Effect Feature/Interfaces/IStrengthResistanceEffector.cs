using Assets.Scripts.Gameplay.Effect_Feature.Data;

namespace Assets.Scripts.Gameplay.Effect_Feature.Interfaces
{
    public interface IStrengthResistanceEffector
    {
        float ApplyResistance(EffectType effectType, float amplifiedStrength, int targetId);
    }
}