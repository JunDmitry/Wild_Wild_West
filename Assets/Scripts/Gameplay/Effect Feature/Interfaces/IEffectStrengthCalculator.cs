namespace Assets.Scripts.Gameplay.Effect_Feature.Interfaces
{
    public interface IEffectStrengthCalculator
    {
        float Calculate(IEffect effect, int producerId, int targetId);
    }
}