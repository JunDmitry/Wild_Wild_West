namespace Assets.Scripts.Gameplay.Effect_Feature.Interfaces
{
    public interface IEffectStrengthAmplifier
    {
        float Amplify(IEffect effect, int producerId);
    }
}