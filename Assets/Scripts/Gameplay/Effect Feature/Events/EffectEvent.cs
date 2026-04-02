using Assets.Scripts.Gameplay.Effect_Feature.Data;

namespace Assets.Scripts.Gameplay.Effect_Feature.Events
{
    public abstract class EffectEvent : IEffectEvent
    {
        public EffectEvent(EffectApplicationContext effectApplicationContext)
        {
            EffectApplicationContext = effectApplicationContext;
        }

        public EffectApplicationContext EffectApplicationContext { get; }
    }
}