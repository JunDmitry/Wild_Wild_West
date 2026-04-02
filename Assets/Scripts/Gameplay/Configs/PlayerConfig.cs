using UnityEngine;

namespace Assets.Scripts.Gameplay.PlayerFeature.Components
{
    [CreateAssetMenu(menuName = "Configs/Characters/Player", fileName = "New Player Config", order = 51)]
    public class PlayerConfig : ScriptableObject
    {
        [SerializeField, Min(1)] private PlayerType _playerType = (PlayerType)1;

        public PlayerType PlayerType => _playerType;
    }
}