using System;
using System.Collections.Generic;
using Game.Arena.Application.Ports;
using Game.Arena.Application.ReadModels;
using Game.Arena.Application.Spawning;
using Game.Arena.Application.Ticks;
using Game.Arena.Domain.Combat;
using Game.Arena.Domain.Configuration;
using Game.Arena.Domain.Geometry;
using Game.Arena.Domain.Identity;
using Game.Arena.Domain.Interactions.Movement;
using Game.Arena.Domain.Movement;
using Game.Arena.Domain.Time;
using Game.Arena.Infrastructure.Spawning;
using Game.Arena.Infrastructure.Targeting;
using NUnit.Framework;
using UnityEngine;

namespace Game.Arena.PlayMode.Tests
{
    [TestFixture]
    public sealed class EnemySpawnPlacementTickTests
    {
        private readonly List<GameObject> _objects = new();

        [TearDown]
        public void TearDown()
        {
            for (int index = 0; index < _objects.Count; index++)
            {
                UnityEngine.Object.DestroyImmediate(_objects[index]);
            }

            _objects.Clear();
        }

        [Test]
        public void Tick_WithRealPlacementResolver_SpawnsAnEnemyAcceptedByTheDomain()
        {
            ArenaDefinition arena = new(
                new ArenaBounds(-20f, 20f, -20f, 20f, 0f),
                Distance.FromValue(5f));

            Transform player = CreatePlayer();
            Camera camera = CreateCamera(new Vector3(0f, 10f, 0f));
            UnityEnemySpawnPlacementResolver resolver = new(arena, camera, player);

            PlayModeTickHarness harness = new(
                new EmptyEnemyColliderRegistry(),
                spawnPlacementResolver: resolver,
                spawnPacingPolicy: new AlwaysSpawnPacingPolicy());

            ArenaRunTickResult result = harness.TickIdle();

            Assert.That(result.Snapshot.Enemies, Has.Count.EqualTo(1));

            EnemySnapshot enemy = result.Snapshot.Enemies[0];
            Assert.That(enemy.Kind, Is.EqualTo(EnemyKind.Regular));
            Assert.That(arena.IsSpawnPosition(enemy.Position), Is.True);
            Assert.That(enemy.Position.Y, Is.EqualTo(arena.Bounds.GroundY));
        }

        [Test]
        public void Tick_TwoSpawnsInOneRun_AreSpacedByTheSumOfRadii()
        {
            ArenaDefinition arena = CreateArena();
            Transform player = CreatePlayer();
            Camera camera = CreateCamera(new Vector3(0f, 10f, 0f));
            UnityEnemySpawnPlacementResolver resolver = new(arena, camera, player);
            PlayModeTickHarness harness = new(
                new EmptyEnemyColliderRegistry(),
                spawnPlacementResolver: resolver,
                spawnPacingPolicy: new AlwaysSpawnPacingPolicy(),
                enemyMovementResolver: new StationaryMovementResolver());

            harness.TickIdle();
            ArenaRunTickResult result = harness.TickIdle();

            Assert.That(result.Snapshot.Enemies, Has.Count.EqualTo(2));

            EnemySnapshot first = result.Snapshot.Enemies[0];
            EnemySnapshot second = result.Snapshot.Enemies[1];
            float minimum = first.CollisionRadius.Value.Value + second.CollisionRadius.Value.Value;

            Assert.That(
                first.Position.GroundDistanceTo(second.Position),
                Is.GreaterThanOrEqualTo(minimum - 1e-3f));
        }

        [Test]
        public void Tick_SpawnAvoidsAMovedEnemyCurrentPosition()
        {
            ArenaDefinition arena = CreateArena();
            Transform player = CreatePlayer();
            Camera camera = CreateCamera(new Vector3(0f, 10f, 0f));
            UnityEnemySpawnPlacementResolver resolver = new(arena, camera, player);
            PlayModeTickHarness harness = new(
                new EmptyEnemyColliderRegistry(),
                spawnPlacementResolver: resolver,
                spawnPacingPolicy: new AlwaysSpawnPacingPolicy(),
                enemyMovementResolver: new PursuitMovementResolver());

            ArenaRunTickResult firstTick = harness.TickIdle();
            Assert.That(firstTick.Snapshot.Enemies, Has.Count.EqualTo(1));
            EnemySnapshot before = firstTick.Snapshot.Enemies[0];

            ArenaRunTickResult secondTick = harness.TickIdle();
            Assert.That(secondTick.Snapshot.Enemies, Has.Count.EqualTo(2));

            EnemySnapshot moved = FindEnemy(secondTick.Snapshot, before.EnemyId);
            EnemySnapshot spawned = FindOther(secondTick.Snapshot, before.EnemyId);

            Assert.That(
                moved.Position.GroundDistanceTo(before.Position),
                Is.GreaterThan(0f),
                "the first enemy must have moved before the second spawn");

            float minimum = moved.CollisionRadius.Value.Value + spawned.CollisionRadius.Value.Value;

            Assert.That(
                spawned.Position.GroundDistanceTo(moved.Position),
                Is.GreaterThanOrEqualTo(minimum - 1e-3f));
        }

        private static ArenaDefinition CreateArena()
        {
            return new ArenaDefinition(
                new ArenaBounds(-20f, 20f, -20f, 20f, 0f),
                Distance.FromValue(5f));
        }

        private static EnemySnapshot FindEnemy(ArenaRunSnapshot snapshot, EnemyId enemyId)
        {
            for (int index = 0; index < snapshot.Enemies.Count; index++)
            {
                if (snapshot.Enemies[index].EnemyId == enemyId)
                {
                    return snapshot.Enemies[index];
                }
            }

            throw new System.InvalidOperationException("Enemy is not present: " + enemyId);
        }

        private static EnemySnapshot FindOther(ArenaRunSnapshot snapshot, EnemyId enemyId)
        {
            for (int index = 0; index < snapshot.Enemies.Count; index++)
            {
                if (snapshot.Enemies[index].EnemyId != enemyId)
                {
                    return snapshot.Enemies[index];
                }
            }

            throw new System.InvalidOperationException("No other enemy is present.");
        }

        private Transform CreatePlayer()
        {
            GameObject gameObject = new("Test Player");
            _objects.Add(gameObject);
            gameObject.transform.SetPositionAndRotation(Vector3.zero, Quaternion.identity);

            return gameObject.transform;
        }

        private Camera CreateCamera(Vector3 position)
        {
            GameObject gameObject = new("Test Camera");
            _objects.Add(gameObject);
            gameObject.transform.position = position;
            gameObject.transform.LookAt(Vector3.zero);

            Camera camera = gameObject.AddComponent<Camera>();
            camera.fieldOfView = 60f;
            camera.aspect = 1f;

            return camera;
        }

        private sealed class StationaryMovementResolver : IEnemyMovementResolver
        {
            public IReadOnlyList<EnemyMovementBatchResolutionEntry> Resolve(
                EnemyMovementBatchRequest request,
                ReadOnlySpan<EnemyFootprint> occupied)
            {
                List<EnemyMovementBatchResolutionEntry> entries = new(request.Intents.Count);

                for (int index = 0; index < request.Intents.Count; index++)
                {
                    entries.Add(new EnemyMovementBatchResolutionEntry(
                        request.Intents[index].EnemyId,
                        request.Intents[index].Intent.From));
                }

                return entries;
            }
        }

        private sealed class PursuitMovementResolver : IEnemyMovementResolver
        {
            public IReadOnlyList<EnemyMovementBatchResolutionEntry> Resolve(
                EnemyMovementBatchRequest request,
                ReadOnlySpan<EnemyFootprint> occupied)
            {
                List<EnemyMovementBatchResolutionEntry> entries = new(request.Intents.Count);

                for (int index = 0; index < request.Intents.Count; index++)
                {
                    entries.Add(new EnemyMovementBatchResolutionEntry(
                        request.Intents[index].EnemyId,
                        request.Intents[index].Intent.RequestedPosition));
                }

                return entries;
            }
        }

        private sealed class EmptyEnemyColliderRegistry : IEnemyColliderRegistry
        {
            public bool TryGetEnemyId(Collider collider, out EnemyId enemyId)
            {
                enemyId = EnemyId.None;

                return false;
            }
        }

        private sealed class AlwaysSpawnPacingPolicy : IEnemySpawnPacingPolicy
        {
            public bool ShouldRequestSpawn(GameTimePoint now)
            {
                return true;
            }

            public void RecordSuccessfulSpawn(GameTimePoint now)
            {
            }

            public void Reset()
            {
            }
        }
    }
}
