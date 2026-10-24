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

        [Test]
        public void MapShowsTheBossHealthWhenTheBossIsInsideBounds()
        {
            ArenaRunHudSnapshot hud = _mapper.Map(
                CreateSnapshot(
                    100,
                    100,
                    WeaponKind.Ranged,
                    ArenaRunStatus.Playing,
                    Enemy(2UL, EnemyKind.Boss, 4f, 0f, 180, 300)));

            Assert.That(hud.BossLabel, Is.EqualTo("Boss"));
            Assert.That(hud.IsBossPresent, Is.True);
            Assert.That(hud.BossHealthCurrent, Is.EqualTo(180));
            Assert.That(hud.BossHealthMaximum, Is.EqualTo(300));
            Assert.That(hud.BossHealthText, Is.EqualTo("180 / 300"));
            Assert.That(hud.BossHealthFraction, Is.EqualTo(0.6f));
        }

        [Test]
        public void MapHidesTheBossWhileItIsStillOutsideBounds()
        {
            ArenaRunHudSnapshot hud = _mapper.Map(
                CreateSnapshot(
                    100,
                    100,
                    WeaponKind.Ranged,
                    ArenaRunStatus.Playing,
                    Enemy(2UL, EnemyKind.Boss, 25f, 0f, 300, 300)));

            Assert.That(hud.IsBossPresent, Is.False);
            Assert.That(hud.BossHealthCurrent, Is.Zero);
            Assert.That(hud.BossHealthMaximum, Is.Zero);
            Assert.That(hud.BossHealthText, Is.Empty);
            Assert.That(hud.BossHealthFraction, Is.Zero);
        }

        [Test]
        public void MapHidesTheBossWhenOnlyRegularEnemiesArePresent()
        {
            ArenaRunHudSnapshot hud = _mapper.Map(
                CreateSnapshot(
                    100,
                    100,
                    WeaponKind.Ranged,
                    ArenaRunStatus.Playing,
                    Enemy(1UL, EnemyKind.Regular, 2f, 0f, 50, 50)));

            Assert.That(hud.IsBossPresent, Is.False);
            Assert.That(hud.BossHealthText, Is.Empty);
        }

        [Test]
        public void MapHidesTheBossWhenThereAreNoEnemies()
        {
            ArenaRunHudSnapshot hud = _mapper.Map(
                CreateSnapshot(100, 100, WeaponKind.Ranged, ArenaRunStatus.Playing));

            Assert.That(hud.IsBossPresent, Is.False);
            Assert.That(hud.BossLabel, Is.EqualTo("Boss"));
        }

        [Test]
        public void MapPrefersThePresentedBossOverAnOutsideOne()
        {
            ArenaRunHudSnapshot hud = _mapper.Map(
                CreateSnapshot(
                    100,
                    100,
                    WeaponKind.Ranged,
                    ArenaRunStatus.Playing,
                    Enemy(2UL, EnemyKind.Boss, 25f, 0f, 300, 300),
                    Enemy(3UL, EnemyKind.Boss, 5f, 0f, 120, 300)));

            Assert.That(hud.IsBossPresent, Is.True);
            Assert.That(hud.BossHealthCurrent, Is.EqualTo(120));
        }

        private static ArenaRunSnapshot CreateSnapshot(
            int currentHealth,
            int maximumHealth,
            WeaponKind weapon,
            ArenaRunStatus status,
            params EnemySnapshot[] enemies)
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
                enemies,
                new WaveSnapshot(
                    WaveNumber.First,
                    WavePhase.RegularCombat,
                    0,
                    BossStatus.NotSpawned),
                WaveNumber.FromValue(3));
        }

        private static EnemySnapshot Enemy(
            ulong id,
            EnemyKind kind,
            float x,
            float z,
            int currentHealth,
            int maximumHealth)
        {
            Health health = Health.Full(maximumHealth);
            int damage = maximumHealth - currentHealth;

            if (damage > 0)
            {
                health = health
                    .ApplyDamage(DamageAmount.FromPoints(damage))
                    .RemainingHealth;
            }

            return new EnemySnapshot(
                EnemyId.FromValue(id),
                kind,
                new Position3D(x, 0f, z),
                health,
                CollisionRadius.FromValue(0.5f),
                new GameTimePoint(0d),
                false,
                default);
        }
    }
}
