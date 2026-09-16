using System;
using Game.Arena.Domain.Configuration;
using Game.Arena.Domain.Geometry;
using Game.Arena.Domain.Interactions.Movement;
using Game.Arena.Domain.Time;
using Game.Arena.Domain.Vitality;
using NUnit.Framework;

namespace Game.Arena.Domain.Tests.Configuration
{
    [TestFixture]
    public sealed class EnemyDefinitionTests
    {
        [Test]
        public void Constructor_IfInvalidHealth_ThrownArgument()
        {
            Assert.Throws<ArgumentException>(() =>
            {
                _ = new EnemyDefinition(
                    Combat.EnemyKind.Regular,
                    default,
                    MovementSpeed.FromUnitsPerSecond(5),
                    CollisionRadius.FromValue(.5f),
                    DamageAmount.FromPoints(10),
                    Distance.FromValue(5),
                    new GameDuration(.75));
            });
        }

        [Test]
        public void Constructor_IfDepletedHealth_ThrownArgument()
        {
            Assert.Throws<ArgumentException>(() =>
            {
                _ = new EnemyDefinition(
                    Combat.EnemyKind.Regular,
                    Health.Full(20).Reduce(DamageAmount.FromPoints(20)),
                    MovementSpeed.FromUnitsPerSecond(5),
                    CollisionRadius.FromValue(.5f),
                    DamageAmount.FromPoints(10),
                    Distance.FromValue(5),
                    new GameDuration(.75));
            });
        }

        [Test]
        public void Constructor_IfInvalidMovementSpeed_ThrownArgument()
        {
            Assert.Throws<ArgumentException>(() =>
            {
                _ = new EnemyDefinition(
                    Combat.EnemyKind.Regular,
                    Health.Full(20),
                    default,
                    CollisionRadius.FromValue(.5f),
                    DamageAmount.FromPoints(10),
                    Distance.FromValue(5),
                    new GameDuration(.75));
            });
        }

        [Test]
        public void Constructor_IfInvalidCollisionRadius_ThrownArgument()
        {
            Assert.Throws<ArgumentException>(() =>
            {
                _ = new EnemyDefinition(
                    Combat.EnemyKind.Regular,
                    Health.Full(20),
                    MovementSpeed.FromUnitsPerSecond(5),
                    default,
                    DamageAmount.FromPoints(10),
                    Distance.FromValue(5),
                    new GameDuration(.75));
            });
        }

        [Test]
        public void Constructor_IfAttackDamageNotPositive_ThrownArgument()
        {
            Assert.Throws<ArgumentException>(() =>
            {
                _ = new EnemyDefinition(
                    Combat.EnemyKind.Regular,
                    default,
                    MovementSpeed.FromUnitsPerSecond(5),
                    CollisionRadius.FromValue(.5f),
                    default,
                    Distance.FromValue(5),
                    new GameDuration(.75));
            });
        }

        [Test]
        public void Constructor_IfAttackRangeInvalid_ThrownArgument()
        {
            Assert.Throws<ArgumentOutOfRangeException>(() =>
            {
                _ = new EnemyDefinition(
                    Combat.EnemyKind.Regular,
                    Health.Full(20),
                    MovementSpeed.FromUnitsPerSecond(5),
                    CollisionRadius.FromValue(.5f),
                    DamageAmount.FromPoints(10),
                    default,
                    new GameDuration(.75));
            });
        }

        [Test]
        public void Constructor_IfAttackCooldownInvalid_ThrownOutOfRange()
        {
            Assert.Throws<ArgumentOutOfRangeException>(() =>
            {
                _ = new EnemyDefinition(
                    Combat.EnemyKind.Regular,
                    Health.Full(20),
                    MovementSpeed.FromUnitsPerSecond(5),
                    CollisionRadius.FromValue(.5f),
                    DamageAmount.FromPoints(10),
                    Distance.FromValue(5),
                    default);
            });
        }

        [Test]
        public void Constructor_WhenCreate_SetValues()
        {
            EnemyDefinition enemyDefinition = new(
                Combat.EnemyKind.Regular,
                Health.Full(20),
                MovementSpeed.FromUnitsPerSecond(5),
                CollisionRadius.FromValue(.5f),
                DamageAmount.FromPoints(10),
                Distance.FromValue(5),
                new GameDuration(.75));

            Assert.IsTrue(enemyDefinition.Kind == Combat.EnemyKind.Regular);
            Assert.IsTrue(enemyDefinition.InitialHealth.Equals(Health.Full(20)));
            Assert.IsTrue(enemyDefinition.MovementSpeed.Equals(MovementSpeed.FromUnitsPerSecond(5)));
            Assert.IsTrue(enemyDefinition.CollisionRadius.Equals(CollisionRadius.FromValue(.5f)));
            Assert.IsTrue(enemyDefinition.AttackDamage.Equals(DamageAmount.FromPoints(10)));
            Assert.IsTrue(enemyDefinition.AttackRange.Equals(Distance.FromValue(5)));
            Assert.IsTrue(enemyDefinition.AttackCooldown.Equals(new GameDuration(.75)));
        }
    }
}
