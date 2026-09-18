using System;
using Game.Arena.Domain.Vitality;
using NUnit.Framework;

namespace Game.Arena.Domain.Tests.Vitality
{
    [TestFixture]
    public sealed class DamageApplicationTests
    {
        [Test]
        public void ApplyDamageReportsRequestedAndAppliedDamage()
        {
            Health health = Health.Full(100);
            DamageAmount damage = DamageAmount.FromPoints(30);

            DamageApplication application = health.ApplyDamage(damage);

            Assert.That(application.RequestedDamage, Is.EqualTo(damage));
            Assert.That(application.AppliedDamage.Points, Is.EqualTo(30));
            Assert.That(application.RemainingHealth.Current, Is.EqualTo(70));
            Assert.That(application.IsLethal, Is.False);
        }

        [Test]
        public void ApplyDamageReportsOverkillCorrectly()
        {
            Health health = Health.Full(5);
            DamageAmount damage = DamageAmount.FromPoints(20);

            DamageApplication application = health.ApplyDamage(damage);

            Assert.That(application.RequestedDamage.Points, Is.EqualTo(20));
            Assert.That(application.AppliedDamage.Points, Is.EqualTo(5));
            Assert.That(application.RemainingHealth.Current, Is.EqualTo(0));
            Assert.That(application.IsLethal, Is.True);
        }

        [Test]
        public void ApplyingDamageToDepletedHealthThrows()
        {
            Health health = Health.Full(5)
                .ApplyDamage(DamageAmount.FromPoints(5))
                .RemainingHealth;

            Assert.Throws<InvalidOperationException>(
                () =>
                {
                    health.ApplyDamage(DamageAmount.FromPoints(1));
                });
        }

        [Test]
        public void ApplicationCannotReportMoreDamageThanRequested()
        {
            Health health = Health.Full(10);
            DamageAmount damage = DamageAmount.FromPoints(3);

            DamageApplication application = health.ApplyDamage(damage);

            Assert.That(application.AppliedDamage <= application.RequestedDamage, Is.True);
        }
    }
}
