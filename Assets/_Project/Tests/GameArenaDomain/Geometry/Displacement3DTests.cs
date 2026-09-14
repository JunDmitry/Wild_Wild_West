using Game.Arena.Domain.Geometry;
using NUnit.Framework;

namespace Game.Arena.Domain.Tests.Geometry
{
    [TestFixture]
    public sealed class Displacement3DTests
    {
        [Test]
        public void ZeroHasZeroLengthAndIsZero()
        {
            Displacement3D zero = Displacement3D.Zero;

            Assert.That(zero.Length, Is.EqualTo(0f));
            Assert.That(zero.IsZero, Is.True);
        }

        [Test]
        public void LengthMatchesEuclideanNorm()
        {
            Displacement3D vector = new(3f, 0f, 4f);

            Assert.That(vector.Length, Is.EqualTo(5f).Within(0.0001f));
        }

        [Test]
        public void TryToDirectionFailsForZeroLength()
        {
            Displacement3D zero = Displacement3D.Zero;

            bool succeeded = zero.TryToDirection(out Direction3D direction);

            Assert.That(succeeded, Is.False);
        }

        [Test]
        public void TryToDirectionSucceedsForNonZeroLength()
        {
            Displacement3D vector = new(0f, 0f, 5f);

            bool succeeded = vector.TryToDirection(out Direction3D direction);

            Assert.That(succeeded, Is.True);
            Assert.That(direction, Is.EqualTo(Direction3D.Forward));
        }

        [Test]
        public void AdditionCombinesComponents()
        {
            Displacement3D left = new(1f, 2f, 3f);
            Displacement3D right = new(4f, 5f, 6f);

            Displacement3D sum = left + right;

            Assert.That(sum.X, Is.EqualTo(5f));
            Assert.That(sum.Y, Is.EqualTo(7f));
            Assert.That(sum.Z, Is.EqualTo(9f));
        }

        [Test]
        public void ScalingMultipliesEachComponent()
        {
            Displacement3D vector = new(1f, 2f, 3f);

            Displacement3D scaled = vector * 2f;

            Assert.That(scaled.X, Is.EqualTo(2f));
            Assert.That(scaled.Y, Is.EqualTo(4f));
            Assert.That(scaled.Z, Is.EqualTo(6f));
        }

        [Test]
        public void PositionPlusDisplacementProducesExpectedPosition()
        {
            Position3D position = new(1f, 0f, 0f);
            Displacement3D displacement = new(2f, 0f, 0f);

            Position3D result = position + displacement;

            Assert.That(result.X, Is.EqualTo(3f));
        }

        [Test]
        public void PositionDifferenceProducesDisplacement()
        {
            Position3D from = new(1f, 0f, 0f);
            Position3D to = new(4f, 0f, 0f);

            Displacement3D difference = to - from;

            Assert.That(difference.X, Is.EqualTo(3f));
        }
    }
}
