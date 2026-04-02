namespace Assets.Scripts.Gameplay.Effect_Feature.Interfaces
{
    public interface IEffectApplyService
    {
        void ApplyEffects(IEffectTarget target, IEffectProducer effectProducer);

        void ApplyEffects(int targetId, int producerId);

        void UnapplyEffect(IEffect effect, IEffectTarget target, int producerId);

        void UnapplyEffect(IEffect effect, int targetId, int producerId);
    }
}