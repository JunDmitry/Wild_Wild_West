namespace Assets.Scripts.Gameplay.PlayerFeature.Components
{
    public class StaticDataService : IStaticDataService
    {
        private readonly StaticDataLoader<EnemyType, EnemyConfig> _enemyLoader;
        private readonly StaticDataLoader<PlayerType, PlayerConfig> _playerLoader;

        public StaticDataService(StaticDataPathsConfig pathsConfig)
        {
            _enemyLoader = new(pathsConfig.EnemyConfigPath, e => e.EnemyType);
            _playerLoader = new(pathsConfig.PlayerConfigPath, p => p.PlayerType);
        }

        public void LoadAll()
        {
            _enemyLoader.Load();
            _playerLoader.Load();
        }

        public EnemyConfig GetEnemyConfig(EnemyType enemyType)
        {
            return _enemyLoader.GetConfig(enemyType);
        }

        public PlayerConfig GetPlayerConfig(PlayerType playerType)
        {
            return _playerLoader.GetConfig(playerType);
        }
    }
}