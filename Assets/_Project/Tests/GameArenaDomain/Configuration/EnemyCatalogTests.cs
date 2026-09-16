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
                            new Domain.Time.GameDuration(.75)));
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
                            new Domain.Time.GameDuration(.75)),
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
                        new Domain.Time.GameDuration(.75)),
                    new EnemyDefinition(
                        Combat.EnemyKind.Boss,
                        Health.Full(20),
                        MovementSpeed.FromUnitsPerSecond(1),
                        CollisionRadius.FromValue(.5f),
                        DamageAmount.FromPoints(5),
                        Distance.FromValue(5f),
                        new Domain.Time.GameDuration(.75)));
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
                            new Domain.Time.GameDuration(.75)),
                        new EnemyDefinition(
                            Combat.EnemyKind.Regular,
                            Health.Full(100),
                            MovementSpeed.FromUnitsPerSecond(1),
                            CollisionRadius.FromValue(.5f),
                            DamageAmount.FromPoints(5),
                            Distance.FromValue(5f),
                            new Domain.Time.GameDuration(.75)));
            });
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
                            new Domain.Time.GameDuration(.75));
            EnemyDefinition boss = new EnemyDefinition(
                        Combat.EnemyKind.Boss,
                        Health.Full(20),
                        MovementSpeed.FromUnitsPerSecond(1),
                        CollisionRadius.FromValue(.5f),
                        DamageAmount.FromPoints(5),
                        Distance.FromValue(5f),
                        new Domain.Time.GameDuration(.75));
            EnemyCatalog catalog = new(regular, boss);

            Assert.AreSame(regular, catalog.Get(Combat.EnemyKind.Regular));
            Assert.AreSame(boss, catalog.Get(Combat.EnemyKind.Boss));
        }
    }
}
