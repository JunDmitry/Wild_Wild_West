using System;
using Game.Arena.Domain.Configuration;
using Game.Arena.Domain.Geometry;
using Game.Arena.Domain.Vitality;
using NUnit.Framework;

namespace Game.Arena.Domain.Tests.Configuration
{
    [TestFixture]
    public sealed class WeaponDefinitionTests
    {
        [Test]
        public void Constructor_IfInvalidDamage_ThrownArgument()
        {
            Assert.Throws<ArgumentException>(() =>
            {
                _ = new WeaponDefinition(
                    Combat.WeaponKind.Ranged,
                    default,
                    Distance.FromValue(5),
                    new Domain.Time.GameDuration(.9),
                    new Domain.Time.GameDuration(.4d));
            });
        }

        [Test]
        public void Constructor_IfNonPositiveRange_ThrownOutOfRange()
        {
            Assert.Throws<ArgumentOutOfRangeException>(() =>
            {
                _ = new WeaponDefinition(
                    Combat.WeaponKind.Ranged,
                    DamageAmount.FromPoints(1),
                    default,
                    new Domain.Time.GameDuration(.9),
                    new Domain.Time.GameDuration(.4d));
            });
        }

        [Test]
        public void Constructor_IfNonPositiveCooldown_ThrownOutOfRange()
        {
            Assert.Throws<ArgumentOutOfRangeException>(() =>
            {
                _ = new WeaponDefinition(
                    Combat.WeaponKind.Melee,
                    DamageAmount.FromPoints(1),
                    Distance.FromValue(5),
                    default,
                    new Domain.Time.GameDuration(.4d));
            });
        }

        [Test]
        public void Constructor_WhenCreate_SetValues()
        {
            WeaponDefinition weaponDefinition = new(
                Combat.WeaponKind.Melee,
                DamageAmount.FromPoints(20),
                Distance.FromValue(5),
                new Domain.Time.GameDuration(1.1),
                new Domain.Time.GameDuration(.4d));

            Assert.IsTrue(weaponDefinition.Kind == Combat.WeaponKind.Melee);
            Assert.IsTrue(weaponDefinition.Damage.Equals(DamageAmount.FromPoints(20)));
            Assert.IsTrue(weaponDefinition.Range.Equals(Distance.FromValue(5)));
            Assert.IsTrue(weaponDefinition.Cooldown.Equals(new Domain.Time.GameDuration(1.1)));
        }
    }
}
