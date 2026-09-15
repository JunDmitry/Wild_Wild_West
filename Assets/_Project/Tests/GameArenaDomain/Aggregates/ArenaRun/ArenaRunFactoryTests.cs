using System;
using Game.Arena.Domain.Aggregates.ArenaRun;
using Game.Arena.Domain.Combat;
using Game.Arena.Domain.Concurrency;
using Game.Arena.Domain.Geometry;
using Game.Arena.Domain.Identity;
using Game.Arena.Domain.Interactions.Movement;
using Game.Arena.Domain.Vitality;
using NUnit.Framework;

namespace Game.Arena.Domain.Tests.Aggregates._ArenaRun
{
    [TestFixture]
    public sealed class ArenaRunFactoryTests
    {
        private ArenaBounds _arenaBounds;
        private ArenaRunFactory _factory;
        private CollisionRadius _playerRadius;
        private MovementSpeed _playerSpeed;

        [SetUp]
        public void SetUp()
        {
            _factory = new ArenaRunFactory();
            _arenaBounds = new ArenaBounds(-20f, 20f, -20f, 20f, 0f);
            _playerSpeed = MovementSpeed.FromUnitsPerSecond(5f);
            _playerRadius = CollisionRadius.FromValue(0.5f);
        }

        [Test]
        public void StartCreatesPlayingRunAtFirstWave()
        {
            Game.Arena.Domain.Aggregates.ArenaRun.ArenaRun run = _factory.Start(
                ArenaRunId.FromValue(1UL),
                PlayerId.FromValue(2UL),
                Position3D.Zero,
                Health.Full(100),
                _playerSpeed,
                _playerRadius,
                _arenaBounds);

            Assert.That(run.Id, Is.EqualTo(ArenaRunId.FromValue(1UL)));
            Assert.That(run.PlayerId, Is.EqualTo(PlayerId.FromValue(2UL)));
            Assert.That(run.PlayerPosition, Is.EqualTo(Position3D.Zero));
            Assert.That(run.Status, Is.EqualTo(ArenaRunStatus.Playing));
            Assert.That(run.Revision, Is.EqualTo(AggregateRevision.Initial));
            Assert.That(run.CurrentWaveNumber, Is.EqualTo(WaveNumber.First));
            Assert.That(run.CurrentWavePhase, Is.EqualTo(WavePhase.RegularCombat));
            Assert.That(run.PlayerHealth, Is.EqualTo(Health.Full(100)));
            Assert.That(run.SelectedWeapon, Is.EqualTo(WeaponKind.Ranged));
        }

        [Test]
        public void StartRejectsNoneArenaRunId()
        {
            Assert.Throws<ArgumentException>(
                () =>
                {
                    _factory.Start(
                        ArenaRunId.None,
                        PlayerId.FromValue(2UL),
                        Position3D.Zero,
                        Health.Full(100),
                        _playerSpeed,
                        _playerRadius,
                        _arenaBounds);
                });
        }

        [Test]
        public void StartRejectsNonePlayerId()
        {
            Assert.Throws<ArgumentException>(
                () =>
                {
                    _factory.Start(
                        ArenaRunId.FromValue(1UL),
                        PlayerId.None,
                        Position3D.Zero,
                        Health.Full(100),
                        _playerSpeed,
                        _playerRadius,
                        _arenaBounds);
                });
        }

        [Test]
        public void StartRejectsPlayerOutsideArena()
        {
            Assert.Throws<ArgumentException>(
                () =>
                {
                    _factory.Start(
                        ArenaRunId.FromValue(1UL),
                        PlayerId.FromValue(2UL),
                        new Position3D(30f, 0f, 0f),
                        Health.Full(100),
                        _playerSpeed,
                        _playerRadius,
                        _arenaBounds);
                });
        }

        [Test]
        public void StartRejectsPlayerAtWrongHeight()
        {
            Assert.Throws<ArgumentException>(
                () =>
                {
                    _factory.Start(
                        ArenaRunId.FromValue(1UL),
                        PlayerId.FromValue(2UL),
                        new Position3D(0f, 1f, 0f),
                        Health.Full(100),
                        _playerSpeed,
                        _playerRadius,
                        _arenaBounds);
                });
        }

        [Test]
        public void StartRejectsArenaUnableToContainPlayer()
        {
            ArenaBounds smallArena = new(-0.25f, 0.25f, -0.25f, 0.25f, 0f);

            Assert.Throws<ArgumentException>(
                () =>
                {
                    _factory.Start(
                        ArenaRunId.FromValue(1UL),
                        PlayerId.FromValue(2UL),
                        Position3D.Zero,
                        Health.Full(100),
                        _playerSpeed,
                        _playerRadius,
                        smallArena);
                });
        }

        [Test]
        public void StartRejectsDepletedPlayerHealth()
        {
            Health depleted = Health.Full(10).Reduce(DamageAmount.FromPoints(10));

            Assert.Throws<ArgumentException>(
                () =>
                {
                    _factory.Start(
                        ArenaRunId.FromValue(1UL),
                        PlayerId.FromValue(2UL),
                        Position3D.Zero,
                        depleted,
                        _playerSpeed,
                        _playerRadius,
                        _arenaBounds);
                });
        }

        [Test]
        public void StartRejectsUninitializedPlayerHealth()
        {
            Assert.Throws<ArgumentException>(
                () =>
                {
                    _factory.Start(
                        ArenaRunId.FromValue(1UL),
                        PlayerId.FromValue(2UL),
                        Position3D.Zero,
                        default,
                        _playerSpeed,
                        _playerRadius,
                        _arenaBounds);
                });
        }
    }
}
