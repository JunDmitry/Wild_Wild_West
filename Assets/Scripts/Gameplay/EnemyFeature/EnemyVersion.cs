using Assets.Scripts.Gameplay.HealthFeature;
using System;
using UnityEngine;

namespace Assets.Scripts.Gameplay.EnemyFeature
{
    [Serializable]
    public class EnemyVersion
    {
        [SerializeField] private HealthContext _healthContext;

        public HealthContext HealthContext;
    }
}