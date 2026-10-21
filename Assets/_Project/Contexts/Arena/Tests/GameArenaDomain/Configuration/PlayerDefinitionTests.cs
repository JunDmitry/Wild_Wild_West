using System;
using Game.Arena.Domain.Configuration;
using Game.Arena.Domain.Geometry;
using Game.Arena.Domain.Interactions.Movement;
using Game.Arena.Domain.Vitality;
using NUnit.Framework;

namespace Game.Arena.Domain.Tests.Configuration
{
    [TestFixture]
    public sealed class PlayerDefinitionTests
    {
        [Test]
        public void Constructor_IfInvalidHealth_ThrownArgument()
        {
            Assert.Throws<ArgumentException>(() =>
            {
                _ = new PlayerDefinition(
                    Position3D.Zero,
                    default,
                    MovementSpeed.FromUnitsPerSecond(1),
                    CollisionRadius.FromValue(.5f));
            });
        }

        [Test]
        public void Constructor_IfDepletedHealth_ThrownArgument()
        {
            Assert.Throws<ArgumentException>(() =>
            {
                _ = new PlayerDefinition(
                    Position3D.Zero,
                    Health.Full(20).ApplyDamage(DamageAmount.FromPoints(20)).RemainingHealth,
                    MovementSpeed.FromUnitsPerSecond(1),
                    CollisionRadius.FromValue(.5f));
            });
        }

        [Test]
        public void Constructor_IfInvalidMovementSpeed_ThrownArgument()
        {
            Assert.Throws<ArgumentException>(() =>
            {
                _ = new PlayerDefinition(
                    Position3D.Zero,
                    Health.Full(20),
                    default,
                    CollisionRadius.FromValue(.5f));
            });
        }

        [Test]
        public void Constructor_IfInvalidCollisionRadius_ThrownArgument()
        {
            Assert.Throws<ArgumentException>(() =>
            {
                _ = new PlayerDefinition(
                    Position3D.Zero,
                    Health.Full(20),
                    MovementSpeed.FromUnitsPerSecond(1),
                    default);
            });
        }

        [Test]
        public void Constructor_WhenCreate_SetValues()
        {
            PlayerDefinition playerDefinition = new(
                new Position3D(1, 0, 1),
                Health.Full(20),
                MovementSpeed.FromUnitsPerSecond(5),
                CollisionRadius.FromValue(1));

            Assert.IsTrue(new Position3D(1, 0, 1).Equals(playerDefinition.StartPosition));
            Assert.IsTrue(Health.Full(20).Equals(playerDefinition.InitialHealth));
            Assert.IsTrue(MovementSpeed.FromUnitsPerSecond(5).Equals(playerDefinition.MovementSpeed));
            Assert.IsTrue(CollisionRadius.FromValue(1).Equals(playerDefinition.CollisionRadius));
        }
    }
}
