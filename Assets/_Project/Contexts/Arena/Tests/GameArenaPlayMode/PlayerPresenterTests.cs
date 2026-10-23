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
using Game.Arena.Presentation.Player;
using NUnit.Framework;
using UnityEngine;

namespace Game.Arena.PlayMode.Tests
{
    [TestFixture]
    public sealed class PlayerPresenterTests
    {
        private readonly List<GameObject> _objects = new();
        private GameObject _playerObject;
        private PlayerPresenter _presenter;

        [SetUp]
        public void SetUp()
        {
            _objects.Clear();

            _playerObject = new GameObject("Player");
            _objects.Add(_playerObject);

            _playerObject.AddComponent<PlayerView>();
            _presenter = _playerObject.AddComponent<PlayerPresenter>();
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
        public void Present_MovesPlayerObjectToSnapshotPosition()
        {
            _playerObject.transform.position = new Vector3(9f, 9f, 9f);

            _presenter.Present(CreateSnapshot(new Position3D(3f, 0f, 4f)));

            Assert.That(_playerObject.transform.position.x, Is.EqualTo(3f).Within(1e-4f));
            Assert.That(_playerObject.transform.position.y, Is.EqualTo(0f).Within(1e-4f));
            Assert.That(_playerObject.transform.position.z, Is.EqualTo(4f).Within(1e-4f));
        }

        [Test]
        public void Present_AppliesFullSnapshotPositionIncludingHeight()
        {
            _presenter.Present(CreateSnapshot(new Position3D(-2f, 0.5f, 7f)));

            Assert.That(_playerObject.transform.position.x, Is.EqualTo(-2f).Within(1e-4f));
            Assert.That(_playerObject.transform.position.y, Is.EqualTo(0.5f).Within(1e-4f));
            Assert.That(_playerObject.transform.position.z, Is.EqualTo(7f).Within(1e-4f));
        }

        [Test]
        public void Present_NullSnapshot_Throws()
        {
            Assert.Throws<ArgumentNullException>(() => _presenter.Present(null));
        }

        private static ArenaRunSnapshot CreateSnapshot(Position3D playerPosition)
        {
            PlayerSnapshot player = new(
                PlayerId.FromValue(1UL),
                playerPosition,
                Health.Full(100),
                WeaponKind.Melee,
                new GameTimePoint(0d),
                new GameTimePoint(0d),
                false,
                default);

            return new ArenaRunSnapshot(
                ArenaRunId.FromValue(1UL),
                AggregateRevision.Initial,
                ArenaRunStatus.Playing,
                new GameTimePoint(0d),
                new ArenaBounds(-20f, 20f, -20f, 20f, 0f),
                player,
                Array.Empty<EnemySnapshot>(),
                new WaveSnapshot(WaveNumber.First, WavePhase.RegularCombat, 0, BossStatus.NotSpawned),
                WaveNumber.FromValue(3));
        }
    }
}
