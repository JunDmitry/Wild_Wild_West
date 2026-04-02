using Assets.Scripts.Gameplay.Effect_Feature.Data;
using System.Collections.Generic;

namespace Assets.Scripts.Gameplay.Effect_Feature.Interfaces
{
    public interface IEffectTargetFilter
    {
        IEnumerable<EffectApplicationContext> GetEffectTargets();

        int GetEffectTargets(EffectApplicationContext[] effectApplicationBuffer);
    }
}