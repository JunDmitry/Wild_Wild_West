using System;
using Game.Arena.Domain.Aggregates.ArenaRun;
using Game.Arena.Domain.Combat;
using Game.Arena.Domain.Concurrency;
using Game.Arena.Domain.Configuration;
using Game.Arena.Domain.Geometry;
using Game.Arena.Domain.Identity;
using Game.Arena.Domain.Interactions.Movement;
using Game.Arena.Domain.Tests.Support;
using Game.Arena.Domain.Vitality;
using NUnit.Framework;

namespace Game.Arena.Domain.Tests.Aggregates._ArenaRun
{
    [TestFixture]
    public sealed class ArenaRunFactoryTests
    {
        private ArenaRunTestKit _kit;

        [SetUp]
        public void SetUp()
        {
            _kit = new ArenaRunTestKit();
        }

        [Test]
        public void StartCreatesPlayingRunAtFirstWave()
        {
            Game.Arena.Domain.Aggregates.ArenaRun.ArenaRun run = _kit.StartRun();

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
                    new ArenaRunFactory().Start(
                        ArenaRunId.None,
                        PlayerId.FromValue(2UL),
                        _kit.Arena,
                        _kit.PlayerAt(Position3D.Zero),
                        _kit.Enemies,
                        _kit.Waves(2, 1, 1));
                });
        }

        [Test]
        public void StartRejectsNonePlayerId()
        {
            Assert.Throws<ArgumentException>(
                () =>
                {
                    new ArenaRunFactory().Start(
                        ArenaRunId.FromValue(1UL),
                        PlayerId.None,
                        _kit.Arena,
                        _kit.PlayerAt(Position3D.Zero),
                        _kit.Enemies,
                        _kit.Waves(2, 1, 1));
                });
        }

        [Test]
        public void StartRejectsPlayerOutsideArena()
        {
            Assert.Throws<ArgumentException>(
                () =>
                {
                    _kit.StartRun(new Position3D(30f, 0f, 0f));
                });
        }

        [Test]
        public void StartRejectsPlayerAtWrongHeight()
        {
            Assert.Throws<ArgumentException>(
                () =>
                {
                    _kit.StartRun(new Position3D(0f, 1f, 0f));
                });
        }

        [Test]
        public void StartRejectsArenaUnableToContainPlayer()
        {
            ArenaBounds smallArena = new(-0.25f, 0.25f, -0.25f, 0.25f, 0f);

            Assert.Throws<ArgumentException>(
                () =>
                {
                    new ArenaRunFactory().Start(
                        ArenaRunId.None,
                        PlayerId.FromValue(2UL),
                        new ArenaDefinition(smallArena, Distance.FromValue(0.01f)),
                        _kit.PlayerAt(Position3D.Zero),
                        _kit.Enemies,
                        _kit.Waves(2, 1, 1));
                });
        }

        [Test]
        public void StartRejectsDepletedPlayerHealth()
        {
            Health depleted = Health.Full(10).Reduce(DamageAmount.FromPoints(10));

            Assert.Throws<ArgumentException>(
                () =>
                {
                    new ArenaRunFactory().Start(
                        ArenaRunId.None,
                        PlayerId.FromValue(2UL),
                        _kit.Arena,
                        new PlayerDefinition(Position3D.Zero, depleted, MovementSpeed.FromUnitsPerSecond(5f), CollisionRadius.FromValue(0.5f)),
                        _kit.Enemies,
                        _kit.Waves(2, 1, 1));
                });
        }

        [Test]
        public void StartRejectsUninitializedPlayerHealth()
        {
            Assert.Throws<ArgumentException>(
                () =>
                {
                    new ArenaRunFactory().Start(
                        ArenaRunId.None,
                        PlayerId.FromValue(2UL),
                        _kit.Arena,
                        new PlayerDefinition(Position3D.Zero, default, MovementSpeed.FromUnitsPerSecond(5f), CollisionRadius.FromValue(0.5f)),
                        _kit.Enemies,
                        _kit.Waves(2, 1, 1));
                });
        }
    }
}
