using Assets.Scripts.Gameplay.Effect_Feature.Data;

namespace Assets.Scripts.Gameplay.Effect_Feature.Events
{
    public class EffectUnappliedEvent : EffectEvent
    {
        public EffectUnappliedEvent(EffectApplicationContext effectApplicationContext)
            : base(effectApplicationContext)
        {
        }
    }
}