using Assets.Scripts.Gameplay.Effect_Feature.Interfaces;

namespace Assets.Scripts.Gameplay.Effect_Feature.Data
{
    public class EffectApplicationContext
    {
        public EffectApplicationContext(IEffect effect, int producerId, int targetId)
        {
            Effect = effect;
            ProducerId = producerId;
            TargetId = targetId;
        }

        public IEffect Effect { get; }
        public int ProducerId { get; }
        public int TargetId { get; }
    }
}