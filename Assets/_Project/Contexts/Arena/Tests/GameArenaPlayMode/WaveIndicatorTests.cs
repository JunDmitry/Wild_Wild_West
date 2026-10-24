using System;
using System.Collections.Generic;
using Game.Arena.Application.ReadModels;
using Game.Arena.Application.Ticks;
using Game.Arena.Domain.Aggregates.ArenaRun;
using Game.Arena.Domain.Configuration;
using Game.Arena.Domain.Geometry;
using Game.Arena.Domain.Identity;
using Game.Arena.Domain.Time;
using Game.Arena.Infrastructure.Targeting;
using Game.Arena.Presentation.Accessibility;
using Game.Arena.Presentation.Hud;
using NUnit.Framework;
using TMPro;
using UnityEngine;

namespace Game.Arena.PlayMode.Tests
{
    [TestFixture]
    public sealed class WaveIndicatorTests
    {
        private const int MaxTicks = 2000;

        private readonly List<GameObject> _objects = new();
        private readonly Dictionary<EnemyId, GameObject> _enemies = new();
        private WaveIndicatorPresenter _presenter;
        private HudView _hudView;
        private ArenaRunAnnouncer _announcer;
        private TextMeshPro _textMesh;
        private TestEnemyColliderRegistry _registry;
        private PlayModeTickHarness _harness;

        [SetUp]
        public void SetUp()
        {
            _objects.Clear();
            _enemies.Clear();
            _registry = new TestEnemyColliderRegistry();

            GameObject root = Track(new GameObject("Hud"));
            _hudView = root.AddComponent<HudView>();
            _announcer = root.AddComponent<ArenaRunAnnouncer>();
            _presenter = root.AddComponent<WaveIndicatorPresenter>();

            GameObject waveText = Track(new GameObject("WaveText"));
            waveText.transform.SetParent(root.transform, false);
            _textMesh = waveText.AddComponent<TextMeshPro>();

            _hudView.WaveTextMesh = _textMesh;
            _presenter.HudView = _hudView;
            _presenter.Announcer = _announcer;
        }

        [TearDown]
        public void TearDown()
        {
            for (int index = 0; index < _objects.Count; index++)
            {
                UnityEngine.Object.DestroyImmediate(_objects[index]);
            }

            _objects.Clear();
            _enemies.Clear();
        }

        [Test]
        public void Present_RunStart_ShowsWaveOneOfThreeAndAnnouncesItOnce()
        {
            CreateHarness();
            ArenaRunTickResult result = _harness.TickIdle();

            Present(result);

            Assert.That(result.Hud.WaveText, Is.EqualTo("Wave 1 of 3"));
            Assert.That(_hudView.WaveLabel, Is.EqualTo(WaveIndicatorPresenter.WaveLabel));
            Assert.That(_hudView.WaveText, Is.EqualTo("Wave 1 of 3"));
            Assert.That(_textMesh.text, Is.EqualTo("Wave 1 of 3"));
            Assert.That(_announcer.AnnouncementCount, Is.EqualTo(1));
            Assert.That(_announcer.LastAnnouncement, Is.EqualTo("Wave 1 of 3 begins"));

            Present(result);

            Assert.That(_announcer.AnnouncementCount, Is.EqualTo(1));
        }

        [Test]
        public void Present_WaveCleared_ShowsWaveTwoOfThreeAndAnnouncesOnce()
        {
            CreateHarness();
            Present(_harness.TickIdle());
            Assert.That(_announcer.AnnouncementCount, Is.EqualTo(1));

            EnemyId regularId = SeedEnemy(1UL, new Vector3(0f, 0f, 21f));
            AttackUntilEnemyRemoved(regularId);
            RemoveEnemy(regularId);

            SeedEnemy(2UL, new Vector3(0f, 0f, 21f));
            ArenaRunTickResult advanced = AttackUntilWave(WaveNumber.FromValue(2));

            Assert.That(advanced.Hud.WaveText, Is.EqualTo("Wave 2 of 3"));
            Assert.That(_hudView.WaveText, Is.EqualTo("Wave 2 of 3"));
            Assert.That(_textMesh.text, Is.EqualTo("Wave 2 of 3"));
            Assert.That(_announcer.AnnouncementCount, Is.EqualTo(2));
            Assert.That(_announcer.LastAnnouncement, Is.EqualTo("Wave 2 of 3 begins"));

            Present(advanced);

            Assert.That(_announcer.AnnouncementCount, Is.EqualTo(2));
        }

        [Test]
        public void Present_DefeatedRun_KeepsTheReachedWaveAndDoesNotAnnounce()
        {
            CreateHarness(new Position3D(19f, 0f, 0f));
            SeedEnemy(1UL, new Vector3(20.5f, 0f, 0f));

            ArenaRunTickResult defeated = PresentUntilStatus(ArenaRunStatus.Defeat);
            int announcementsAtDefeat = _announcer.AnnouncementCount;

            Assert.That(defeated.Hud.WaveText, Is.EqualTo("Wave 1 of 3"));
            Assert.That(_hudView.WaveText, Is.EqualTo("Wave 1 of 3"));

            for (int index = 0; index < 5; index++)
            {
                ArenaRunTickResult terminal = _harness.Tick(true, Direction3D.Forward);

                Present(terminal);

                Assert.That(_announcer.AnnouncementCount, Is.EqualTo(announcementsAtDefeat));
                Assert.That(_hudView.WaveText, Is.EqualTo("Wave 1 of 3"));
            }
        }

        [Test]
        public void Present_RestartedRun_AnnouncesWaveOneOnce()
        {
            CreateHarness(new Position3D(19f, 0f, 0f), new GameDuration(1d));
            SeedEnemy(1UL, new Vector3(20.5f, 0f, 0f));

            ArenaRunTickResult defeated = PresentUntilStatus(ArenaRunStatus.Defeat);
            int announcementsAtDefeat = _announcer.AnnouncementCount;

            ArenaRunTickResult restarted = PresentUntilRestart(defeated.Snapshot.ArenaRunId);

            Assert.That(restarted.Snapshot.Status, Is.EqualTo(ArenaRunStatus.Playing));
            Assert.That(restarted.Hud.WaveText, Is.EqualTo("Wave 1 of 3"));
            Assert.That(_hudView.WaveText, Is.EqualTo("Wave 1 of 3"));
            Assert.That(_announcer.AnnouncementCount, Is.EqualTo(announcementsAtDefeat + 1));
            Assert.That(_announcer.LastAnnouncement, Is.EqualTo("Wave 1 of 3 begins"));
        }

        [Test]
        public void Present_NullArguments_Throw()
        {
            CreateHarness();
            ArenaRunTickResult result = _harness.TickIdle();

            Assert.Throws<ArgumentNullException>(() => _presenter.Present(null, result.Hud));
            Assert.Throws<ArgumentNullException>(() => _presenter.Present(result.Snapshot, null));
        }

        private static WaveCatalog CreateThreeWaveCatalog()
        {
            return new WaveCatalog(
                new[]
                {
                    new WaveDefinition(WaveNumber.First, 1),
                    new WaveDefinition(WaveNumber.FromValue(2), 1),
                    new WaveDefinition(WaveNumber.FromValue(3), 1),
                });
        }

        private static bool ContainsEnemy(ArenaRunSnapshot snapshot, EnemyId enemyId)
        {
            for (int index = 0; index < snapshot.Enemies.Count; index++)
            {
                if (snapshot.Enemies[index].EnemyId == enemyId)
                {
                    return true;
                }
            }

            return false;
        }

        private void CreateHarness(
            Position3D? playerStart = null,
            GameDuration? restartDelay = null)
        {
            _harness = new PlayModeTickHarness(
                _registry,
                playerStart: playerStart,
                restartDelay: restartDelay,
                waveCatalog: CreateThreeWaveCatalog());
        }

        private EnemyId SeedEnemy(ulong value, Vector3 position)
        {
            EnemyId enemyId = EnemyId.FromValue(value);
            GameObject enemy = CreateCollider("Enemy" + value, position);

            _registry.Register(enemy.GetComponent<Collider>(), enemyId);
            _enemies[enemyId] = enemy;

            _harness.SeedEnemy(
                value,
                new Position3D(position.x, position.y, position.z));

            return enemyId;
        }

        private void RemoveEnemy(EnemyId enemyId)
        {
            GameObject enemy = _enemies[enemyId];

            _enemies.Remove(enemyId);
            _objects.Remove(enemy);
            UnityEngine.Object.DestroyImmediate(enemy);
            Physics.SyncTransforms();
        }

        private void AttackUntilEnemyRemoved(EnemyId enemyId)
        {
            for (int index = 0; index < MaxTicks; index++)
            {
                ArenaRunTickResult result = _harness.Tick(true, Direction3D.Forward);

                Present(result);

                if (ContainsEnemy(result.Snapshot, enemyId) == false)
                {
                    return;
                }
            }

            Assert.Fail("Enemy was not defeated: " + enemyId);
        }

        private ArenaRunTickResult AttackUntilWave(WaveNumber waveNumber)
        {
            for (int index = 0; index < MaxTicks; index++)
            {
                ArenaRunTickResult result = _harness.Tick(true, Direction3D.Forward);

                Present(result);

                if (result.Snapshot.Wave.Number == waveNumber)
                {
                    return result;
                }
            }

            Assert.Fail("Wave did not begin: " + waveNumber.Value);
            return null;
        }

        private ArenaRunTickResult PresentUntilStatus(ArenaRunStatus status)
        {
            for (int index = 0; index < MaxTicks; index++)
            {
                ArenaRunTickResult result = _harness.TickIdle();

                Present(result);

                if (result.Snapshot.Status == status)
                {
                    return result;
                }
            }

            Assert.Fail("Run did not reach " + status + ".");
            return null;
        }

        private ArenaRunTickResult PresentUntilRestart(ArenaRunId previousRunId)
        {
            for (int index = 0; index < 500; index++)
            {
                ArenaRunTickResult result = _harness.TickIdle();

                Present(result);

                if (result.Snapshot.Status == ArenaRunStatus.Playing
                    && result.Snapshot.ArenaRunId != previousRunId)
                {
                    return result;
                }
            }

            Assert.Fail("Run did not restart.");
            return null;
        }

        private void Present(ArenaRunTickResult result)
        {
            _presenter.Present(result.Snapshot, result.Hud);
        }

        private GameObject CreateCollider(string name, Vector3 position)
        {
            GameObject gameObject = Track(new GameObject(name));
            gameObject.transform.position = position;

            BoxCollider collider = gameObject.AddComponent<BoxCollider>();
            collider.size = new Vector3(1f, 2f, 1f);

            Physics.SyncTransforms();

            return gameObject;
        }

        private GameObject Track(GameObject gameObject)
        {
            _objects.Add(gameObject);
            return gameObject;
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
    }
}
