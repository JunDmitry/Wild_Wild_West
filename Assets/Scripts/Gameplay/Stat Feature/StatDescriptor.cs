using System;
using UnityEngine;

namespace Assets.Scripts.Gameplay.Stat_Feature
{
    [Serializable]
    public class StatDescriptor
    {
        [SerializeField] private StatType _type;
        [SerializeField] private float _baseValue;
        [SerializeField] private float _minValue;
        [SerializeField] private float _maxValue;

        public StatDescriptor(StatType type, float baseValue, float minValue, float maxValue)
        {
            _type = type;
            _baseValue = baseValue;
            _minValue = minValue;
            _maxValue = maxValue;
        }

        public StatType Type => _type;
        public float BaseValue => _baseValue;
        public float MinValue => _minValue;
        public float MaxValue => _maxValue;
    }
}