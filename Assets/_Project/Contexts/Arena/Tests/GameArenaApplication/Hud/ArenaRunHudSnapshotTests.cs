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
                false);
        }
    }
}
