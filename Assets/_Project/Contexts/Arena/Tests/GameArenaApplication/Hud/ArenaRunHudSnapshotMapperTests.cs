using System;
using Game.Arena.Application.ReadModels;
using Game.Arena.Domain.Aggregates.ArenaRun;
using Game.Arena.Domain.Combat;
using Game.Arena.Domain.Concurrency;
using Game.Arena.Domain.Geometry;
using Game.Arena.Domain.Identity;
using Game.Arena.Domain.Time;
using Game.Arena.Domain.Vitality;
using NUnit.Framework;

namespace Game.Arena.Application.Hud
{
    [TestFixture]
    public sealed class ArenaRunHudSnapshotMapperTests
    {
        private ArenaRunHudSnapshotMapper _mapper;

        [SetUp]
        public void SetUp()
        {
            _mapper = new ArenaRunHudSnapshotMapper();
        }

        [Test]
        public void MapShowsFullHealthForAFullRun()
        {
            ArenaRunHudSnapshot hud = _mapper.Map(
                CreateSnapshot(100, 100, WeaponKind.Ranged, ArenaRunStatus.Playing));

            Assert.That(hud.HealthLabel, Is.EqualTo("Health"));
            Assert.That(hud.HealthCurrent, Is.EqualTo(100));
            Assert.That(hud.HealthMaximum, Is.EqualTo(100));
            Assert.That(hud.HealthText, Is.EqualTo("100 / 100"));
            Assert.That(hud.HealthFraction, Is.EqualTo(1f));
            Assert.That(hud.IsDefeated, Is.False);
        }

        [Test]
        public void MapShowsCurrentAgainstMaximumForDamagedHealth()
        {
            ArenaRunHudSnapshot hud = _mapper.Map(
                CreateSnapshot(60, 100, WeaponKind.Ranged, ArenaRunStatus.Playing));

            Assert.That(hud.HealthCurrent, Is.EqualTo(60));
            Assert.That(hud.HealthText, Is.EqualTo("60 / 100"));
            Assert.That(hud.HealthFraction, Is.EqualTo(0.6f));
        }

        [Test]
        public void MapShowsZeroAndDefeatedForADefeatedPlayer()
        {
            ArenaRunHudSnapshot hud = _mapper.Map(
                CreateSnapshot(0, 100, WeaponKind.Ranged, ArenaRunStatus.Defeat));

            Assert.That(hud.HealthCurrent, Is.Zero);
            Assert.That(hud.HealthText, Is.EqualTo("0 / 100"));
            Assert.That(hud.HealthFraction, Is.Zero);
            Assert.That(hud.IsDefeated, Is.True);
        }

        [Test]
        public void MapDoesNotMarkAVictoryAsDefeated()
        {
            ArenaRunHudSnapshot hud = _mapper.Map(
                CreateSnapshot(80, 100, WeaponKind.Ranged, ArenaRunStatus.Victory));

            Assert.That(hud.IsDefeated, Is.False);
        }

        [Test]
        public void MapNamesTheFirearmForTheRangedWeapon()
        {
            ArenaRunHudSnapshot hud = _mapper.Map(
                CreateSnapshot(100, 100, WeaponKind.Ranged, ArenaRunStatus.Playing));

            Assert.That(hud.EquippedWeaponLabel, Is.EqualTo("Equipped weapon"));
            Assert.That(hud.EquippedWeaponText, Is.EqualTo("Firearm"));
        }

        [Test]
        public void MapNamesTheMeleeWeaponForTheMeleeWeapon()
        {
            ArenaRunHudSnapshot hud = _mapper.Map(
                CreateSnapshot(100, 100, WeaponKind.Melee, ArenaRunStatus.Playing));

            Assert.That(hud.EquippedWeaponText, Is.EqualTo("Melee Weapon"));
        }

        [Test]
        public void MapRejectsAnUnknownWeaponKind()
        {
            Assert.Throws<ArgumentOutOfRangeException>(
                () =>
                {
                    _mapper.Map(
                        CreateSnapshot(
                            100,
                            100,
                            (WeaponKind)42,
                            ArenaRunStatus.Playing));
                });
        }

        [Test]
        public void MapRejectsNullSource()
        {
            Assert.Throws<ArgumentNullException>(
                () =>
                {
                    _mapper.Map(null);
                });
        }

        [Test]
        public void MapShowsTheCurrentWaveAgainstTheTotal()
        {
            ArenaRunHudSnapshot hud = _mapper.Map(
                CreateSnapshot(100, 100, WeaponKind.Ranged, ArenaRunStatus.Playing));

            Assert.That(hud.WaveCurrent, Is.EqualTo(1));
            Assert.That(hud.WaveTotal, Is.EqualTo(3));
            Assert.That(hud.WaveText, Is.EqualTo("Wave 1 of 3"));
        }

        private static ArenaRunSnapshot CreateSnapshot(
            int currentHealth,
            int maximumHealth,
            WeaponKind weapon,
            ArenaRunStatus status)
        {
            Health health = Health.Full(maximumHealth);
            int damage = maximumHealth - currentHealth;

            if (damage > 0)
            {
                health = health
                    .ApplyDamage(DamageAmount.FromPoints(damage))
                    .RemainingHealth;
            }

            PlayerSnapshot player = new(
                PlayerId.FromValue(1UL),
                new Position3D(0f, 0f, 0f),
                health,
                weapon,
                new GameTimePoint(0d),
                new GameTimePoint(0d),
                false,
                default);

            return new ArenaRunSnapshot(
                ArenaRunId.FromValue(1UL),
                AggregateRevision.Initial,
                status,
                new GameTimePoint(0d),
                new ArenaBounds(-10f, 10f, -10f, 10f, 0f),
                player,
                Array.Empty<EnemySnapshot>(),
                new WaveSnapshot(
                    WaveNumber.First,
                    WavePhase.RegularCombat,
                    0,
                    BossStatus.NotSpawned),
                WaveNumber.FromValue(3));
        }
    }
}
