using Assets.Scripts.Gameplay.Effect_Feature.Data;
using System.Collections.Generic;
using UnityEngine;

namespace Assets.Scripts.Gameplay.Effect_Feature.Configs
{
    [CreateAssetMenu(menuName = "Configs/Effect/StrengthAmplify", fileName = "New EffectStrengthAmplify", order = 51)]
    public class EffectStrengthAmplifyConfig : ScriptableObject
    {
        [SerializeField] private List<EffectAmplify> _effectAmplifies;

        public IReadOnlyList<EffectAmplify> EffectAmplifies => _effectAmplifies.AsReadOnly();
    }
}