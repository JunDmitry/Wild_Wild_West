using System;
using System.Collections.Generic;
using Game.Arena.Domain.Aggregates.ArenaRun;
using Game.Arena.Domain.Combat;
using Game.Arena.Domain.Configuration;
using Game.Arena.Domain.Geometry;
using Game.Arena.Domain.Identity;
using Game.Arena.Domain.Interactions.Movement;
using Game.Arena.Domain.Interactions.Spawn;
using Game.Arena.Domain.Movement;
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

        public ArenaDefinition SmallArena { get; } = new ArenaDefinition(
            new ArenaBounds(-2f, 2f, -2f, 2f, 0f),
            Distance.FromValue(5f));

        public ArenaDefinition CompactArena { get; } = new ArenaDefinition(
            new ArenaBounds(-1f, 1f, -1f, 1f, 0f),
            Distance.FromValue(5f));

        public EnemyCatalog Enemies { get; } = new EnemyCatalog(
            new EnemyDefinition(
                EnemyKind.Regular,
                Health.Full(50),
                MovementSpeed.FromUnitsPerSecond(2f),
                CollisionRadius.FromValue(0.5f),
                DamageAmount.FromPoints(8),
                Distance.FromValue(2f),
                new GameDuration(1d),
                new GameDuration(.2d)),
            new EnemyDefinition(
                EnemyKind.Boss,
                Health.Full(300),
                MovementSpeed.FromUnitsPerSecond(1.75f),
                CollisionRadius.FromValue(1f),
                DamageAmount.FromPoints(25),
                Distance.FromValue(3f),
                new GameDuration(1.1d),
                new GameDuration(.4d)));

        public WeaponCatalog Weapons { get; } = new WeaponCatalog(
            new WeaponDefinition(
                WeaponKind.Ranged,
                DamageAmount.FromPoints(10),
                Distance.FromValue(50f),
                new GameDuration(0.4d),
                new GameDuration(0)),
            new WeaponDefinition(
                WeaponKind.Melee,
                DamageAmount.FromPoints(20),
                Distance.FromValue(2f),
                new GameDuration(0.8d),
                new GameDuration(0.3d)));

        public MovementPathPolicy MovementPath { get; } = new MovementPathPolicy();

        public EnemyCatalog WeakEnemies { get; } = new EnemyCatalog(
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
                DamageAmount.FromPoints(9),
                Distance.FromValue(3f),
                new GameDuration(1.1d),
                new GameDuration(0.4d)));

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

        public ArenaRun StartRunInSmallArena()
        {
            return new ArenaRunFactory().Start(
                ArenaRunId.FromValue(1UL),
                PlayerId.FromValue(2UL),
                SmallArena,
                PlayerAt(Position3D.Zero),
                Enemies,
                Weapons,
                Waves(2, 1, 1));
        }

        public ArenaRun StartRunInCompactArena()
        {
            return StartRunInCompactArena(Waves(2, 1, 1));
        }

        public ArenaRun StartRunInCompactArena(WaveCatalog waves)
        {
            return new ArenaRunFactory().Start(
                ArenaRunId.FromValue(1UL),
                PlayerId.FromValue(2UL),
                CompactArena,
                PlayerAt(Position3D.Zero),
                Enemies,
                Weapons,
                waves);
        }

        public ArenaRun StartRunInCompactArenaWithWeakEnemies()
        {
            return new ArenaRunFactory(MovementPath).Start(
                ArenaRunId.FromValue(1UL),
                PlayerId.FromValue(2UL),
                CompactArena,
                PlayerAt(Position3D.Zero),
                WeakEnemies,
                Weapons,
                Waves(2, 1, 1));
        }

        public ArenaRun StartRunWithSameId(ArenaRunId arenaRunId)
        {
            return new ArenaRunFactory(MovementPath).Start(
                arenaRunId,
                PlayerId.FromValue(777UL),
                Arena,
                PlayerAt(Position3D.Zero),
                Enemies,
                Weapons,
                Waves(2, 1, 1));
        }

        public EnemyId Spawn(ArenaRun run, ulong value, Position3D position)
        {
            EnemySpawnRequest request = run.RequestEnemySpawn().Request;
            EnemyId enemyId = EnemyId.FromValue(value);

            run.ApplyEnemySpawn(new EnemySpawnResolution(request.Correlation, enemyId, position));

            return enemyId;
        }

        public ArenaRun StartCompactRunWithPlayerHealth(int max, int current)
        {
            return new ArenaRunFactory().Start(
                ArenaRunId.FromValue(1UL),
                PlayerId.FromValue(2UL),
                CompactArena,
                new PlayerDefinition(
                    Position3D.Zero,
                    Health.Full(max).ApplyDamage(DamageAmount.FromPoints(max - current)).RemainingHealth,
                    MovementSpeed.FromUnitsPerSecond(5f),
                    CollisionRadius.FromValue(0.5f)),
                Enemies,
                Weapons,
                Waves(2, 1, 1));
        }
    }
}
