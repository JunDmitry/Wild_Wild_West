using System;
using System.Collections.Generic;
using Game.Arena.Application.Input;
using Game.Arena.Application.Ports;
using Game.Arena.Application.Ticks;
using Game.Arena.Domain.Aggregates.ArenaRun;
using Game.Arena.Domain.Configuration;
using Game.Arena.Domain.Geometry;
using Game.Arena.Domain.Identity;
using Game.Arena.Domain.Time;
using Game.Arena.Infrastructure.Scenes;
using Game.Arena.Infrastructure.Targeting;
using NUnit.Framework;
using UnityEngine;

namespace Game.Arena.PlayMode.Tests
{
    [TestFixture]
    public sealed class ArenaSceneReloadTests
    {
        private const int MaxTicks = 3000;
        private static readonly GameDuration s_restartDelay = new(0.3d);
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
        public void DefeatingTheRun_TriggersExactlyOneReload()
        {
            RecordingArenaSceneReloader reloader = new();
            PlayModeTickHarness harness = CreateHarness(reloader);
            SeedEnemy(harness);

            ArenaRunTickResult defeated = TickUntilStatus(harness, ArenaRunStatus.Defeat);

            Assert.That(defeated.Snapshot.Status, Is.EqualTo(ArenaRunStatus.Defeat));
            Assert.That(reloader.ReloadCount, Is.EqualTo(0));

            ArenaRunTickResult restarted = TickUntilRestart(harness, defeated.Snapshot.ArenaRunId);

            Assert.That(restarted.Snapshot.Status, Is.EqualTo(ArenaRunStatus.Playing));
            Assert.That(reloader.ReloadCount, Is.EqualTo(1));
        }

        [Test]
        public void TickAfterTheRestart_DoesNotReloadAgain()
        {
            RecordingArenaSceneReloader reloader = new();
            PlayModeTickHarness harness = CreateHarness(reloader);
            SeedEnemy(harness);

            ArenaRunTickResult defeated = TickUntilStatus(harness, ArenaRunStatus.Defeat);
            TickUntilRestart(harness, defeated.Snapshot.ArenaRunId);

            Assert.That(reloader.ReloadCount, Is.EqualTo(1));

            for (int index = 0; index < 20; index++)
            {
                harness.TickIdle();
            }

            Assert.That(reloader.ReloadCount, Is.EqualTo(1));
        }

        [Test]
        public void RunThatNeverReachesDefeat_TriggersNoReload()
        {
            RecordingArenaSceneReloader reloader = new();
            PlayModeTickHarness harness = CreateHarness(reloader);

            for (int index = 0; index < 100; index++)
            {
                ArenaRunTickResult result = harness.TickIdle();

                Assert.That(result.Snapshot.Status, Is.EqualTo(ArenaRunStatus.Playing));
            }

            Assert.That(reloader.ReloadCount, Is.EqualTo(0));
        }

        [Test]
        public void WinningTheRun_RequestsNoReload()
        {
            RecordingArenaSceneReloader reloader = new();
            TestEnemyColliderRegistry registry = new();
            FixedInputSource input = new(CreateInput(false));
            PlayModeTickHarness harness = new(
                registry,
                waveCatalog: CreateBossOnlyCatalog(),
                playerInputSource: input,
                restartDelay: s_restartDelay,
                sceneReloader: reloader);

            SeedBoss(harness, registry, 1UL, new Vector3(0f, 0f, 21f));

            input.Input = CreateInput(true);

            ArenaRunTickResult victory = TickFrameUntilStatus(harness, ArenaRunStatus.Victory);

            Assert.That(victory.Snapshot.Status, Is.EqualTo(ArenaRunStatus.Victory));
            Assert.That(reloader.ReloadCount, Is.EqualTo(0));

            for (int index = 0; index < 20; index++)
            {
                ArenaRunTickResult later = harness.TickFrame();

                Assert.That(later.Snapshot.Status, Is.EqualTo(ArenaRunStatus.Victory));
            }

            Assert.That(reloader.ReloadCount, Is.EqualTo(0));
        }

        [Test]
        public void UnityArenaSceneReloader_RejectsInvalidSceneNames()
        {
            Assert.Throws<ArgumentNullException>(() => new UnityArenaSceneReloader(null));
            Assert.Throws<ArgumentException>(() => new UnityArenaSceneReloader(string.Empty));
            Assert.Throws<ArgumentException>(() => new UnityArenaSceneReloader("   "));
        }

        private static PlayModeTickHarness CreateHarness(IArenaSceneReloader reloader)
        {
            return new PlayModeTickHarness(
                new EmptyEnemyColliderRegistry(),
                playerStart: new Position3D(19f, 0f, 0f),
                restartDelay: s_restartDelay,
                sceneReloader: reloader);
        }

        private static void SeedEnemy(PlayModeTickHarness harness)
        {
            harness.SeedEnemy(1UL, new Position3D(20.5f, 0f, 0f));
        }

        private static ArenaRunTickResult TickUntilStatus(
            PlayModeTickHarness harness,
            ArenaRunStatus status)
        {
            for (int index = 0; index < MaxTicks; index++)
            {
                ArenaRunTickResult result = harness.TickIdle();

                if (result.Snapshot.Status == status)
                {
                    return result;
                }
            }

            Assert.Fail("Run did not reach " + status + ".");
            return null;
        }

        private static ArenaRunTickResult TickUntilRestart(
            PlayModeTickHarness harness,
            ArenaRunId previousRunId)
        {
            for (int index = 0; index < MaxTicks; index++)
            {
                ArenaRunTickResult result = harness.TickIdle();

                if (result.Snapshot.Status == ArenaRunStatus.Playing
                    && result.Snapshot.ArenaRunId != previousRunId)
                {
                    return result;
                }
            }

            Assert.Fail("Run did not restart.");
            return null;
        }

        private static ArenaRunTickResult TickFrameUntilStatus(
            PlayModeTickHarness harness,
            ArenaRunStatus status)
        {
            for (int index = 0; index < MaxTicks; index++)
            {
                ArenaRunTickResult result = harness.TickFrame();

                if (result.Snapshot.Status == status)
                {
                    return result;
                }
            }

            Assert.Fail("Run did not reach " + status + ".");
            return null;
        }

        private void SeedBoss(
            PlayModeTickHarness harness,
            TestEnemyColliderRegistry registry,
            ulong value,
            Vector3 position)
        {
            EnemyId enemyId = EnemyId.FromValue(value);

            GameObject enemy = Track(new GameObject("Boss" + value));
            enemy.transform.position = position;

            BoxCollider collider = enemy.AddComponent<BoxCollider>();
            collider.size = new Vector3(1f, 2f, 1f);

            Physics.SyncTransforms();

            registry.Register(collider, enemyId);
            harness.SeedEnemy(value, new Position3D(position.x, position.y, position.z));
        }

        private static PlayerFrameInput CreateInput(bool attackPressed)
        {
            return new PlayerFrameInput(
                MovementAxes.Zero,
                Direction3D.Forward,
                attackPressed,
                false);
        }

        private static WaveCatalog CreateBossOnlyCatalog()
        {
            return new WaveCatalog(
                new[]
                {
                    new WaveDefinition(WaveNumber.First, 0),
                });
        }

        private GameObject Track(GameObject gameObject)
        {
            _objects.Add(gameObject);
            return gameObject;
        }

        private sealed class RecordingArenaSceneReloader : IArenaSceneReloader
        {
            public int ReloadCount { get; private set; }

            public void ReloadArenaScene()
            {
                ReloadCount++;
            }
        }

        private sealed class TestEnemyColliderRegistry : IEnemyColliderRegistry
        {
            private readonly Dictionary<Collider, EnemyId> _enemyIds = new();

            public void Register(Collider collider, EnemyId enemyId)
            {
                _enemyIds.Add(collider, enemyId);
            }

            public bool TryGetEnemyId(Collider collider, out EnemyId enemyId)
            {
                if (collider == null)
                {
                    enemyId = EnemyId.None;
                    return false;
                }

                return _enemyIds.TryGetValue(collider, out enemyId);
            }
        }

        private sealed class FixedInputSource : IPlayerInputSource
        {
            public FixedInputSource(PlayerFrameInput input)
            {
                Input = input;
            }

            public PlayerFrameInput Input { get; set; }

            public PlayerFrameInput Read()
            {
                return Input;
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
    }
}
