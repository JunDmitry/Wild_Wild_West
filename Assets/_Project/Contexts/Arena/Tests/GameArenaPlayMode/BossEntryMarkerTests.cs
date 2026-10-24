using System.Collections.Generic;
using Game.Arena.Application.Hud;
using Game.Arena.Application.ReadModels;
using Game.Arena.Domain.Aggregates.ArenaRun;
using Game.Arena.Domain.Combat;
using Game.Arena.Domain.Concurrency;
using Game.Arena.Domain.Geometry;
using Game.Arena.Domain.Identity;
using Game.Arena.Domain.Time;
using Game.Arena.Domain.Vitality;
using Game.Arena.Presentation.Combat;
using NUnit.Framework;
using UnityEngine;

namespace Game.Arena.PlayMode.Tests
{
    [TestFixture]
    public sealed class BossEntryMarkerTests
    {
        private readonly List<GameObject> _objects = new();
        private readonly ArenaRunHudSnapshotMapper _mapper = new();
        private BossEntryMarkerPresenter _presenter;

        [SetUp]
        public void SetUp()
        {
            _objects.Clear();

            GameObject root = Track(new GameObject("BossEntryMarkers"));

            GameObject templateObject = Track(new GameObject("MarkerTemplate"));
            templateObject.transform.SetParent(root.transform);
            templateObject.AddComponent<LineRenderer>();
            BossEntryMarkerView template = templateObject.AddComponent<BossEntryMarkerView>();
            templateObject.SetActive(false);

            _presenter = root.AddComponent<BossEntryMarkerPresenter>();
            _presenter.Template = template;
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
        public void Marker_AppearsWhenTheBossCrossesIntoBounds()
        {
            Present(CreateSnapshot(Enemy(EnemyKind.Boss, 25f, 0f)));
            Assert.That(_presenter.IsVisible, Is.False);

            Present(CreateSnapshot(Enemy(EnemyKind.Boss, 5f, 0f)));

            Assert.That(_presenter.IsVisible, Is.True);
            Assert.That(_presenter.Label, Is.EqualTo(BossEntryMarkerView.StatusLabel));
            Assert.That(_presenter.Label, Is.EqualTo("Boss"));
            Assert.That(_presenter.Position.X, Is.EqualTo(5f).Within(1e-4f));
            Assert.That(_presenter.Position.Z, Is.Zero);
        }

        [Test]
        public void Marker_ClearsOnTheNextPresentation()
        {
            Present(CreateSnapshot(Enemy(EnemyKind.Boss, 25f, 0f)));
            Present(CreateSnapshot(Enemy(EnemyKind.Boss, 5f, 0f)));
            Assert.That(_presenter.IsVisible, Is.True);

            Present(CreateSnapshot(Enemy(EnemyKind.Boss, 6f, 0f)));

            Assert.That(_presenter.IsVisible, Is.False);
        }

        [Test]
        public void Marker_NotShownBeforeTheBossEnters()
        {
            Present(CreateSnapshot(Enemy(EnemyKind.Boss, 40f, 0f)));
            Present(CreateSnapshot(Enemy(EnemyKind.Boss, 25f, 0f)));

            Assert.That(_presenter.IsVisible, Is.False);
        }

        [Test]
        public void Marker_NotShownForRegularEnemies()
        {
            Present(CreateSnapshot(Enemy(EnemyKind.Regular, 25f, 0f)));
            Present(CreateSnapshot(Enemy(EnemyKind.Regular, 5f, 0f)));

            Assert.That(_presenter.IsVisible, Is.False);
        }

        [Test]
        public void Marker_NotShownWhenARegularEnemyEntersBesideTheBoss()
        {
            Present(CreateSnapshot(
                Enemy(EnemyKind.Boss, 25f, 0f),
                Enemy(EnemyKind.Regular, 25f, 5f)));

            Present(CreateSnapshot(
                Enemy(EnemyKind.Boss, 25f, 0f),
                Enemy(EnemyKind.Regular, 5f, 5f)));

            Assert.That(_presenter.IsVisible, Is.False);
        }

        [Test]
        public void Marker_NotShownOnANewRunWhereTheBossIsAlreadyPresented()
        {
            Present(CreateSnapshot(Enemy(EnemyKind.Boss, 5f, 0f)));
            Assert.That(_presenter.IsVisible, Is.False);

            Present(CreateSnapshot(2UL, Enemy(EnemyKind.Boss, 5f, 0f)));

            Assert.That(_presenter.IsVisible, Is.False);
        }

        [Test]
        public void Marker_NotShownWhenTheRunIsTerminal()
        {
            Present(CreateSnapshot(Enemy(EnemyKind.Boss, 25f, 0f)));

            Present(CreateSnapshot(
                ArenaRunStatus.Victory,
                Enemy(EnemyKind.Boss, 5f, 0f)));

            Assert.That(_presenter.IsVisible, Is.False);
        }

        [Test]
        public void Marker_IsNotFocusable()
        {
            Present(CreateSnapshot(Enemy(EnemyKind.Boss, 25f, 0f)));
            Present(CreateSnapshot(Enemy(EnemyKind.Boss, 5f, 0f)));

            BossEntryMarkerView[] markers = _presenter.GetComponentsInChildren<BossEntryMarkerView>();

            Assert.That(markers.Length, Is.EqualTo(1));

            for (int index = 0; index < markers.Length; index++)
            {
                Assert.That(markers[index].GetComponent<Collider>(), Is.Null);
                Assert.That(markers[index].GetComponent<Collider2D>(), Is.Null);
            }
        }

        [Test]
        public void Present_NullArguments_Throw()
        {
            ArenaRunSnapshot snapshot = CreateSnapshot(Enemy(EnemyKind.Boss, 5f, 0f));
            ArenaRunHudSnapshot hud = _mapper.Map(snapshot);

            Assert.Throws<System.ArgumentNullException>(() => _presenter.Present(null, hud));
            Assert.Throws<System.ArgumentNullException>(() => _presenter.Present(snapshot, null));
        }

        private void Present(ArenaRunSnapshot snapshot)
        {
            _presenter.Present(snapshot, _mapper.Map(snapshot));
        }

        private static ArenaRunSnapshot CreateSnapshot(params EnemySnapshot[] enemies)
        {
            return CreateSnapshot(ArenaRunStatus.Playing, enemies);
        }

        private static ArenaRunSnapshot CreateSnapshot(ulong arenaRunId, EnemySnapshot enemy)
        {
            return CreateSnapshotCore(
                ArenaRunId.FromValue(arenaRunId),
                ArenaRunStatus.Playing,
                new[] { enemy });
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
                WeaponKind.Ranged,
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
                new WaveSnapshot(WaveNumber.First, WavePhase.BossCombat, 0, BossStatus.Alive),
                WaveNumber.FromValue(3));
        }

        private static EnemySnapshot Enemy(EnemyKind kind, float x, float z)
        {
            float radius = kind == EnemyKind.Boss ? 1f : 0.5f;

            return new EnemySnapshot(
                EnemyId.FromValue(2UL),
                kind,
                new Position3D(x, 0f, z),
                Health.Full(300),
                CollisionRadius.FromValue(radius),
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
