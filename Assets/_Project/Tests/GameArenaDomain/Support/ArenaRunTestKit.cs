using System.Collections.Generic;
using Game.Arena.Domain.Aggregates.ArenaRun;
using Game.Arena.Domain.Combat;
using Game.Arena.Domain.Configuration;
using Game.Arena.Domain.Geometry;
using Game.Arena.Domain.Identity;
using Game.Arena.Domain.Interactions.Movement;
using Game.Arena.Domain.Time;
using Game.Arena.Domain.Vitality;

namespace Game.Arena.Domain.Tests.Support
{
    internal sealed class ArenaRunTestKit
    {
        private readonly ArenaRunFactory _factory = new();

        public ArenaDefinition Arena { get; } = new ArenaDefinition(
            new ArenaBounds(-20f, 20f, -20f, 20f, 0f),
            Distance.FromValue(5f));

        public EnemyCatalog Enemies { get; } = new EnemyCatalog(
            new EnemyDefinition(
                EnemyKind.Regular,
                Health.Full(50),
                MovementSpeed.FromUnitsPerSecond(2f),
                CollisionRadius.FromValue(0.5f),
                DamageAmount.FromPoints(8),
                Distance.FromValue(2f),
                new GameDuration(1d)),
            new EnemyDefinition(
                EnemyKind.Boss,
                Health.Full(300),
                MovementSpeed.FromUnitsPerSecond(1.75f),
                CollisionRadius.FromValue(1f),
                DamageAmount.FromPoints(25),
                Distance.FromValue(3f),
                new GameDuration(1.1d)));

        public WeaponCatalog Weapons { get; } = new WeaponCatalog(
            new WeaponDefinition(
                WeaponKind.Ranged,
                DamageAmount.FromPoints(10),
                Distance.FromValue(50f),
                new GameDuration(0.4d)),
            new WeaponDefinition(
                WeaponKind.Melee,
                DamageAmount.FromPoints(20),
                Distance.FromValue(2f),
                new GameDuration(0.8d)));

        public PlayerDefinition PlayerAt(Position3D startPosition)
        {
            return new PlayerDefinition(
                startPosition,
                Health.Full(100),
                MovementSpeed.FromUnitsPerSecond(5f),
                CollisionRadius.FromValue(0.5f));
        }

        public WaveCatalog Waves(params int[] regularCounts)
        {
            List<WaveDefinition> definitions = new();
            WaveNumber number = WaveNumber.First;

            foreach (int count in regularCounts)
            {
                definitions.Add(new WaveDefinition(number, count));
                number = number.Next();
            }

            return new WaveCatalog(definitions);
        }

        public ArenaRun StartRun()
        {
            return StartRun(Position3D.Zero, Waves(2, 1, 1));
        }

        public ArenaRun StartRun(Position3D playerStart)
        {
            return StartRun(playerStart, Waves(2, 1, 1));
        }

        public ArenaRun StartRun(Position3D playerStart, WaveCatalog waves)
        {
            return _factory.Start(
                ArenaRunId.FromValue(1UL),
                PlayerId.FromValue(2UL),
                Arena,
                PlayerAt(playerStart),
                Enemies,
                Weapons,
                waves);
        }
    }
}
