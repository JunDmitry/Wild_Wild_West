using System;
using Game.Arena.Domain.Geometry;
using NUnit.Framework;

namespace Game.Arena.Domain.Tests.Geometry
{
    [TestFixture]
    public sealed class DistanceTests
    {
        [Test]
        public void FromValueRejectsNegative()
        {
            Assert.Throws<ArgumentOutOfRangeException>(
                () =>
                {
                    Distance.FromValue(-1f);
                });
        }

        [Test]
        public void FromValueRejectsNonFinite()
        {
            Assert.Throws<ArgumentOutOfRangeException>(
                () =>
                {
                    Distance.FromValue(float.NaN);
                });

            Assert.Throws<ArgumentOutOfRangeException>(
                () =>
                {
                    Distance.FromValue(float.PositiveInfinity);
                });
        }

        [Test]
        public void ZeroIsZero()
        {
            Assert.That(Distance.Zero.IsZero, Is.True);
            Assert.That(Distance.FromValue(0.5f).IsZero, Is.False);
        }

        [Test]
        public void ScaledMultipliesValue()
        {
            Distance scaled = Distance.FromValue(4f).Scaled(0.5f);

            Assert.That(scaled.Value, Is.EqualTo(2f));
        }

        [Test]
        public void ScaledRejectsNegativeFactor()
        {
            Assert.Throws<ArgumentOutOfRangeException>(
                () =>
                {
                    Distance.FromValue(1f).Scaled(-1f);
                });
        }

        [Test]
        public void ComparisonOrdersDistances()
        {
            Distance shorter = Distance.FromValue(1f);
            Distance longer = Distance.FromValue(2f);

            Assert.That(shorter < longer, Is.True);
            Assert.That(longer >= shorter, Is.True);
        }

        [Test]
        public void CollisionRadiusRejectsZeroDistance()
        {
            Assert.Throws<ArgumentOutOfRangeException>(
                () =>
                {
                    CollisionRadius.FromDistance(Distance.Zero);
                });
        }
    }
}
