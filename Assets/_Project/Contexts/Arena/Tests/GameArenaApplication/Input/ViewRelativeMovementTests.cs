using Game.Arena.Application.Input;
using Game.Arena.Domain.Geometry;
using NUnit.Framework;

namespace Game.Arena.Application.Tests.Input
{
    [TestFixture]
    public sealed class ViewRelativeMovementTests
    {
        [Test]
        public void ForwardAxisMovesAlongTheViewDirection()
        {
            MovementInput movement = ViewRelativeMovement.Translate(
                new MovementAxes(1f, 0f),
                Direction3D.Forward);

            Assert.That(
                movement,
                Is.EqualTo(MovementInput.FromVector(new Displacement3D(0f, 0f, 1f))));
        }

        [Test]
        public void BackAxisMovesTowardTheView()
        {
            MovementInput movement = ViewRelativeMovement.Translate(
                new MovementAxes(-1f, 0f),
                Direction3D.Forward);

            Assert.That(
                movement,
                Is.EqualTo(MovementInput.FromVector(new Displacement3D(0f, 0f, -1f))));
        }

        [Test]
        public void RightAxisMovesBesideTheView()
        {
            MovementInput movement = ViewRelativeMovement.Translate(
                new MovementAxes(0f, 1f),
                Direction3D.Right);

            Assert.That(
                movement,
                Is.EqualTo(MovementInput.FromVector(new Displacement3D(0f, 0f, -1f))));
        }

        [Test]
        public void RotatedViewTurnsTheMovementWithIt()
        {
            MovementInput movement = ViewRelativeMovement.Translate(
                new MovementAxes(1f, 0f),
                Direction3D.Right);

            Assert.That(
                movement,
                Is.EqualTo(MovementInput.FromVector(new Displacement3D(1f, 0f, 0f))));
        }

        [Test]
        public void DiagonalInputIsClampedToUnitMagnitude()
        {
            MovementInput movement = ViewRelativeMovement.Translate(
                new MovementAxes(1f, 1f),
                Direction3D.Forward);

            Assert.That(movement.Magnitude, Is.EqualTo(1f).Within(0.0001f));
            Assert.That(movement.Vector.X, Is.EqualTo(0.7071f).Within(0.0001f));
            Assert.That(movement.Vector.Z, Is.EqualTo(0.7071f).Within(0.0001f));
        }

        [Test]
        public void ZeroAxesProduceZeroMovement()
        {
            MovementInput movement = ViewRelativeMovement.Translate(
                MovementAxes.Zero,
                Direction3D.Forward);

            Assert.That(movement, Is.EqualTo(MovementInput.Zero));
        }

        [Test]
        public void MovementIsAlwaysPlanar()
        {
            MovementInput movement = ViewRelativeMovement.Translate(
                new MovementAxes(0.5f, 0.5f),
                Direction3D.Left);

            Assert.That(movement.Vector.Y, Is.EqualTo(0f));
        }
    }
}
