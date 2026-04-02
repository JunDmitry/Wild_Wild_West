using Assets.Scripts.Gameplay.Effect_Feature.Configs;
using Assets.Scripts.Gameplay.Effect_Feature.Data;
using Assets.Scripts.Gameplay.Effect_Feature.Interfaces;
using Assets.Scripts.Gameplay.Stat_Feature;
using System.Collections.Generic;

namespace Assets.Scripts.Gameplay.Effect_Feature.Implementations
{
    public class EffectStrengthAmplifier : IEffectStrengthAmplifier
    {
        private readonly IStatsService _statsService;
        private readonly Dictionary<EffectType, List<EffectAmplify>> _effectAmplifiesByType;

        public EffectStrengthAmplifier(IStatsService statsService, EffectStrengthAmplifyConfig strengthAmplifyConfig)
        {
            ThrowIf.Null(statsService, nameof(statsService));
            ThrowIf.Null(strengthAmplifyConfig, nameof(strengthAmplifyConfig));

            _statsService = statsService;
            _effectAmplifiesByType = new();

            InitializeEffectAmplifies(strengthAmplifyConfig);
        }

        public float Amplify(IEffect effect, int producerId)
        {
            ThrowIf.Null(effect, nameof(effect));

            float totalMultiplier = 1f;

            foreach (EffectAmplify effectAmplify in _effectAmplifiesByType[effect.Type])
            {
                if (_statsService.TryGetStat(producerId, effectAmplify.StatType, out Stat stat) == false)
                    continue;

                totalMultiplier += stat.CurrentValue * effectAmplify.PercentagePerStatValue;
            }

            return effect.Value * totalMultiplier;
        }

        private void InitializeEffectAmplifies(EffectStrengthAmplifyConfig strengthAmplifyConfig)
        {
            foreach (EffectAmplify effectAmplify in strengthAmplifyConfig.EffectAmplifies)
            {
                if (_effectAmplifiesByType.TryGetValue(effectAmplify.EffectType, out List<EffectAmplify> effectAmplifies) == false)
                {
                    effectAmplifies = new();
                    _effectAmplifiesByType[effectAmplify.EffectType] = effectAmplifies;
                }

                effectAmplifies.Add(effectAmplify);
            }
        }
    }
}