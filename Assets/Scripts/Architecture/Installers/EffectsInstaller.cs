using Assets.Scripts.Gameplay.Effect_Feature.Configs;
using Assets.Scripts.Gameplay.Effect_Feature.Factories;
using Assets.Scripts.Gameplay.Effect_Feature.Implementations;
using Assets.Scripts.Gameplay.Effect_Feature.Interfaces;
using Assets.Scripts.Gameplay.Stat_Feature;
using UnityEngine;
using Zenject;

namespace Assets.Scripts.Architecture.Installers
{
    public class EffectsInstaller : MonoInstaller
    {
        [SerializeField] private EffectStrengthAmplifyConfig _amplifyConfig;
        [SerializeField] private StrengthResistanceMapConfig _strengthResistanceMapConfig;

        public override void InstallBindings()
        {
            BindConfigs();
            BindFactories();
            BindAppliers();
            BindEffectCalculation();
            BindStats();
        }

        private void BindConfigs()
        {
            Container.Bind<EffectStrengthAmplifyConfig>().ToSelf().FromInstance(_amplifyConfig).AsSingle();
            Container.Bind<StrengthResistanceMapConfig>().ToSelf().FromInstance(_strengthResistanceMapConfig).AsSingle();
        }

        private void BindFactories()
        {
            Container.Bind<IEffectFactory>().To<EffectFactory>().AsSingle();
            Container.Bind<IEffectApplierFactory>().To<EffectApplierFactory>().AsSingle();
            Container.Bind<IEffectTargetFilterFactory>().To<EffectTargetFilterFactory>().AsSingle();
        }

        private void BindAppliers()
        {
            Container.Bind<IEffectApplier>().To<CompositeEffectApplier>().AsSingle();

            Container.Bind<IEffectApplyService>().To<EffectApplyService>().AsSingle();
        }

        private void BindEffectCalculation()
        {
            Container.Bind<IEffectStrengthAmplifier>().To<EffectStrengthAmplifier>().AsSingle();
            Container.Bind<IStrengthResistanceEffector>().To<StrengthResistanceEffector>().AsSingle();
            Container.Bind<IEffectStrengthCalculator>().To<EffectStrengthCalculator>().AsSingle();
        }

        private void BindStats()
        {
            Container.BindInterfacesTo<StatsService>().AsSingle();
        }
    }
}