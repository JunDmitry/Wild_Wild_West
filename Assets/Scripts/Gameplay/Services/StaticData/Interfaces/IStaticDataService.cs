using Assets.Scripts.Gameplay.Configs;
using Assets.Scripts.Gameplay.EnemyFeature;

namespace Assets.Scripts.Gameplay.PlayerFeature.Components
{
    public interface IStaticDataService : IService
    {
        void LoadAll();

        EnemyConfig GetEnemyConfig(EnemyType enemyType);

        PlayerConfig GetPlayerConfig(PlayerType playerType);
    }
}