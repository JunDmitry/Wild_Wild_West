using UnityEngine;

namespace Assets.Scripts.Gameplay.PlayerFeature.Components
{
    [CreateAssetMenu(menuName = "Configs/Infrastructure/StaticDataPaths", fileName = "New Static Data Paths Config", order = 51)]
    public class StaticDataPathsConfig : ScriptableObject
    {
        [SerializeField] private string _enemyConfigsPath;
        [SerializeField] private string _playerConfigsPath;

        public string EnemyConfigPath => _enemyConfigsPath;
        public string PlayerConfigPath => _playerConfigsPath;
    }
}