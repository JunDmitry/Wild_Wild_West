using Assets.Scripts.Gameplay.Effect_Feature.Data;

namespace Assets.Scripts.Gameplay.Effect_Feature.Events
{
    public class EffectAppliedEvent : EffectEvent
    {
        public EffectAppliedEvent(EffectApplicationContext effectApplicationContext)
            : base(effectApplicationContext)
        {
        }
    }
}