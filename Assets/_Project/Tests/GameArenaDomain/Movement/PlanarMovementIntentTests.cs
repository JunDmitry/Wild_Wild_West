using System;
using Game.Arena.Domain.Geometry;
using NUnit.Framework;

namespace Game.Arena.Domain.Movement
{
    [TestFixture]
    public sealed class PlanarMovementIntentTests
    {
        [Test]
        public void RequestedPositionIsDerivedFromIntent()
        {
            PlanarMovementIntent intent = new(
                new Position3D(1f, 0f, 1f),
                Direction3D.Right,
                Distance.FromValue(3f));

            Assert.That(intent.RequestedPosition, Is.EqualTo(new Position3D(4f, 0f, 1f)));
            Assert.That(intent.IsValid, Is.True);
        }

        [Test]
        public void ConstructorRejectsZeroDistance()
        {
            Assert.Throws<ArgumentOutOfRangeException>(
                () =>
                {
                    PlanarMovementIntent unused = new(
                        Position3D.Zero,
                        Direction3D.Right,
                        Distance.Zero);
                });
        }

        [Test]
        public void ConstructorRejectsVerticalDirection()
        {
            Assert.Throws<ArgumentException>(
                () =>
                {
                    PlanarMovementIntent unused = new(
                        Position3D.Zero,
                        Direction3D.Up,
                        Distance.FromValue(1f));
                });
        }

        [Test]
        public void ConstructorRejectsInvalidDirection()
        {
            Assert.Throws<ArgumentException>(
                () =>
                {
                    PlanarMovementIntent unused = new(
                        Position3D.Zero,
                        default,
                        Distance.FromValue(1f));
                });
        }

        [Test]
        public void DefaultIntentIsInvalid()
        {
            PlanarMovementIntent intent = default;

            Assert.That(intent.IsValid, Is.False);
        }
    }
}
