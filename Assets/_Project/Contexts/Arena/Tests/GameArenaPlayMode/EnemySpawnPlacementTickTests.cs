using System.Collections.Generic;
using Game.Arena.Application.ReadModels;
using Game.Arena.Application.Spawning;
using Game.Arena.Application.Ticks;
using Game.Arena.Domain.Combat;
using Game.Arena.Domain.Configuration;
using Game.Arena.Domain.Geometry;
using Game.Arena.Domain.Identity;
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
