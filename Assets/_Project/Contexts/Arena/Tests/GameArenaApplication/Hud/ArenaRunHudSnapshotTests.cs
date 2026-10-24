using System;
using NUnit.Framework;

namespace Game.Arena.Application.Hud
{
    [TestFixture]
    public sealed class ArenaRunHudSnapshotTests
    {
        [Test]
        public void ConstructorDerivesTextAndFraction()
        {
            ArenaRunHudSnapshot hud = Create(25, 50);

            Assert.That(hud.HealthText, Is.EqualTo("25 / 50"));
            Assert.That(hud.HealthFraction, Is.EqualTo(0.5f));
        }

        [Test]
        public void ConstructorRejectsNonPositiveMaximum()
        {
            Assert.Throws<ArgumentOutOfRangeException>(
                () =>
                {
                    _ = Create(0, 0);
                });
        }

        [Test]
        public void ConstructorRejectsCurrentAboveMaximum()
        {
            Assert.Throws<ArgumentOutOfRangeException>(
                () =>
                {
                    _ = Create(51, 50);
                });
        }

        [Test]
        public void ConstructorRejectsNegativeCurrent()
        {
            Assert.Throws<ArgumentOutOfRangeException>(
                () =>
                {
                    _ = Create(-1, 50);
                });
        }

        [Test]
        public void ConstructorRejectsEmptyHealthLabel()
        {
            Assert.Throws<ArgumentException>(
                () =>
                {
                    _ = new ArenaRunHudSnapshot(
                        " ",
                        1,
                        1,
                        "Equipped weapon",
                        "Firearm",
                        1,
                        3,
                        false,
                        "Boss",
                        0,
                        0,
                        false);
                });
        }

        [Test]
        public void ConstructorRejectsEmptyWeaponText()
        {
            Assert.Throws<ArgumentException>(
                () =>
                {
                    _ = new ArenaRunHudSnapshot(
                        "Health",
                        1,
                        1,
                        "Equipped weapon",
                        string.Empty,
                        1,
                        3,
                        false,
                        "Boss",
                        0,
                        0,
                        false);
                });
        }

        [Test]
        public void ConstructorDerivesWaveText()
        {
            ArenaRunHudSnapshot hud = Create(25, 50);

            Assert.That(hud.WaveCurrent, Is.EqualTo(1));
            Assert.That(hud.WaveTotal, Is.EqualTo(3));
            Assert.That(hud.WaveText, Is.EqualTo("Wave 1 of 3"));
        }

        [Test]
        public void ConstructorDerivesSecondWaveText()
        {
            ArenaRunHudSnapshot hud = new ArenaRunHudSnapshot(
                "Health",
                1,
                1,
                "Equipped weapon",
                "Firearm",
                2,
                3,
                false,
                "Boss",
                0,
                0,
                false);

            Assert.That(hud.WaveCurrent, Is.EqualTo(2));
            Assert.That(hud.WaveTotal, Is.EqualTo(3));
            Assert.That(hud.WaveText, Is.EqualTo("Wave 2 of 3"));
        }

        [Test]
        public void ConstructorRejectsWaveCurrentBelowOne()
        {
            Assert.Throws<ArgumentOutOfRangeException>(
                () =>
                {
                    _ = new ArenaRunHudSnapshot(
                        "Health",
                        1,
                        1,
                        "Equipped weapon",
                        "Firearm",
                        0,
                        3,
                        false,
                        "Boss",
                        0,
                        0,
                        false);
                });
        }

        [Test]
        public void ConstructorRejectsWaveTotalBeforeCurrent()
        {
            Assert.Throws<ArgumentException>(
                () =>
                {
                    _ = new ArenaRunHudSnapshot(
                        "Health",
                        1,
                        1,
                        "Equipped weapon",
                        "Firearm",
                        3,
                        2,
                        false,
                        "Boss",
                        0,
                        0,
                        false);
                });
        }

        [Test]
        public void ConstructorDerivesBossTextAndFraction()
        {
            ArenaRunHudSnapshot hud = CreateWithBoss(180, 300);

            Assert.That(hud.BossLabel, Is.EqualTo("Boss"));
            Assert.That(hud.BossHealthCurrent, Is.EqualTo(180));
            Assert.That(hud.BossHealthMaximum, Is.EqualTo(300));
            Assert.That(hud.BossHealthText, Is.EqualTo("180 / 300"));
            Assert.That(hud.BossHealthFraction, Is.EqualTo(0.6f));
            Assert.That(hud.IsBossPresent, Is.True);
        }

        [Test]
        public void ConstructorCarriesAnAbsentBossWithoutValues()
        {
            ArenaRunHudSnapshot hud = Create(25, 50);

            Assert.That(hud.BossLabel, Is.EqualTo("Boss"));
            Assert.That(hud.BossHealthCurrent, Is.Zero);
            Assert.That(hud.BossHealthMaximum, Is.Zero);
            Assert.That(hud.BossHealthText, Is.Empty);
            Assert.That(hud.BossHealthFraction, Is.Zero);
            Assert.That(hud.IsBossPresent, Is.False);
        }

        [Test]
        public void ConstructorRejectsEmptyBossLabel()
        {
            Assert.Throws<ArgumentException>(
                () =>
                {
                    _ = new ArenaRunHudSnapshot(
                        "Health",
                        1,
                        1,
                        "Equipped weapon",
                        "Firearm",
                        1,
                        3,
                        false,
                        " ",
                        300,
                        300,
                        true);
                });
        }

        [Test]
        public void ConstructorRejectsNonPositiveBossMaximum()
        {
            Assert.Throws<ArgumentOutOfRangeException>(
                () =>
                {
                    _ = CreateWithBoss(0, 0);
                });
        }

        [Test]
        public void ConstructorRejectsBossCurrentAboveMaximum()
        {
            Assert.Throws<ArgumentOutOfRangeException>(
                () =>
                {
                    _ = CreateWithBoss(301, 300);
                });
        }

        [Test]
        public void ConstructorRejectsNegativeBossCurrent()
        {
            Assert.Throws<ArgumentOutOfRangeException>(
                () =>
                {
                    _ = CreateWithBoss(-1, 300);
                });
        }

        [Test]
        public void ConstructorRejectsBossHealthWhenNoBossIsPresent()
        {
            Assert.Throws<ArgumentException>(
                () =>
                {
                    _ = new ArenaRunHudSnapshot(
                        "Health",
                        1,
                        1,
                        "Equipped weapon",
                        "Firearm",
                        1,
                        3,
                        false,
                        "Boss",
                        300,
                        300,
                        false);
                });
        }

        private static ArenaRunHudSnapshot Create(int current, int maximum)
        {
            return new ArenaRunHudSnapshot(
                "Health",
                current,
                maximum,
                "Equipped weapon",
                "Firearm",
                1,
                3,
                false,
                "Boss",
                0,
                0,
                false);
        }

        private static ArenaRunHudSnapshot CreateWithBoss(int bossCurrent, int bossMaximum)
        {
            return new ArenaRunHudSnapshot(
                "Health",
                1,
                1,
                "Equipped weapon",
                "Firearm",
                1,
                3,
                false,
                "Boss",
                bossCurrent,
                bossMaximum,
                true);
        }
    }
}
