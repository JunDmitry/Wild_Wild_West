using System;
using Game.Arena.Application.Ports;
using Game.Arena.Application.Ticks;
using Game.Arena.Domain.Aggregates.ArenaRun;
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

        private sealed class RecordingArenaSceneReloader : IArenaSceneReloader
        {
            public int ReloadCount { get; private set; }

            public void ReloadArenaScene()
            {
                ReloadCount++;
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
