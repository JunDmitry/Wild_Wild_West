using Assets.Scripts.Gameplay.Effect_Feature.Factories;
using Assets.Scripts.Gameplay.Effect_Feature.Interfaces;
using System.Collections.Generic;

namespace Assets.Scripts.Gameplay.Effect_Feature.Implementations
{
    public class CompositeEffectApplier : IEffectApplier
    {
        private readonly List<IEffectApplier> _effectAppliers;

        public CompositeEffectApplier(IEffectApplierFactory effectApplierFactory)
        {
            _effectAppliers = new();

            Initialize(effectApplierFactory);
        }

        public void Execute()
        {
            foreach (IEffectApplier effectApplier in _effectAppliers)
                effectApplier.Execute();
        }

        private void Initialize(IEffectApplierFactory effectApplierFactory)
        {
            // TODO
        }
    }
}