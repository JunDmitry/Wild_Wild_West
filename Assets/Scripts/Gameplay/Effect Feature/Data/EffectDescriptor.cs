using Assets.Scripts.Gameplay.Effect_Feature.Factories;
using Assets.Scripts.Gameplay.Effect_Feature.Interfaces;
using System;
using UnityEngine;

namespace Assets.Scripts.Gameplay.Effect_Feature.Data
{
    [Serializable]
    public abstract class EffectDescriptor : IEffectDescriptor
    {
        [SerializeField] private string _effectId = nameof(EffectDescriptor);
        [SerializeField, Min(float.Epsilon)] private float _value = float.Epsilon;

        public string EffectId => _effectId;
        public float Value => _value;
        public abstract EffectType Type { get; }

        public abstract IEffect CreateEffect(IEffectFactory effectFactory);
    }
}