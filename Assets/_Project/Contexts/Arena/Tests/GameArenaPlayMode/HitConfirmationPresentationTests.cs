using System;
using System.Collections.Generic;
using Game.Arena.Application.Notifications;
using Game.Arena.Application.ReadModels;
using Game.Arena.Domain.Aggregates.ArenaRun;
using Game.Arena.Domain.Combat;
using Game.Arena.Domain.Concurrency;
using Game.Arena.Domain.Geometry;
using Game.Arena.Domain.Identity;
using Game.Arena.Domain.Time;
using Game.Arena.Domain.Vitality;
using Game.Arena.Presentation.Accessibility;
using Game.Arena.Presentation.Combat;
using Game.Arena.Presentation.Notifications;
using NUnit.Framework;
using UnityEngine;

namespace Game.Arena.PlayMode.Tests
{
    [TestFixture]
    public sealed class HitConfirmationPresentationTests
    {
        private static readonly ArenaRunId RunId = ArenaRunId.FromValue(1UL);

        private readonly List<GameObject> _objects = new();
        private HitConfirmationPresenter _hitConfirmation;
        private ArenaRunAnnouncer _announcer;
        private ArenaRunNotificationPresenter _presenter;

        [SetUp]
        public void SetUp()
        {
            _objects.Clear();

            GameObject root = Track(new GameObject("Notifications"));
            root.AddComponent<HitConfirmationPresenter>().LifetimeSeconds = 10f;

            _hitConfirmation = root.GetComponent<HitConfirmationPresenter>();
            _announcer = root.AddComponent<ArenaRunAnnouncer>();
            _presenter = root.AddComponent<ArenaRunNotificationPresenter>();
            _presenter.HitConfirmation = _hitConfirmation;
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
        public void Publish_BeforeSnapshot_IsBufferedUntilPresent()
        {
            _presenter.Publish(Damaged(EnemyId.FromValue(1UL), Revision(1UL)));

            Assert.That(_presenter.PendingCount, Is.EqualTo(1));
            Assert.That(_hitConfirmation.ActiveCount, Is.EqualTo(0));
            Assert.That(_announcer.AnnouncementCount, Is.EqualTo(0));

            _presenter.Present(CreateSnapshot(Enemy(1UL, 3f, 4f)));

            Assert.That(_presenter.PendingCount, Is.EqualTo(0));
            Assert.That(_hitConfirmation.IsShownFor(EnemyId.FromValue(1UL)), Is.True);
            Assert.That(_announcer.AnnouncementCount, Is.EqualTo(1));
        }

        [Test]
        public void Publish_ShowsMarkAtDamageEnemyPosition()
        {
            _presenter.Present(CreateSnapshot(Enemy(1UL, 3f, 4f)));

            _presenter.Publish(Damaged(EnemyId.FromValue(1UL), Revision(1UL)));

            Position3D position = _hitConfirmation.PositionOf(EnemyId.FromValue(1UL));

            Assert.That(_hitConfirmation.IsShownFor(EnemyId.FromValue(1UL)), Is.True);
            Assert.That(position.X, Is.EqualTo(3f).Within(1e-4f));
            Assert.That(position.Z, Is.EqualTo(4f).Within(1e-4f));
        }

        [Test]
        public void Publish_TwoEnemiesInOneImpact_ShowsMarkForEach()
        {
            _presenter.Present(CreateSnapshot(Enemy(1UL, 3f, 4f), Enemy(2UL, -5f, 6f)));

            _presenter.Publish(Damaged(EnemyId.FromValue(1UL), Revision(1UL)));
            _presenter.Publish(Damaged(EnemyId.FromValue(2UL), Revision(1UL)));

            Assert.That(_hitConfirmation.ActiveCount, Is.EqualTo(2));
            Assert.That(_hitConfirmation.IsShownFor(EnemyId.FromValue(1UL)), Is.True);
            Assert.That(_hitConfirmation.IsShownFor(EnemyId.FromValue(2UL)), Is.True);
        }

        [Test]
        public void Publish_TwoEnemiesInOneImpact_AnnouncesHitOnce()
        {
            _presenter.Present(CreateSnapshot(Enemy(1UL, 3f, 4f), Enemy(2UL, -5f, 6f)));

            _presenter.Publish(Damaged(EnemyId.FromValue(1UL), Revision(1UL)));
            _presenter.Publish(Damaged(EnemyId.FromValue(2UL), Revision(1UL)));

            Assert.That(_announcer.AnnouncementCount, Is.EqualTo(1));
            Assert.That(_announcer.LastAnnouncement, Is.EqualTo(ArenaRunAnnouncer.HitAnnouncement));
        }

        [Test]
        public void Publish_SameNotificationTwice_AnnouncesOnceAndKeepsOneMark()
        {
            _presenter.Present(CreateSnapshot(Enemy(1UL, 3f, 4f)));

            _presenter.Publish(Damaged(EnemyId.FromValue(1UL), Revision(1UL)));
            _presenter.Publish(Damaged(EnemyId.FromValue(1UL), Revision(1UL)));

            Assert.That(_announcer.AnnouncementCount, Is.EqualTo(1));
            Assert.That(_hitConfirmation.ActiveCount, Is.EqualTo(1));
        }

        [Test]
        public void Publish_NextImpact_AnnouncesAgain()
        {
            _presenter.Present(CreateSnapshot(Enemy(1UL, 3f, 4f)));

            _presenter.Publish(Damaged(EnemyId.FromValue(1UL), Revision(1UL)));
            _presenter.Publish(Damaged(EnemyId.FromValue(1UL), Revision(2UL)));

            Assert.That(_announcer.AnnouncementCount, Is.EqualTo(2));
        }

        [Test]
        public void Publish_EnemyMissingFromSnapshot_ShowsNoMarkButAnnounces()
        {
            _presenter.Present(CreateSnapshot(Enemy(1UL, 3f, 4f)));

            _presenter.Publish(Damaged(EnemyId.FromValue(9UL), Revision(1UL)));

            Assert.That(_hitConfirmation.ActiveCount, Is.EqualTo(0));
            Assert.That(_announcer.AnnouncementCount, Is.EqualTo(1));
        }

        [Test]
        public void Publish_ForeignRun_IsIgnored()
        {
            _presenter.Present(CreateSnapshot(Enemy(1UL, 3f, 4f)));

            ArenaRunEnemyDamagedNotification notification = new(
                ArenaRunId.FromValue(2UL),
                Revision(1UL),
                EnemyId.FromValue(1UL));

            _presenter.Publish(notification);

            Assert.That(_hitConfirmation.ActiveCount, Is.EqualTo(0));
            Assert.That(_announcer.AnnouncementCount, Is.EqualTo(0));
        }

        [Test]
        public void Advance_ExpiresMark()
        {
            _hitConfirmation.LifetimeSeconds = 1f;
            _presenter.Present(CreateSnapshot(Enemy(1UL, 3f, 4f)));
            _presenter.Publish(Damaged(EnemyId.FromValue(1UL), Revision(1UL)));

            _hitConfirmation.Advance(0.5f);

            Assert.That(_hitConfirmation.IsShownFor(EnemyId.FromValue(1UL)), Is.True);

            _hitConfirmation.Advance(0.6f);

            Assert.That(_hitConfirmation.IsShownFor(EnemyId.FromValue(1UL)), Is.False);
            Assert.That(_hitConfirmation.ActiveCount, Is.EqualTo(0));
        }

        [Test]
        public void Publish_NullNotification_Throws()
        {
            Assert.Throws<ArgumentNullException>(() => _presenter.Publish(null));
        }

        private static ArenaRunEnemyDamagedNotification Damaged(
            EnemyId enemyId,
            AggregateRevision revision)
        {
            return new ArenaRunEnemyDamagedNotification(RunId, revision, enemyId);
        }

        private static AggregateRevision Revision(ulong value)
        {
            AggregateRevision revision = AggregateRevision.Initial;

            for (ulong index = 0; index < value; index++)
            {
                revision = revision.Next();
            }

            return revision;
        }

        private static EnemySnapshot Enemy(ulong value, float x, float z)
        {
            return new EnemySnapshot(
                EnemyId.FromValue(value),
                EnemyKind.Regular,
                new Position3D(x, 0f, z),
                Health.Full(PlayModeTickHarness.EnemyHealth),
                CollisionRadius.FromValue(0.5f),
                new GameTimePoint(0d),
                false,
                default);
        }

        private static ArenaRunSnapshot CreateSnapshot(params EnemySnapshot[] enemies)
        {
            PlayerSnapshot player = new(
                PlayerId.FromValue(1UL),
                Position3D.Zero,
                Health.Full(100),
                WeaponKind.Melee,
                new GameTimePoint(0d),
                new GameTimePoint(0d),
                false,
                default);

            return new ArenaRunSnapshot(
                RunId,
                AggregateRevision.Initial,
                ArenaRunStatus.Playing,
                new GameTimePoint(0d),
                new ArenaBounds(-20f, 20f, -20f, 20f, 0f),
                player,
                enemies,
                new WaveSnapshot(WaveNumber.First, WavePhase.RegularCombat, 0, BossStatus.NotSpawned),
                WaveNumber.FromValue(3));
        }

        private GameObject Track(GameObject gameObject)
        {
            _objects.Add(gameObject);
            return gameObject;
        }
    }
}
