using System;
using Game.Arena.Domain.Geometry;
using NUnit.Framework;

namespace Game.Arena.Domain.Tests.Geometry
{
    [TestFixture]
    public sealed class Direction3DTests
    {
        [Test]
        public void CanonicalDirectionsAreUnitLength()
        {
            AssertUnitLength(Direction3D.Forward);
            AssertUnitLength(Direction3D.Back);
            AssertUnitLength(Direction3D.Up);
            AssertUnitLength(Direction3D.Down);
            AssertUnitLength(Direction3D.Right);
            AssertUnitLength(Direction3D.Left);
        }

        [Test]
        public void FromNormalizesNonUnitDisplacement()
        {
            Displacement3D displacement = new(0f, 0f, 10f);

            Direction3D direction = Direction3D.From(displacement);

            Assert.That(direction, Is.EqualTo(Direction3D.Forward));
        }

        [Test]
        public void FromThrowsForZeroDisplacement()
        {
            Assert.Throws<ArgumentException>(
                () =>
                {
                    Direction3D.From(Displacement3D.Zero);
                });
        }

        [Test]
        public void TryFromReturnsFalseForZeroDisplacement()
        {
            bool succeeded = Direction3D.TryFrom(
                Displacement3D.Zero,
                out _);

            Assert.That(succeeded, Is.False);
        }

        [Test]
        public void NegatedProducesOppositeUnitDirection()
        {
            Direction3D negated = Direction3D.Forward.Negated();

            Assert.That(negated, Is.EqualTo(Direction3D.Back));
        }

        [Test]
        public void ScalingProducesDisplacementNotDirection()
        {
            Displacement3D scaled = Direction3D.Forward * 5f;

            Assert.That(scaled.X, Is.EqualTo(0f));
            Assert.That(scaled.Y, Is.EqualTo(0f));
            Assert.That(scaled.Z, Is.EqualTo(5f));
        }

        private void AssertUnitLength(Direction3D direction)
        {
            float lengthSquared =
                (direction.X * direction.X)
                + (direction.Y * direction.Y)
                + (direction.Z * direction.Z);

            Assert.That(lengthSquared, Is.EqualTo(1f).Within(0.0001f));
        }
    }
}
