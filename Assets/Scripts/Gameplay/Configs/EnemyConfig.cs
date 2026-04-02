using UnityEngine;

namespace Assets.Scripts.Gameplay.PlayerFeature.Components
{
    [CreateAssetMenu(menuName = "Configs/Characters/Enemy", fileName = "New Enemy Config", order = 51)]
    public class EnemyConfig : ScriptableObject
    {
        [SerializeField, Min(1)] private EnemyType _enemyType = (EnemyType)1;
        [SerializeField] private EnemyVersion _commonVesion;
        [SerializeField] private EnemyVersion _bossVersion;

        public EnemyType EnemyType => _enemyType;

        public EnemyVersion GetContextForRarity(EnemyRarityType enemyRarity)
        {
            ThrowIf.Invalid(enemyRarity == EnemyRarityType.Unknown, $"{_enemyType} {enemyRarity} rarity don't exist");

            EnemyVersion enemyContext = null;

            if (enemyRarity == EnemyRarityType.Common)
                enemyContext = _commonVesion;
            else if (enemyRarity == EnemyRarityType.Boss)
                enemyContext = _bossVersion;

            return enemyContext;
        }
    }
}