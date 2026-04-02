using System;
using UnityEngine;

namespace Assets.Scripts.Gameplay.HealthFeature
{
    [Serializable]
    public class HealthContext
    {
        [SerializeField, Min(float.Epsilon)] private float _maxValue = float.Epsilon;

        public float Max => _maxValue;
    }
}