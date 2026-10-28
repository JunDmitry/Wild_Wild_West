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
    public sealed class EquippedWeaponIndicatorTests
    {
        private const string FirearmAnnouncement = "Equipped weapon: Firearm";
        private const string MeleeAnnouncement = "Equipped weapon: Melee Weapon";

        private readonly List<GameObject> _objects = new();
        private EquippedWeaponPresenter _presenter;
        private HudView _hudView;
        private ArenaRunAnnouncer _announcer;
        private TextMeshProUGUI _textMesh;

        [SetUp]
        public void SetUp()
        {
            _objects.Clear();

            GameObject root = Track(new GameObject("Hud"));
            _textMesh = root.AddComponent<TextMeshProUGUI>();
            _hudView = root.AddComponent<HudView>();
            _announcer = root.AddComponent<ArenaRunAnnouncer>();
            _presenter = root.AddComponent<EquippedWeaponPresenter>();
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
        public void Present_NewRun_ShowsFirearmAndAnnouncesIt()
        {
            PlayModeTickHarness harness = CreateHarness();
            ArenaRunTickResult result = harness.TickIdle();

            _presenter.Present(result.Hud);

            Assert.That(result.Hud.EquippedWeaponText, Is.EqualTo("Firearm"));
            Assert.That(_hudView.EquippedWeaponLabel, Is.EqualTo("Equipped weapon"));
            Assert.That(_hudView.EquippedWeaponText, Is.EqualTo("Firearm"));
            Assert.That(_textMesh.text, Is.EqualTo(FirearmAnnouncement));
            Assert.That(_announcer.AnnouncementCount, Is.EqualTo(1));
            Assert.That(_announcer.LastAnnouncement, Is.EqualTo(FirearmAnnouncement));
        }

        [Test]
        public void Present_SuccessfulSwitch_UpdatesTextAndAnnouncesEachChange()
        {
            PlayModeTickHarness harness = CreateHarness();
            _presenter.Present(harness.TickIdle().Hud);

            _presenter.Present(harness.Tick(false, Direction3D.Forward, switchWeaponPressed: true).Hud);

            Assert.That(_hudView.EquippedWeaponText, Is.EqualTo("Melee Weapon"));
            Assert.That(_textMesh.text, Is.EqualTo(MeleeAnnouncement));
            Assert.That(_announcer.LastAnnouncement, Is.EqualTo(MeleeAnnouncement));

            _presenter.Present(harness.Tick(false, Direction3D.Forward, switchWeaponPressed: true).Hud);

            Assert.That(_hudView.EquippedWeaponText, Is.EqualTo("Firearm"));
            Assert.That(_textMesh.text, Is.EqualTo(FirearmAnnouncement));
            Assert.That(_announcer.AnnouncementCount, Is.EqualTo(3));
        }

        [Test]
        public void Present_SameWeaponTwice_AnnouncesOnce()
        {
            PlayModeTickHarness harness = CreateHarness();

            _presenter.Present(harness.TickIdle().Hud);
            _presenter.Present(harness.TickIdle().Hud);

            Assert.That(_announcer.AnnouncementCount, Is.EqualTo(1));
        }

        [Test]
        public void Present_SwitchWhileAttackPending_KeepsTextAndDoesNotAnnounce()
        {
            PlayModeTickHarness harness = CreateHarness();
            _presenter.Present(harness.TickIdle().Hud);
            _presenter.Present(harness.Tick(false, Direction3D.Forward, switchWeaponPressed: true).Hud);
            int announcementsBefore = _announcer.AnnouncementCount;

            harness.Tick(true, Direction3D.Forward);
            ArenaRunTickResult refused = harness.Tick(false, Direction3D.Forward, switchWeaponPressed: true);

            _presenter.Present(refused.Hud);

            Assert.That(refused.Hud.EquippedWeaponText, Is.EqualTo("Melee Weapon"));
            Assert.That(_hudView.EquippedWeaponText, Is.EqualTo("Melee Weapon"));
            Assert.That(_announcer.AnnouncementCount, Is.EqualTo(announcementsBefore));
        }

        [Test]
        public void Present_SwitchWhileDefeated_KeepsTextAndDoesNotAnnounce()
        {
            PlayModeTickHarness harness = CreateHarness(new Position3D(19f, 0f, 0f));
            harness.Tick(false, Direction3D.Forward, switchWeaponPressed: true);
            harness.SeedEnemy(1UL, new Position3D(20.5f, 0f, 0f));

            ArenaRunTickResult defeated = TickUntilStatus(harness, ArenaRunStatus.Defeat);

            _presenter.Present(defeated.Hud);
            int announcementsBefore = _announcer.AnnouncementCount;

            ArenaRunTickResult refused = harness.Tick(false, Direction3D.Forward, switchWeaponPressed: true);

            _presenter.Present(refused.Hud);

            Assert.That(defeated.Hud.EquippedWeaponText, Is.EqualTo("Melee Weapon"));
            Assert.That(refused.Hud.EquippedWeaponText, Is.EqualTo("Melee Weapon"));
            Assert.That(_hudView.EquippedWeaponText, Is.EqualTo("Melee Weapon"));
            Assert.That(_announcer.AnnouncementCount, Is.EqualTo(announcementsBefore));
        }

        [Test]
        public void Present_AfterDefeatRestart_ShowsFirearmAgain()
        {
            PlayModeTickHarness harness = CreateHarness(
                new Position3D(19f, 0f, 0f),
                new GameDuration(1d));
            harness.Tick(false, Direction3D.Forward, switchWeaponPressed: true);
            harness.SeedEnemy(1UL, new Position3D(20.5f, 0f, 0f));

            ArenaRunTickResult defeated = TickUntilStatus(harness, ArenaRunStatus.Defeat);

            _presenter.Present(defeated.Hud);
            int announcementsBefore = _announcer.AnnouncementCount;

            ArenaRunTickResult restarted = TickUntilRestart(harness, defeated.Snapshot.ArenaRunId);

            Assert.That(restarted.Snapshot.Status, Is.EqualTo(ArenaRunStatus.Playing));
            Assert.That(restarted.Hud.EquippedWeaponText, Is.EqualTo("Firearm"));

            _presenter.Present(restarted.Hud);

            Assert.That(_hudView.EquippedWeaponText, Is.EqualTo("Firearm"));
            Assert.That(_announcer.AnnouncementCount, Is.EqualTo(announcementsBefore + 1));
            Assert.That(_announcer.LastAnnouncement, Is.EqualTo(FirearmAnnouncement));
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

        private static ArenaRunTickResult TickUntilStatus(
            PlayModeTickHarness harness,
            ArenaRunStatus status)
        {
            for (int index = 0; index < 2000; index++)
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
            for (int index = 0; index < 500; index++)
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
