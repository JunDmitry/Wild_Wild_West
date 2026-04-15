using Assets.Scripts.Gameplay.PlayerFeature;
using UnityEngine;

namespace Assets.Scripts.Gameplay.Configs
{
    [CreateAssetMenu(menuName = "Configs/Characters/Player", fileName = "New Player Config", order = 51)]
    public class PlayerConfig : ScriptableObject, IModelData
    {
        [SerializeField] private string _name;
        [SerializeField] private string _viewPrefabPath;
        [SerializeField, Min(1)] private PlayerType _playerType = (PlayerType)1;

        public string Name => _name;
        public string ViewPrefabPath => _viewPrefabPath;
        public PlayerType PlayerType => _playerType;
    }
}