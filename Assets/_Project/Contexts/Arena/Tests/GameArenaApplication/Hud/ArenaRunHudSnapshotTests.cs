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
                false);
        }
    }
}
