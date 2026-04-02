using Assets.Scripts.Gameplay.Effect_Feature.Interfaces;

namespace Assets.Scripts.Gameplay.Effect_Feature.Implementations
{
    public class EffectStrengthCalculator : IEffectStrengthCalculator
    {
        private readonly IEffectStrengthAmplifier _effectStrengthAmplifier;
        private readonly IStrengthResistanceEffector _strengthResistanceEffector;

        public EffectStrengthCalculator(IEffectStrengthAmplifier effectStrengthAmplifier, IStrengthResistanceEffector strengthResistanceEffector)
        {
            ThrowIf.Null(effectStrengthAmplifier, nameof(effectStrengthAmplifier));
            ThrowIf.Null(strengthResistanceEffector, nameof(strengthResistanceEffector));

            _effectStrengthAmplifier = effectStrengthAmplifier;
            _strengthResistanceEffector = strengthResistanceEffector;
        }

        public float Calculate(IEffect effect, int producerId, int targetId)
        {
            float amplifiedStrength = _effectStrengthAmplifier.Amplify(effect, producerId);
            float finalStrength = _strengthResistanceEffector.ApplyResistance(effect.Type, amplifiedStrength, targetId);

            return finalStrength;
        }
    }
}