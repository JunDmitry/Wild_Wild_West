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
    public sealed class EnemyAttackOutcomeAnnouncementTests
    {
        private static readonly ArenaRunId RunId = ArenaRunId.FromValue(1UL);

        private readonly List<GameObject> _objects = new();
        private ArenaRunAnnouncer _announcer;
        private ArenaRunNotificationPresenter _presenter;

        [SetUp]
        public void SetUp()
        {
            _objects.Clear();

            GameObject root = Track(new GameObject("Notifications"));
            HitConfirmationPresenter hitConfirmation = root.AddComponent<HitConfirmationPresenter>();
            hitConfirmation.LifetimeSeconds = 10f;

            _announcer = root.AddComponent<ArenaRunAnnouncer>();
            _presenter = root.AddComponent<ArenaRunNotificationPresenter>();
            _presenter.HitConfirmation = hitConfirmation;
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
        public void Handle_Miss_AnnouncesMissedOnce()
        {
            _presenter.Handle(CreateSnapshot());

            _presenter.Handle(Resolved(EnemyId.FromValue(1UL), Revision(1UL), ArenaRunEnemyAttackOutcome.Miss));

            Assert.That(_announcer.AnnouncementCount, Is.EqualTo(1));
            Assert.That(
                _announcer.LastAnnouncement,
                Is.EqualTo(ArenaRunAnnouncer.EnemyAttackMissedAnnouncement));
        }

        [Test]
        public void Handle_Cancelled_AnnouncesMissedOnce()
        {
            _presenter.Handle(CreateSnapshot());

            _presenter.Handle(Resolved(
                EnemyId.FromValue(1UL),
                Revision(1UL),
                ArenaRunEnemyAttackOutcome.Cancelled));

            Assert.That(_announcer.AnnouncementCount, Is.EqualTo(1));
            Assert.That(
                _announcer.LastAnnouncement,
                Is.EqualTo(ArenaRunAnnouncer.EnemyAttackMissedAnnouncement));
        }

        [Test]
        public void Handle_Hit_AnnouncesNothing()
        {
            _presenter.Handle(CreateSnapshot());

            _presenter.Handle(Resolved(EnemyId.FromValue(1UL), Revision(1UL), ArenaRunEnemyAttackOutcome.Hit));

            Assert.That(_announcer.AnnouncementCount, Is.EqualTo(0));
        }

        [Test]
        public void Handle_SameMissNotificationTwice_AnnouncesOnce()
        {
            _presenter.Handle(CreateSnapshot());

            _presenter.Handle(Resolved(EnemyId.FromValue(1UL), Revision(1UL), ArenaRunEnemyAttackOutcome.Miss));
            _presenter.Handle(Resolved(EnemyId.FromValue(1UL), Revision(1UL), ArenaRunEnemyAttackOutcome.Miss));

            Assert.That(_announcer.AnnouncementCount, Is.EqualTo(1));
        }

        [Test]
        public void Handle_MissForTwoEnemiesInOneRevision_AnnouncesTwice()
        {
            _presenter.Handle(CreateSnapshot());

            _presenter.Handle(Resolved(EnemyId.FromValue(1UL), Revision(1UL), ArenaRunEnemyAttackOutcome.Miss));
            _presenter.Handle(Resolved(EnemyId.FromValue(2UL), Revision(1UL), ArenaRunEnemyAttackOutcome.Miss));

            Assert.That(_announcer.AnnouncementCount, Is.EqualTo(2));
        }

        [Test]
        public void Handle_NextMissForSameEnemy_AnnouncesAgain()
        {
            _presenter.Handle(CreateSnapshot());

            _presenter.Handle(Resolved(EnemyId.FromValue(1UL), Revision(1UL), ArenaRunEnemyAttackOutcome.Miss));
            _presenter.Handle(Resolved(EnemyId.FromValue(1UL), Revision(2UL), ArenaRunEnemyAttackOutcome.Miss));

            Assert.That(_announcer.AnnouncementCount, Is.EqualTo(2));
        }

        [Test]
        public void Handle_ForeignRun_IsIgnored()
        {
            _presenter.Handle(CreateSnapshot());

            _presenter.Handle(new ArenaRunEnemyAttackResolvedNotification(
                ArenaRunId.FromValue(2UL),
                Revision(1UL),
                EnemyId.FromValue(1UL),
                ArenaRunEnemyAttackOutcome.Miss));

            Assert.That(_announcer.AnnouncementCount, Is.EqualTo(0));
        }

        [Test]
        public void Handle_NotificationBeforeSnapshot_IsIgnored()
        {
            _presenter.Handle(Resolved(EnemyId.FromValue(1UL), Revision(1UL), ArenaRunEnemyAttackOutcome.Miss));

            Assert.That(_announcer.AnnouncementCount, Is.EqualTo(0));

            _presenter.Handle(CreateSnapshot());

            Assert.That(_announcer.AnnouncementCount, Is.EqualTo(0));
        }

        private static ArenaRunEnemyAttackResolvedNotification Resolved(
            EnemyId enemyId,
            AggregateRevision revision,
            ArenaRunEnemyAttackOutcome outcome)
        {
            return new ArenaRunEnemyAttackResolvedNotification(RunId, revision, enemyId, outcome);
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

        private static ArenaRunSnapshot CreateSnapshot()
        {
            EnemySnapshot enemy = new(
                EnemyId.FromValue(1UL),
                EnemyKind.Regular,
                new Position3D(2f, 0f, 3f),
                Health.Full(PlayModeTickHarness.EnemyHealth),
                CollisionRadius.FromValue(0.5f),
                new GameTimePoint(0d),
                false,
                default);

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
                new[] { enemy },
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
