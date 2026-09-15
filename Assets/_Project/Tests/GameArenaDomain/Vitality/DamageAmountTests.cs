using System;
using Game.Arena.Domain.Vitality;
using NUnit.Framework;

namespace Game.Arena.Domain.Tests.Vitality
{
    [TestFixture]
    public sealed class DamageAmountTests
    {
        [Test]
        public void FromPointsRejectsZero()
        {
            Assert.Throws<ArgumentOutOfRangeException>(
                () =>
                {
                    DamageAmount.FromPoints(0);
                });
        }

        [Test]
        public void FromPointsRejectsNegative()
        {
            Assert.Throws<ArgumentOutOfRangeException>(
                () =>
                {
                    DamageAmount.FromPoints(-5);
                });
        }

        [Test]
        public void FromPointsStoresPositiveValue()
        {
            DamageAmount damage = DamageAmount.FromPoints(12);

            Assert.That(damage.Points, Is.EqualTo(12));
        }

        [Test]
        public void EqualityIsBasedOnPoints()
        {
            DamageAmount left = DamageAmount.FromPoints(7);
            DamageAmount right = DamageAmount.FromPoints(7);
            DamageAmount other = DamageAmount.FromPoints(8);

            Assert.That(left == right, Is.True);
            Assert.That(left != other, Is.True);
            Assert.That(left.GetHashCode(), Is.EqualTo(right.GetHashCode()));
        }

        [Test]
        public void ComparisonOrdersDamage()
        {
            DamageAmount weak = DamageAmount.FromPoints(3);
            DamageAmount strong = DamageAmount.FromPoints(9);

            Assert.That(weak < strong, Is.True);
            Assert.That(strong >= weak, Is.True);
        }
    }

    [TestFixture]
    public sealed class HealthTests
    {
        [Test]
        public void FullRejectsNonPositiveMaximum()
        {
            Assert.Throws<ArgumentOutOfRangeException>(
                () =>
                {
                    Health.Full(0);
                });

            Assert.Throws<ArgumentOutOfRangeException>(
                () =>
                {
                    Health.Full(-10);
                });
        }

        [Test]
        public void FullStartsAtMaximumAndIsNotDepleted()
        {
            Health health = Health.Full(100);

            Assert.That(health.Current, Is.EqualTo(100));
            Assert.That(health.Maximum, Is.EqualTo(100));
            Assert.That(health.IsDepleted, Is.False);
            Assert.That(health.IsValid, Is.True);
        }

        [Test]
        public void ReducedSubtractsDamagePoints()
        {
            Health health = Health.Full(100).Reduce(DamageAmount.FromPoints(30));

            Assert.That(health.Current, Is.EqualTo(70));
            Assert.That(health.Maximum, Is.EqualTo(100));
            Assert.That(health.IsDepleted, Is.False);
        }

        [Test]
        public void ExactLethalDamageDepletesHealth()
        {
            Health health = Health.Full(25).Reduce(DamageAmount.FromPoints(25));

            Assert.That(health.Current, Is.EqualTo(0));
            Assert.That(health.IsDepleted, Is.True);
        }

        [Test]
        public void ExcessiveDamageClampsAtZero()
        {
            Health health = Health.Full(25).Reduce(DamageAmount.FromPoints(400));

            Assert.That(health.Current, Is.EqualTo(0));
            Assert.That(health.Maximum, Is.EqualTo(25));
            Assert.That(health.IsDepleted, Is.True);
        }

        [Test]
        public void DamageDoesNotChangeMaximum()
        {
            Health health = Health.Full(80)
                .Reduce(DamageAmount.FromPoints(10))
                .Reduce(DamageAmount.FromPoints(10));

            Assert.That(health.Current, Is.EqualTo(60));
            Assert.That(health.Maximum, Is.EqualTo(80));
        }

        [Test]
        public void DefaultHealthIsInvalid()
        {
            Health health = default;

            Assert.That(health.IsValid, Is.False);
        }

        [Test]
        public void ReducingInvalidHealthThrows()
        {
            Health health = default;

            Assert.Throws<InvalidOperationException>(
                () =>
                {
                    health.Reduce(DamageAmount.FromPoints(1));
                });
        }

        [Test]
        public void EqualityIsBasedOnCurrentAndMaximum()
        {
            Health left = Health.Full(50).Reduce(DamageAmount.FromPoints(10));
            Health right = Health.Full(50).Reduce(DamageAmount.FromPoints(10));
            Health other = Health.Full(50);

            Assert.That(left == right, Is.True);
            Assert.That(left != other, Is.True);
        }
    }
}
