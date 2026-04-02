using Assets.Scripts.Gameplay.Stat_Feature;
using System;
using UnityEngine;

namespace Assets.Scripts.Gameplay.Effect_Feature.Data
{
    [Serializable]
    public struct EffectAmplify
    {
        [SerializeField] private StatType _statType;
        [SerializeField] private EffectType _effectType;
        [SerializeField] private float _percentagePerStatValue;

        public EffectAmplify(StatType statType, EffectType effectType, float percentagePerStatValue)
        {
            _statType = statType;
            _effectType = effectType;
            _percentagePerStatValue = percentagePerStatValue;
        }

        public readonly StatType StatType => _statType;
        public readonly EffectType EffectType => _effectType;
        public readonly float PercentagePerStatValue => _percentagePerStatValue;
    }
}