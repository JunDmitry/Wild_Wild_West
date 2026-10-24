using System;
using System.Collections.Generic;
using Game.Arena.Application.ReadModels;
using Game.Arena.Domain.Aggregates.ArenaRun;
using Game.Arena.Domain.Combat;
using Game.Arena.Domain.Concurrency;
using Game.Arena.Domain.Geometry;
using Game.Arena.Domain.Identity;
using Game.Arena.Domain.Time;
using Game.Arena.Domain.Vitality;
using Game.Arena.Presentation.Accessibility;
using Game.Arena.Presentation.Enemies;
using NUnit.Framework;
using UnityEngine;

namespace Game.Arena.PlayMode.Tests
{
    [TestFixture]
    public sealed class EnemyEntryPresentationTests
    {
        private readonly List<GameObject> _objects = new();
        private EnemyViewRegistry _registry;
        private ArenaRunAnnouncer _announcer;
        private EnemyPresenter _presenter;

        [SetUp]
        public void SetUp()
        {
            _objects.Clear();

            GameObject root = Track(new GameObject("Enemies"));

            _registry = root.AddComponent<EnemyViewRegistry>();
            _announcer = root.AddComponent<ArenaRunAnnouncer>();
            _presenter = root.AddComponent<EnemyPresenter>();
            _presenter.Registry = _registry;
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
        public void Enemy_OutsideBounds_IsNotRendered()
        {
            Present(CreateSnapshot(Enemy(1UL, 25f, 0f)));

            Assert.That(_registry.Count, Is.Zero);
            Assert.That(_registry.IsRendered(EnemyId.FromValue(1UL)), Is.False);
            Assert.That(_registry.transform.childCount, Is.Zero);
            Assert.That(_registry.GetComponentsInChildren<EnemyView>().Length, Is.Zero);
        }

        [Test]
        public void Enemy_InsideBounds_IsRenderedAndFollowsTheSnapshot()
        {
            EnemyId enemyId = EnemyId.FromValue(1UL);

            Present(CreateSnapshot(Enemy(1UL, 25f, 0f)));
            Present(CreateSnapshot(Enemy(1UL, 5f, 0f)));

            Assert.That(_registry.IsRendered(enemyId), Is.True);
            Assert.That(_registry.Count, Is.EqualTo(1));
            Assert.That(_registry.PositionOf(enemyId).X, Is.EqualTo(5f).Within(1e-4f));

            Present(CreateSnapshot(Enemy(1UL, 4f, 0f)));

            Assert.That(_registry.PositionOf(enemyId).X, Is.EqualTo(4f).Within(1e-4f));
            Assert.That(_registry.PositionOf(enemyId).Z, Is.Zero);
        }

        [Test]
        public void OnlyInsideEnemiesOfAMixedSnapshotAreRendered()
        {
            EnemyId insideId = EnemyId.FromValue(1UL);
            EnemyId outsideId = EnemyId.FromValue(2UL);

            Present(CreateSnapshot(
                Enemy(1UL, 5f, 0f),
                Enemy(2UL, 25f, 0f)));

            Assert.That(_registry.Count, Is.EqualTo(1));
            Assert.That(_registry.IsRendered(insideId), Is.True);
            Assert.That(_registry.IsRendered(outsideId), Is.False);
            Assert.That(_registry.transform.childCount, Is.EqualTo(1));
        }

        [Test]
        public void EntryAnnouncement_FiresOnceAtTheCrossing()
        {
            Present(CreateSnapshot(Enemy(1UL, 25f, 0f)));
            Present(CreateSnapshot(Enemy(1UL, 5f, 0f)));

            Assert.That(_announcer.AnnouncementCount, Is.EqualTo(1));
            Assert.That(_announcer.LastAnnouncement, Is.EqualTo(ArenaRunAnnouncer.EnemyEnteredAnnouncement));
            Assert.That(_announcer.LastAnnouncement, Is.EqualTo("Enemy entered the arena"));

            Present(CreateSnapshot(Enemy(1UL, 4f, 0f)));
            Present(CreateSnapshot(Enemy(1UL, 3f, 0f)));

            Assert.That(_announcer.AnnouncementCount, Is.EqualTo(1));
        }

        [Test]
        public void EntryAnnouncement_IsNotFiredWhileTheEnemyStaysOutside()
        {
            Present(CreateSnapshot(Enemy(1UL, 25f, 0f)));
            Present(CreateSnapshot(Enemy(1UL, 24f, 0f)));
            Present(CreateSnapshot(Enemy(1UL, 23f, 0f)));

            Assert.That(_announcer.AnnouncementCount, Is.Zero);
        }

        [Test]
        public void EntryAnnouncement_IsNotFiredWhenTheCrossingIsTerminal()
        {
            Present(CreateSnapshot(Enemy(1UL, 25f, 0f)));

            Present(CreateSnapshot(
                ArenaRunStatus.Victory,
                Enemy(1UL, 5f, 0f)));

            Assert.That(_announcer.AnnouncementCount, Is.Zero);
        }

        [Test]
        public void NewRun_ClearsViewsAndRememberedEntries()
        {
            EnemyId firstRunEnemy = EnemyId.FromValue(1UL);
            EnemyId secondRunEnemy = EnemyId.FromValue(2UL);
            EnemyId secondRunLateEnemy = EnemyId.FromValue(3UL);

            Present(CreateSnapshot(1UL, Enemy(1UL, 25f, 0f)));
            Present(CreateSnapshot(1UL, Enemy(1UL, 5f, 0f)));

            Assert.That(_announcer.AnnouncementCount, Is.EqualTo(1));
            Assert.That(_registry.IsRendered(firstRunEnemy), Is.True);

            Present(CreateSnapshot(2UL, Enemy(2UL, 5f, 0f)));

            Assert.That(_announcer.AnnouncementCount, Is.EqualTo(1));
            Assert.That(_registry.IsRendered(firstRunEnemy), Is.False);
            Assert.That(_registry.IsRendered(secondRunEnemy), Is.True);

            Present(CreateSnapshot(
                2UL,
                Enemy(2UL, 4f, 0f),
                Enemy(3UL, 25f, 0f)));

            Assert.That(_announcer.AnnouncementCount, Is.EqualTo(1));

            Present(CreateSnapshot(
                2UL,
                Enemy(2UL, 4f, 0f),
                Enemy(3UL, 5f, 0f)));

            Assert.That(_announcer.AnnouncementCount, Is.EqualTo(2));
            Assert.That(_registry.IsRendered(secondRunLateEnemy), Is.True);
        }

        [Test]
        public void EnemyView_IsNotFocusable()
        {
            EnemyId enemyId = EnemyId.FromValue(1UL);

            Present(CreateSnapshot(Enemy(1UL, 5f, 0f)));

            Assert.That(_registry.TryGetView(enemyId, out EnemyView view), Is.True);
            Assert.That(view.gameObject.activeSelf, Is.True);
            Assert.That(view.GetComponent<Collider>(), Is.Null);
            Assert.That(view.GetComponent<Collider2D>(), Is.Null);
        }

        [Test]
        public void Present_NullSnapshot_Throws()
        {
            Assert.Throws<ArgumentNullException>(() => _presenter.Present(null));
        }

        private void Present(ArenaRunSnapshot snapshot)
        {
            _presenter.Present(snapshot);
        }

        private static ArenaRunSnapshot CreateSnapshot(params EnemySnapshot[] enemies)
        {
            return CreateSnapshotCore(ArenaRunId.FromValue(1UL), ArenaRunStatus.Playing, enemies);
        }

        private static ArenaRunSnapshot CreateSnapshot(ulong arenaRunId, params EnemySnapshot[] enemies)
        {
            return CreateSnapshotCore(ArenaRunId.FromValue(arenaRunId), ArenaRunStatus.Playing, enemies);
        }

        private static ArenaRunSnapshot CreateSnapshot(
            ArenaRunStatus status,
            params EnemySnapshot[] enemies)
        {
            return CreateSnapshotCore(ArenaRunId.FromValue(1UL), status, enemies);
        }

        private static ArenaRunSnapshot CreateSnapshotCore(
            ArenaRunId arenaRunId,
            ArenaRunStatus status,
            EnemySnapshot[] enemies)
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
                arenaRunId,
                AggregateRevision.Initial,
                status,
                new GameTimePoint(0d),
                new ArenaBounds(-20f, 20f, -20f, 20f, 0f),
                player,
                enemies,
                new WaveSnapshot(WaveNumber.First, WavePhase.RegularCombat, 0, BossStatus.NotSpawned),
                WaveNumber.FromValue(3));
        }

        private static EnemySnapshot Enemy(ulong enemyId, float x, float z)
        {
            return new EnemySnapshot(
                EnemyId.FromValue(enemyId),
                EnemyKind.Regular,
                new Position3D(x, 0f, z),
                Health.Full(100),
                CollisionRadius.FromValue(0.5f),
                new GameTimePoint(0d),
                false,
                default);
        }

        private GameObject Track(GameObject gameObject)
        {
            _objects.Add(gameObject);
            return gameObject;
        }
    }
}
