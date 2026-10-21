using Game.Arena.Application.Sessions;
using Game.Arena.Domain.Combat;
using Game.Arena.Domain.Configuration;
using Game.Arena.Domain.Geometry;
using Game.Arena.Domain.Identity;
using Game.Arena.Domain.Interactions.Movement;
using Game.Arena.Domain.Time;
using Game.Arena.Domain.Vitality;

namespace Game.Arena.Application.Tests.Sessions
{
    internal sealed class ArenaRunCreationParametersFactory
    {
        public ArenaRunCreationParameters Create()
        {
            ArenaDefinition arena = new(
                new ArenaBounds(-20f, 20f, -20f, 20f, 0f),
                Distance.FromValue(5f));

            PlayerDefinition player = new(
                Position3D.Zero,
                Health.Full(100),
                MovementSpeed.FromUnitsPerSecond(5f),
                CollisionRadius.FromValue(0.5f));

            WeaponCatalog weapons = new(
                new WeaponDefinition(
                    WeaponKind.Ranged,
                    DamageAmount.FromPoints(10),
                    Distance.FromValue(50f),
                    new GameDuration(0.4d),
                    new GameDuration(0d)),
                new WeaponDefinition(
                    WeaponKind.Melee,
                    DamageAmount.FromPoints(20),
                    Distance.FromValue(2f),
                    new GameDuration(0.8d),
                    new GameDuration(0.3d)));

            EnemyCatalog enemies = new(
                new EnemyDefinition(
                    EnemyKind.Regular,
                    Health.Full(50),
                    MovementSpeed.FromUnitsPerSecond(2f),
                    CollisionRadius.FromValue(0.5f),
                    DamageAmount.FromPoints(5),
                    Distance.FromValue(2f),
                    new GameDuration(1d),
                    new GameDuration(0.2d)),
                new EnemyDefinition(
                    EnemyKind.Boss,
                    Health.Full(300),
                    MovementSpeed.FromUnitsPerSecond(1.75f),
                    CollisionRadius.FromValue(1f),
                    DamageAmount.FromPoints(25),
                    Distance.FromValue(3f),
                    new GameDuration(1.1d),
                    new GameDuration(0.4d)));

            WaveCatalog waves = new(
                new[]
                {
                    new WaveDefinition(WaveNumber.First, 10),
                });

            return new ArenaRunCreationParameters(
                arena,
                player,
                weapons,
                enemies,
                waves);
        }
    }
}
