using System.Collections.Generic;
using Game.Core.Model.Configs;
using Game.Core.Model.Entities;
using Game.Core.Model.Enums;
using Game.Core.Model.States;

namespace Game.Legacy.Characterization.Tests
{
    internal sealed class LegacyScenarioFactory
    {
        public GameConfig CreateConfig()
        {
            ArenaConfig arena = new(
                -20f,
                20f,
                -20f,
                20f,
                0f,
                0.5f);

            PlayerConfig player = new(
                100,
                Position3D.Zero,
                0f,
                0f,
                5f);

            WeaponConfig rangedWeapon = new(
                WeaponKind.Ranged,
                0.4f,
                10f,
                100f);

            WeaponConfig meleeWeapon = new(
                WeaponKind.Melee,
                0.8f,
                17.5f,
                5f);

            EnemyConfig regularEnemy = new(
                EnemyKind.Regular,
                50,
                2f,
                7.5f,
                2f,
                1f);

            EnemyConfig bossEnemy = new(
                EnemyKind.Boss,
                300,
                1.75f,
                25f,
                3f,
                1.1f);

            WaveConfig[] waves =
            {
                new(1, 2, true),
                new(2, 1, true),
                new(3, 1, true),
            };

            return new GameConfig(
                arena,
                player,
                rangedWeapon,
                meleeWeapon,
                regularEnemy,
                bossEnemy,
                waves,
                2f);
        }

        public PlayerState CreatePlayer(
            EntityId id,
            WeaponKind selectedWeapon,
            float currentHealth = 100f,
            float rangedReadyTime = 0f,
            float meleeReadyTime = 0f)
        {
            return new PlayerState(
                id,
                Position3D.Zero,
                currentHealth,
                100f,
                selectedWeapon,
                rangedReadyTime,
                meleeReadyTime);
        }

        public EnemyState CreateEnemy(
            EntityId id,
            EnemyKind kind,
            Position3D position,
            float currentHealth,
            float attackReadyTime = 0f)
        {
            float maxHealth = kind == EnemyKind.Regular
                ? 50f
                : 300f;

            return new EnemyState(
                id,
                kind,
                position,
                currentHealth,
                maxHealth,
                attackReadyTime);
        }

        public GameState CreateState(
            PlayerState player,
            IReadOnlyDictionary<EntityId, EnemyState> enemies,
            WaveState wave,
            GamePhase phase = GamePhase.Playing,
            float time = 0f,
            float phaseEnteredTime = 0f)
        {
            return new GameState(
                phase,
                player,
                enemies,
                wave,
                time,
                phaseEnteredTime);
        }

        public Dictionary<EntityId, EnemyState> CreateEnemies(
            params EnemyState[] enemies)
        {
            Dictionary<EntityId, EnemyState> result = new();

            foreach (EnemyState enemy in enemies)
            {
                result.Add(enemy.Id, enemy);
            }

            return result;
        }
    }
}
