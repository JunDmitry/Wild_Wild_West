using Assets.Scripts.Gameplay.Stat_Feature;
using System;
using UnityEngine;

namespace Assets.Scripts.Gameplay.Effect_Feature.Data
{
    [Serializable]
    public struct ResistanceStatPair
    {
        [SerializeField] private StatType _resistanceStatType;
        [SerializeField] private EffectType _effectTypeToAffect;

        public ResistanceStatPair(StatType resistanceStatType, EffectType effectTypeToAffect)
        {
            _resistanceStatType = resistanceStatType;
            _effectTypeToAffect = effectTypeToAffect;
        }

        public readonly StatType ResistanceStatType => _resistanceStatType;
        public readonly EffectType EffectTypeToAffect => _effectTypeToAffect;
    }
}