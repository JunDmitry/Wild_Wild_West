using System;
using System.Collections.Generic;
using Game.Arena.Application.Ticks;
using Game.Arena.Domain.Aggregates.ArenaRun;
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
    public sealed class HealthIndicatorTests
    {
        private readonly List<GameObject> _objects = new();
        private HealthIndicatorPresenter _presenter;
        private HudView _hudView;
        private ArenaRunAnnouncer _announcer;
        private TextMeshProUGUI _textMesh;
        private Transform _fill;

        [SetUp]
        public void SetUp()
        {
            _objects.Clear();

            GameObject root = Track(new GameObject("Hud"));
            _hudView = root.AddComponent<HudView>();
            _announcer = root.AddComponent<ArenaRunAnnouncer>();
            _presenter = root.AddComponent<HealthIndicatorPresenter>();

            GameObject healthText = Track(new GameObject("HealthText"));
            healthText.transform.SetParent(root.transform, false);
            _textMesh = healthText.AddComponent<TextMeshProUGUI>();

            GameObject fill = Track(new GameObject("HealthFill"));
            fill.transform.SetParent(root.transform, false);
            fill.transform.localScale = Vector3.one;
            _fill = fill.transform;

            _hudView.HealthTextMesh = _textMesh;
            _hudView.HealthFill = _fill;
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
        }

        [Test]
        public void Present_NewRun_ShowsFullHealthAndAnnouncesIt()
        {
            PlayModeTickHarness harness = CreateHarness();
            ArenaRunTickResult result = harness.TickIdle();

            _presenter.Present(result.Hud);

            string expectedText = result.Hud.HealthCurrent + " / " + result.Hud.HealthMaximum;

            Assert.That(result.Hud.HealthCurrent, Is.EqualTo(result.Hud.HealthMaximum));
            Assert.That(result.Hud.IsDefeated, Is.False);
            Assert.That(_hudView.HealthLabel, Is.EqualTo("Health"));
            Assert.That(_hudView.HealthCurrent, Is.EqualTo(result.Hud.HealthCurrent));
            Assert.That(_hudView.HealthMaximum, Is.EqualTo(result.Hud.HealthMaximum));
            Assert.That(_hudView.HealthText, Is.EqualTo(expectedText));
            Assert.That(_hudView.HealthFraction, Is.EqualTo(1f).Within(0.001f));
            Assert.That(_hudView.IsDefeated, Is.False);
            Assert.That(_fill.localScale.x, Is.EqualTo(1f).Within(0.001f));
            Assert.That(_textMesh.text, Is.EqualTo("Health: " + expectedText));
            Assert.That(_announcer.AnnouncementCount, Is.EqualTo(1));
            Assert.That(_announcer.LastAnnouncement, Is.EqualTo("Health: " + expectedText));
        }

        [Test]
        public void Present_LandedEnemyAttack_UpdatesReadoutAndFillInTheSameTick()
        {
            PlayModeTickHarness harness = CreateHarness(new Position3D(19f, 0f, 0f));
            harness.SeedEnemy(1UL, new Position3D(20.5f, 0f, 0f));

            _presenter.Present(harness.TickIdle().Hud);
            int announcementsBefore = _announcer.AnnouncementCount;

            for (int index = 0; index < 2000; index++)
            {
                ArenaRunTickResult result = harness.TickIdle();

                _presenter.Present(result.Hud);

                if (result.Hud.HealthCurrent >= result.Hud.HealthMaximum)
                {
                    continue;
                }

                Assert.That(_hudView.HealthCurrent, Is.EqualTo(result.Hud.HealthCurrent));
                Assert.That(_hudView.HealthText, Is.EqualTo(result.Hud.HealthText));
                Assert.That(_hudView.HealthFraction, Is.EqualTo(result.Hud.HealthFraction).Within(0.001f));
                Assert.That(_fill.localScale.x, Is.EqualTo(result.Hud.HealthFraction).Within(0.001f));
                Assert.That(_announcer.AnnouncementCount, Is.EqualTo(announcementsBefore + 1));
                Assert.That(
                    _announcer.LastAnnouncement,
                    Is.EqualTo("Health: " + result.Hud.HealthText));
                return;
            }

            Assert.Fail("No enemy attack landed.");
        }

        [Test]
        public void Present_LethalDamage_ShowsZeroEmptyFillAndAnnouncesDefeatOnce()
        {
            PlayModeTickHarness harness = CreateHarness(new Position3D(19f, 0f, 0f));
            harness.SeedEnemy(1UL, new Position3D(20.5f, 0f, 0f));

            ArenaRunTickResult defeated = null;

            for (int index = 0; index < 2000; index++)
            {
                ArenaRunTickResult result = harness.TickIdle();

                _presenter.Present(result.Hud);

                if (result.Snapshot.Status == ArenaRunStatus.Defeat)
                {
                    defeated = result;
                    break;
                }
            }

            Assert.That(defeated, Is.Not.Null);

            int announcementsAtDefeat = _announcer.AnnouncementCount;

            Assert.That(defeated.Hud.HealthCurrent, Is.EqualTo(0));
            Assert.That(defeated.Hud.IsDefeated, Is.True);
            Assert.That(_hudView.HealthCurrent, Is.EqualTo(0));
            Assert.That(_hudView.HealthText, Is.EqualTo("0 / " + defeated.Hud.HealthMaximum));
            Assert.That(_hudView.HealthFraction, Is.Zero.Within(0.001f));
            Assert.That(_hudView.IsDefeated, Is.True);
            Assert.That(_fill.localScale.x, Is.Zero.Within(0.001f));
            Assert.That(
                _announcer.LastAnnouncement,
                Is.EqualTo("Health: 0 / " + defeated.Hud.HealthMaximum + ArenaRunAnnouncer.PlayerDefeatedSuffix));

            for (int index = 0; index < 5; index++)
            {
                ArenaRunTickResult terminal = harness.Tick(true, Direction3D.Forward);

                _presenter.Present(terminal.Hud);

                Assert.That(_announcer.AnnouncementCount, Is.EqualTo(announcementsAtDefeat));
                Assert.That(_fill.localScale.x, Is.Zero.Within(0.001f));
            }
        }

        [Test]
        public void Present_AfterDefeatRestart_ShowsFullHealthAgain()
        {
            PlayModeTickHarness harness = CreateHarness(
                new Position3D(19f, 0f, 0f),
                new GameDuration(1d));
            harness.SeedEnemy(1UL, new Position3D(20.5f, 0f, 0f));

            ArenaRunTickResult defeated = PresentUntilStatus(harness, ArenaRunStatus.Defeat);
            int announcementsAtDefeat = _announcer.AnnouncementCount;

            ArenaRunTickResult restarted = PresentUntilRestart(harness, defeated.Snapshot.ArenaRunId);

            Assert.That(restarted.Snapshot.Status, Is.EqualTo(ArenaRunStatus.Playing));
            Assert.That(restarted.Hud.HealthCurrent, Is.EqualTo(restarted.Hud.HealthMaximum));
            Assert.That(_hudView.HealthText, Is.EqualTo(restarted.Hud.HealthText));
            Assert.That(_hudView.HealthFraction, Is.EqualTo(1f).Within(0.001f));
            Assert.That(_hudView.IsDefeated, Is.False);
            Assert.That(_fill.localScale.x, Is.EqualTo(1f).Within(0.001f));
            Assert.That(_announcer.AnnouncementCount, Is.EqualTo(announcementsAtDefeat + 1));
            Assert.That(
                _announcer.LastAnnouncement,
                Is.EqualTo("Health: " + restarted.Hud.HealthText));
        }

        [Test]
        public void Present_SameHealthTwice_AnnouncesOnce()
        {
            PlayModeTickHarness harness = CreateHarness();

            _presenter.Present(harness.TickIdle().Hud);
            _presenter.Present(harness.TickIdle().Hud);

            Assert.That(_announcer.AnnouncementCount, Is.EqualTo(1));
        }

        [Test]
        public void Present_NullHud_Throws()
        {
            Assert.Throws<ArgumentNullException>(() => _presenter.Present(null));
        }

        private static PlayModeTickHarness CreateHarness(
            Position3D? playerStart = null,
            GameDuration? restartDelay = null)
        {
            return new PlayModeTickHarness(
                new EmptyEnemyColliderRegistry(),
                playerStart: playerStart,
                restartDelay: restartDelay);
        }

        private ArenaRunTickResult PresentUntilStatus(
            PlayModeTickHarness harness,
            ArenaRunStatus status)
        {
            for (int index = 0; index < 2000; index++)
            {
                ArenaRunTickResult result = harness.TickIdle();

                _presenter.Present(result.Hud);

                if (result.Snapshot.Status == status)
                {
                    return result;
                }
            }

            Assert.Fail("Run did not reach " + status + ".");
            return null;
        }

        private ArenaRunTickResult PresentUntilRestart(
            PlayModeTickHarness harness,
            ArenaRunId previousRunId)
        {
            for (int index = 0; index < 500; index++)
            {
                ArenaRunTickResult result = harness.TickIdle();

                _presenter.Present(result.Hud);

                if (result.Snapshot.Status == ArenaRunStatus.Playing
                    && result.Snapshot.ArenaRunId != previousRunId)
                {
                    return result;
                }
            }

            Assert.Fail("Run did not restart.");
            return null;
        }

        private GameObject Track(GameObject gameObject)
        {
            _objects.Add(gameObject);
            return gameObject;
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
