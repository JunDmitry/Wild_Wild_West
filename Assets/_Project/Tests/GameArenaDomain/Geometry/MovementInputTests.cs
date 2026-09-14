using System;
using Game.Arena.Domain.Geometry;
using NUnit.Framework;

namespace Game.Arena.Domain.Tests.Geometry
{
    [TestFixture]
    public sealed class MovementInputTests
    {
        [Test]
        public void ZeroIsZero()
        {
            Assert.That(MovementInput.Zero.IsZero, Is.True);
        }

        [Test]
        public void FromVectorAcceptsUnitLength()
        {
            MovementInput input = MovementInput.FromVector(new Displacement3D(1f, 0f, 0f));

            Assert.That(input.IsZero, Is.False);
        }

        [Test]
        public void FromVectorRejectsLengthAboveOne()
        {
            Assert.Throws<ArgumentOutOfRangeException>(
                () =>
                {
                    MovementInput.FromVector(new Displacement3D(2f, 0f, 0f));
                });
        }

        [Test]
        public void TryGetDirectionFailsForZeroInput()
        {
            bool succeeded = MovementInput.Zero.TryGetDirection(out _);

            Assert.That(succeeded, Is.False);
        }

        [Test]
        public void TryGetDirectionSucceedsForNonZeroInput()
        {
            MovementInput input = MovementInput.FromVector(new Displacement3D(0f, 0f, 1f));

            bool succeeded = input.TryGetDirection(out Direction3D direction);

            Assert.That(succeeded, Is.True);
            Assert.That(direction, Is.EqualTo(Direction3D.Forward));
        }
    }
}
