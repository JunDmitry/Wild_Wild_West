using System;
using Game.Arena.Domain.Geometry;
using Game.Arena.Domain.Movement;
using NUnit.Framework;

namespace Game.Arena.Domain.Tests.Movement
{

    [TestFixture]
    public sealed class MovementPathPolicyTests
    {
        private MovementPathPolicy _policy;
        private PlanarMovementIntent _intent;

        [SetUp]
        public void SetUp()
        {
            _policy = new MovementPathPolicy();
            _intent = new PlanarMovementIntent(
                Position3D.Zero,
                Direction3D.Right,
                Distance.FromValue(5f));
        }

        [Test]
        public void OriginIsAccepted()
        {
            Assert.That(_policy.Validate(_intent, Position3D.Zero), Is.EqualTo(MovementPathVerdict.Accepted));
        }

        [Test]
        public void PartialTravelIsAccepted()
        {
            Assert.That(
                _policy.Validate(_intent, new Position3D(2.5f, 0f, 0f)),
                Is.EqualTo(MovementPathVerdict.Accepted));
        }

        [Test]
        public void FullTravelIsAccepted()
        {
            Assert.That(
                _policy.Validate(_intent, new Position3D(5f, 0f, 0f)),
                Is.EqualTo(MovementPathVerdict.Accepted));
        }

        [Test]
        public void TravelBeyondDistanceIsRejected()
        {
            Assert.That(
                _policy.Validate(_intent, new Position3D(5.1f, 0f, 0f)),
                Is.EqualTo(MovementPathVerdict.BeyondRequestedDistance));
        }

        [Test]
        public void TravelBehindOriginIsRejected()
        {
            Assert.That(
                _policy.Validate(_intent, new Position3D(-0.5f, 0f, 0f)),
                Is.EqualTo(MovementPathVerdict.BehindOrigin));
        }

        [Test]
        public void LateralTravelIsRejected()
        {
            Assert.That(
                _policy.Validate(_intent, new Position3D(2f, 0f, 0.5f)),
                Is.EqualTo(MovementPathVerdict.OffMovementPath));
        }

        [Test]
        public void TravelOffGroundPlaneIsRejected()
        {
            Assert.That(
                _policy.Validate(_intent, new Position3D(2f, 0.5f, 0f)),
                Is.EqualTo(MovementPathVerdict.OffGroundPlane));
        }

        [Test]
        public void GroundPlaneIsCheckedBeforeDirection()
        {
            Assert.That(
                _policy.Validate(_intent, new Position3D(-1f, 1f, 0f)),
                Is.EqualTo(MovementPathVerdict.OffGroundPlane));
        }

        [Test]
        public void DiagonalIntentAcceptsPointOnRay()
        {
            Direction3D diagonal = Direction3D.From(new Displacement3D(1f, 0f, 1f));
            PlanarMovementIntent intent = new(
                Position3D.Zero,
                diagonal,
                Distance.FromValue(4f));
            Position3D onRay = Position3D.Zero.MovedAlong(diagonal, Distance.FromValue(2f));

            Assert.That(_policy.Validate(intent, onRay), Is.EqualTo(MovementPathVerdict.Accepted));
        }

        [Test]
        public void InvalidIntentThrows()
        {
            Assert.Throws<ArgumentException>(
                () =>
                {
                    _policy.Validate(default, Position3D.Zero);
                });
        }
    }
}
