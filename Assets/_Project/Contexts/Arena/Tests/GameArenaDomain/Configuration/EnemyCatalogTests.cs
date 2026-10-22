using System;
using Game.Arena.Domain.Configuration;
using Game.Arena.Domain.Geometry;
using Game.Arena.Domain.Interactions.Movement;
using Game.Arena.Domain.Vitality;
using NUnit.Framework;

namespace Game.Arena.Domain.Tests.Configuration
{
    [TestFixture]
    public sealed class EnemyCatalogTests
    {
        [Test]
        public void Constructor_IfNullDefinition_ThrownNull()
        {
            Assert.Throws<ArgumentNullException>(() =>
            {
                _ = new EnemyCatalog(null,
                        new EnemyDefinition(
                            Combat.EnemyKind.Boss,
                            Health.Full(100),
                            MovementSpeed.FromUnitsPerSecond(1),
                            CollisionRadius.FromValue(.5f),
                            DamageAmount.FromPoints(5),
                            Distance.FromValue(5f),
                            new Domain.Time.GameDuration(.75),
                            new Domain.Time.GameDuration(.4d)));
            });
            Assert.Throws<ArgumentNullException>(() =>
            {
                _ = new EnemyCatalog(
                        new EnemyDefinition(
                            Combat.EnemyKind.Regular,
                            Health.Full(20),
                            MovementSpeed.FromUnitsPerSecond(1),
                            CollisionRadius.FromValue(.5f),
                            DamageAmount.FromPoints(5),
                            Distance.FromValue(5f),
                            new Domain.Time.GameDuration(.75),
                            new Domain.Time.GameDuration(.4d)),
                        null);
            });
        }

        [Test]
        public void Constructor_IfInvalidKind_ThrownArgument()
        {
            Assert.Throws<ArgumentException>(() =>
            {
                _ = new EnemyCatalog(
                    new EnemyDefinition(
                        Combat.EnemyKind.Boss,
                        Health.Full(20),
                        MovementSpeed.FromUnitsPerSecond(1),
                        CollisionRadius.FromValue(.5f),
                        DamageAmount.FromPoints(5),
                        Distance.FromValue(5f),
                        new Domain.Time.GameDuration(.75),
                        new Domain.Time.GameDuration(.4d)),
                    new EnemyDefinition(
                        Combat.EnemyKind.Boss,
                        Health.Full(20),
                        MovementSpeed.FromUnitsPerSecond(1),
                        CollisionRadius.FromValue(.5f),
                        DamageAmount.FromPoints(5),
                        Distance.FromValue(5f),
                        new Domain.Time.GameDuration(.75),
                        new Domain.Time.GameDuration(.4d)));
            });
            Assert.Throws<ArgumentException>(() =>
            {
                _ = new EnemyCatalog(
                        new EnemyDefinition(
                            Combat.EnemyKind.Regular,
                            Health.Full(20),
                            MovementSpeed.FromUnitsPerSecond(1),
                            CollisionRadius.FromValue(.5f),
                            DamageAmount.FromPoints(5),
                            Distance.FromValue(5f),
                            new Domain.Time.GameDuration(.75),
                            new Domain.Time.GameDuration(.4d)),
                        new EnemyDefinition(
                            Combat.EnemyKind.Regular,
                            Health.Full(100),
                            MovementSpeed.FromUnitsPerSecond(1),
                            CollisionRadius.FromValue(.5f),
                            DamageAmount.FromPoints(5),
                            Distance.FromValue(5f),
                            new Domain.Time.GameDuration(.75),
                            new Domain.Time.GameDuration(.4d)));
            });
        }

        [Test]
        public void Constructor_IfBossHealthEqualsRegular_ThrownArgument()
        {
            Assert.Throws<ArgumentException>(() =>
            {
                _ = new EnemyCatalog(
                    RegularWithHealth(50),
                    BossWithHealth(50));
            });
        }

        [Test]
        public void Constructor_IfBossHealthBelowRegular_ThrownArgument()
        {
            Assert.Throws<ArgumentException>(() =>
            {
                _ = new EnemyCatalog(
                    RegularWithHealth(50),
                    BossWithHealth(40));
            });
        }

        [Test]
        public void Constructor_IfBossHealthAboveRegular_Accepted()
        {
            EnemyCatalog catalog = new(RegularWithHealth(50), BossWithHealth(51));

            Assert.That(catalog.Get(Combat.EnemyKind.Boss).InitialHealth.Maximum, Is.EqualTo(51));
        }

        [Test]
        public void GetEnemyDefinition_ReturnCorrentDefinition()
        {
            EnemyDefinition regular = new EnemyDefinition(
                            Combat.EnemyKind.Regular,
                            Health.Full(20),
                            MovementSpeed.FromUnitsPerSecond(1),
                            CollisionRadius.FromValue(.5f),
                            DamageAmount.FromPoints(5),
                            Distance.FromValue(5f),
                            new Domain.Time.GameDuration(.75),
                            new Domain.Time.GameDuration(.4d));
            EnemyDefinition boss = new EnemyDefinition(
                        Combat.EnemyKind.Boss,
                        Health.Full(30),
                        MovementSpeed.FromUnitsPerSecond(1),
                        CollisionRadius.FromValue(.5f),
                        DamageAmount.FromPoints(5),
                        Distance.FromValue(5f),
                        new Domain.Time.GameDuration(.75),
                        new Domain.Time.GameDuration(.4d));
            EnemyCatalog catalog = new(regular, boss);

            Assert.AreSame(regular, catalog.Get(Combat.EnemyKind.Regular));
            Assert.AreSame(boss, catalog.Get(Combat.EnemyKind.Boss));
        }

        private static EnemyDefinition RegularWithHealth(int maximumHealth)
        {
            return new EnemyDefinition(
                Combat.EnemyKind.Regular,
                Health.Full(maximumHealth),
                MovementSpeed.FromUnitsPerSecond(1),
                CollisionRadius.FromValue(.5f),
                DamageAmount.FromPoints(5),
                Distance.FromValue(5f),
                new Domain.Time.GameDuration(.75),
                new Domain.Time.GameDuration(.4d));
        }

        private static EnemyDefinition BossWithHealth(int maximumHealth)
        {
            return new EnemyDefinition(
                Combat.EnemyKind.Boss,
                Health.Full(maximumHealth),
                MovementSpeed.FromUnitsPerSecond(1),
                CollisionRadius.FromValue(1f),
                DamageAmount.FromPoints(5),
                Distance.FromValue(5f),
                new Domain.Time.GameDuration(.75),
                new Domain.Time.GameDuration(.4d));
        }
    }
}
