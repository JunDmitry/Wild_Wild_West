using Assets.Scripts.Gameplay.HealthFeature;
using System;
using UnityEngine;

namespace Assets.Scripts.Gameplay.PlayerFeature.Components
{
    [Serializable]
    public class EnemyVersion
    {
        [SerializeField] private HealthContext _healthContext;

        public HealthContext HealthContext;
    }
}