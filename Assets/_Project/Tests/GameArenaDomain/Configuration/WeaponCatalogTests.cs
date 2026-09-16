using System;
using Game.Arena.Domain.Configuration;
using Game.Arena.Domain.Geometry;
using Game.Arena.Domain.Tests.Support;
using Game.Arena.Domain.Time;
using Game.Arena.Domain.Vitality;
using NUnit.Framework;

namespace Game.Arena.Domain.Tests.Configuration
{
    [TestFixture]
    public sealed class WeaponCatalogTests
    {
        private ArenaRunTestKit _kit;

        [SetUp]
        public void SetUp()
        {
            _kit = new ArenaRunTestKit();
        }

        [Test]
        public void Constructor_IfWeaponDefinitionNull_ThrownNull()
        {
            Assert.Throws<ArgumentNullException>(() =>
            {
                _ = new WeaponCatalog(
                    null,
                    _kit.Weapons.Get(Combat.WeaponKind.Melee));
            });
            Assert.Throws<ArgumentNullException>(() =>
            {
                _ = new WeaponCatalog(
                    _kit.Weapons.Get(Combat.WeaponKind.Ranged),
                    null);
            });
        }

        [Test]
        public void Constructor_IfInvalidWeaponKind_ThrownArgument()
        {
            Assert.Throws<ArgumentException>(() =>
            {
                _ = new WeaponCatalog(
                    _kit.Weapons.Get(Combat.WeaponKind.Melee),
                    _kit.Weapons.Get(Combat.WeaponKind.Melee));
            });
            Assert.Throws<ArgumentException>(() =>
            {
                _ = new WeaponCatalog(
                    _kit.Weapons.Get(Combat.WeaponKind.Ranged),
                    _kit.Weapons.Get(Combat.WeaponKind.Ranged));
            });
        }

        [Test]
        public void Get_WeaponDefinition_ReturnCorrectKind()
        {
            WeaponDefinition ranged = new(
                Combat.WeaponKind.Ranged,
                DamageAmount.FromPoints(10),
                Distance.FromValue(10),
                new GameDuration(1));
            WeaponDefinition melee = new(
                Combat.WeaponKind.Melee,
                DamageAmount.FromPoints(20),
                Distance.FromValue(2),
                new GameDuration(1.25));
            WeaponCatalog catalog = new(ranged, melee);

            Assert.AreSame(ranged, catalog.Get(Combat.WeaponKind.Ranged));
            Assert.AreSame(melee, catalog.Get(Combat.WeaponKind.Melee));
        }
    }
}
