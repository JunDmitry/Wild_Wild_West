using Assets.Scripts.Gameplay.Effect_Feature.Configs;
using Assets.Scripts.Gameplay.Effect_Feature.Data;
using Assets.Scripts.Gameplay.Effect_Feature.Interfaces;
using Assets.Scripts.Gameplay.Stat_Feature;
using System.Collections.Generic;

namespace Assets.Scripts.Gameplay.Effect_Feature.Implementations
{
    public class StrengthResistanceEffector : IStrengthResistanceEffector
    {
        private const float MaxResistance = 1f;

        private readonly IStatsService _statsService;
        private readonly Dictionary<EffectType, StatType> _statTypeByEffectType;

        public StrengthResistanceEffector(StrengthResistanceMapConfig strengthResistanceMap, IStatsService statsService)
        {
            ThrowIf.Null(statsService, nameof(statsService));

            _statsService = statsService;
            _statTypeByEffectType = new();

            InitializeStatTypesMap(strengthResistanceMap);
        }

        public float ApplyResistance(EffectType effectType, float amplifiedStrength, int targetId)
        {
            float multiplier = MaxResistance;

            if (_statTypeByEffectType.TryGetValue(effectType, out StatType statType)
                && _statsService.TryGetStat(targetId, statType, out Stat targetStat))
                multiplier -= targetStat.Normalized;

            return amplifiedStrength * multiplier;
        }

        private void InitializeStatTypesMap(StrengthResistanceMapConfig strengthResistanceMap)
        {
            foreach (ResistanceStatPair resistanceStatPair in strengthResistanceMap.ResistanceStatPairs)
                _statTypeByEffectType[resistanceStatPair.EffectTypeToAffect] = resistanceStatPair.ResistanceStatType;
        }
    }
}