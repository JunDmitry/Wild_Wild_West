using Assets.Scripts.Gameplay.Common.Interfaces;
using Assets.Scripts.Gameplay.Effect_Feature.Data;
using System;

namespace Assets.Scripts.Gameplay.Effect_Feature.Interfaces
{
    public interface IEffectTarget : IModel
    {
        event Action<IEffect> EffectApplied;

        event Action<IEffect> EffectUnapplied;

        void Apply(IEffect effect, int producerId);

        void Unapply(IEffect effect, int producerId);

        int GetEffects(EffectApplicationContext[] effectsBuffer);
    }
}