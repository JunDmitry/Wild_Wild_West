using Assets.Scripts.Architecture.Repository.Interfaces;
using Assets.Scripts.Gameplay.Common.Interfaces;
using Assets.Scripts.Gameplay.Effect_Feature.Factories;
using Assets.Scripts.Gameplay.Effect_Feature.Interfaces;
using System.Collections.Generic;
using UnityEngine;

namespace Assets.Scripts.Gameplay.Effect_Feature.Implementations
{
    public class EffectApplyService : IEffectApplyService
    {
        private readonly IEffectFactory _effectFactory;
        private readonly IRepository<int, IModel> _repository;

        public EffectApplyService(IEffectFactory effectFactory, IRepository<int, IModel> repository)
        {
            _effectFactory = effectFactory;
            _repository = repository;
        }

        public void ApplyEffects(int targetId, int producerId)
        {
            if (TryGetModel(targetId, nameof(IEffectTarget), out IEffectTarget target) == false)
                return;

            if (TryGetModel(producerId, nameof(IEffectProducer), out IEffectProducer producer) == false)
                return;

            ApplyEffects(target, producer);
        }

        public void ApplyEffects(IEffectTarget target, IEffectProducer effectProducer)
        {
            IReadOnlyList<IEffectDescriptor> effectDescriptors = effectProducer.Effects;
            int producerId = effectProducer.ProducerId;

            foreach (IEffectDescriptor effectDescriptor in effectDescriptors)
            {
                IEffect effect = effectDescriptor.CreateEffect(_effectFactory);

                target.Apply(effect, producerId);
            }
        }

        public void UnapplyEffect(IEffect effect, int targetId, int producerId)
        {
            if (TryGetModel(targetId, nameof(IEffectTarget), out IEffectTarget target) == false)
                return;

            UnapplyEffect(effect, target, producerId);
        }

        public void UnapplyEffect(IEffect effect, IEffectTarget target, int producerId)
        {
            target.Unapply(effect, producerId);
        }

        private bool TryGetModel<T>(int id, string typeName, out T model)
            where T : class, IModel
        {
            if (_repository.TryGetItem(id, out model) == false)
            {
                Debug.LogWarning($"{typeName} with id {id} was not found");
                return false;
            }

            return true;
        }
    }
}