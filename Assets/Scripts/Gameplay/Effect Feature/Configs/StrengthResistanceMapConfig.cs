using Assets.Scripts.Gameplay.Effect_Feature.Data;
using System.Collections.Generic;
using UnityEngine;

namespace Assets.Scripts.Gameplay.Effect_Feature.Configs
{
    [CreateAssetMenu(menuName = "Configs/Effect/StrengthResistanceMap", fileName = "New StrengthResistanceMap", order = 51)]
    public class StrengthResistanceMapConfig : ScriptableObject
    {
        [SerializeField] private List<ResistanceStatPair> _resistanceStatPairs;

        public IReadOnlyList<ResistanceStatPair> ResistanceStatPairs => _resistanceStatPairs.AsReadOnly();
    }
}