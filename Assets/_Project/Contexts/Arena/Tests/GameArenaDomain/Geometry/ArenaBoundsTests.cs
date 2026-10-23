using System;
using Game.Arena.Domain.Geometry;
using NUnit.Framework;

namespace Game.Arena.Domain.Tests.Geometry
{
    [TestFixture]
    public sealed class ArenaBoundsTests
    {
        [Test]
        public void ConstructorRejectsInvalidHorizontalBounds()
        {
            Assert.Throws<ArgumentOutOfRangeException>(
                () =>
                {
                    ArenaBounds unused = new(10f, 10f, -10f, 10f, 0f);
                });
        }

        [Test]
        public void ContainsAccountsForCollisionRadius()
        {
            ArenaBounds bounds = new(-10f, 10f, -10f, 10f, 0f);

            CollisionRadius radius = CollisionRadius.FromValue(1f);

            Assert.That(bounds.Contains(new Position3D(9f, 0f, 0f), radius), Is.True);
            Assert.That(bounds.Contains(new Position3D(9.1f, 0f, 0f), radius), Is.False);
        }

        [Test]
        public void ContainsRejectsDifferentGroundHeight()
        {
            ArenaBounds bounds = new(-10f, 10f, -10f, 10f, 0f);

            CollisionRadius radius = CollisionRadius.FromValue(1f);

            bool contains = bounds.Contains(new Position3D(0f, 1f, 0f), radius);

            Assert.That(contains, Is.False);
        }

        [Test]
        public void CanContainRejectsRadiusLargerThanArena()
        {
            ArenaBounds bounds = new(-1f, 1f, -1f, 1f, 0f);

            CollisionRadius radius = CollisionRadius.FromValue(2f);

            Assert.That(bounds.CanContain(radius), Is.False);
        }

        [Test]
        public void PermittedMovementReturnsRequestedWhenBoundaryIsFar()
        {
            ArenaBounds bounds = new(-10f, 10f, -10f, 10f, 0f);
            CollisionRadius radius = CollisionRadius.FromValue(1f);

            BoundedMovement movement = bounds.PermittedMovement(
                Position3D.Zero,
                Direction3D.Back,
                radius,
                Distance.FromValue(3f));

            Assert.That(movement.Direction, Is.EqualTo(Direction3D.Back));
            Assert.That(movement.Distance.Value, Is.EqualTo(3f));
        }

        [Test]
        public void PermittedMovementApproachesBoundaryAlongDirection()
        {
            ArenaBounds bounds = new(-10f, 10f, -10f, 10f, 0f);
            CollisionRadius radius = CollisionRadius.FromValue(1f);

            BoundedMovement movement = bounds.PermittedMovement(
                new Position3D(7f, 0f, 0f),
                Direction3D.Right,
                radius,
                Distance.FromValue(50f));

            Assert.That(movement.Direction, Is.EqualTo(Direction3D.Right));
            Assert.That(movement.Distance.Value, Is.EqualTo(2f).Within(0.0001f));
        }

        [Test]
        public void PermittedMovementIsZeroWhenPushedStraightOutOfBounds()
        {
            ArenaBounds bounds = new(-10f, 10f, -10f, 10f, 0f);
            CollisionRadius radius = CollisionRadius.FromValue(1f);

            BoundedMovement movement = bounds.PermittedMovement(
                new Position3D(0f, 0f, -9f),
                Direction3D.Back,
                radius,
                Distance.FromValue(3f));

            Assert.That(movement.IsZero, Is.True);
        }

        [Test]
        public void PermittedMovementSlidesAlongBoundary()
        {
            ArenaBounds bounds = new(-10f, 10f, -10f, 10f, 0f);
            CollisionRadius radius = CollisionRadius.FromValue(1f);
            Direction3D diagonal = Direction3D.From(new Displacement3D(1f, 0f, 1f));

            BoundedMovement movement = bounds.PermittedMovement(
                new Position3D(9f, 0f, 0f),
                diagonal,
                radius,
                Distance.FromValue(10f));

            Assert.That(movement.Direction, Is.EqualTo(Direction3D.Forward));
            Assert.That(movement.Distance.Value, Is.EqualTo(10f * 0.70710678f).Within(0.001f));

            Position3D destination = new Position3D(9f, 0f, 0f)
                .MovedAlong(movement.Direction, movement.Distance);

            Assert.That(destination.X, Is.EqualTo(9f).Within(0.001f));
            Assert.That(destination.Z, Is.EqualTo(7.071f).Within(0.001f));
            Assert.That(bounds.Contains(destination, radius), Is.True);
        }

        [Test]
        public void PermittedMovementClampsSlideToRemainingBoundary()
        {
            ArenaBounds bounds = new(-10f, 10f, -10f, 10f, 0f);
            CollisionRadius radius = CollisionRadius.FromValue(1f);
            Direction3D diagonal = Direction3D.From(new Displacement3D(1f, 0f, 1f));

            BoundedMovement movement = bounds.PermittedMovement(
                new Position3D(9f, 0f, 8f),
                diagonal,
                radius,
                Distance.FromValue(10f));

            Assert.That(movement.Direction, Is.EqualTo(Direction3D.Forward));
            Assert.That(movement.Distance.Value, Is.EqualTo(1f).Within(0.0001f));

            Position3D destination = new Position3D(9f, 0f, 8f)
                .MovedAlong(movement.Direction, movement.Distance);

            Assert.That(bounds.Contains(destination, radius), Is.True);
        }

        [Test]
        public void PermittedMovementIsZeroForDiagonalIntoCorner()
        {
            ArenaBounds bounds = new(-10f, 10f, -10f, 10f, 0f);
            CollisionRadius radius = CollisionRadius.FromValue(1f);
            Direction3D diagonal = Direction3D.From(new Displacement3D(1f, 0f, 1f));

            BoundedMovement movement = bounds.PermittedMovement(
                new Position3D(9f, 0f, 9f),
                diagonal,
                radius,
                Distance.FromValue(10f));

            Assert.That(movement.IsZero, Is.True);
        }

        [Test]
        public void PermittedMovementKeepsDirectionNearBoundary()
        {
            ArenaBounds bounds = new(-10f, 10f, -10f, 10f, 0f);
            CollisionRadius radius = CollisionRadius.FromValue(1f);
            Direction3D diagonal = Direction3D.From(new Displacement3D(1f, 0f, 1f));

            BoundedMovement movement = bounds.PermittedMovement(
                new Position3D(8f, 0f, 0f),
                diagonal,
                radius,
                Distance.FromValue(1.4f));

            Assert.That(movement.Direction, Is.EqualTo(diagonal));
            Assert.That(movement.Distance.Value, Is.EqualTo(1.4f).Within(0.0001f));
        }
    }
}
