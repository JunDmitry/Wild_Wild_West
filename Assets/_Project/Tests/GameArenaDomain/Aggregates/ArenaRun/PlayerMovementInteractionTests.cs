using System;
using Game.Arena.Domain.Aggregates.ArenaRun;
using Game.Arena.Domain.Concurrency;
using Game.Arena.Domain.Geometry;
using Game.Arena.Domain.Identity;
using Game.Arena.Domain.Interactions;
using Game.Arena.Domain.Interactions.Movement;
using Game.Arena.Domain.Time;
using NUnit.Framework;

namespace Game.Arena.Domain.Tests.Aggregates._ArenaRun
{
    [TestFixture]
    public sealed class PlayerMovementInteractionTests
    {
        private ArenaBounds _arenaBounds;
        private ArenaRunFactory _factory;
        private CollisionRadius _playerRadius;
        private MovementSpeed _playerSpeed;

        [SetUp]
        public void SetUp()
        {
            _factory = new ArenaRunFactory();
            _arenaBounds = new ArenaBounds(-20f, 20f, -20f, 20f, 0f);
            _playerRadius = CollisionRadius.FromValue(0.5f);
            _playerSpeed = MovementSpeed.FromUnitsPerSecond(5f);
        }

        [Test]
        public void CreatingInteractionRequestDoesNotAdvanceRevision()
        {
            ArenaRun run = CreateRun();

            PlayerMovementRequestOutcome outcome = RequestRight(run, 1d);

            Assert.That(outcome.HasRequest, Is.True);
            Assert.That(run.Revision, Is.EqualTo(AggregateRevision.Initial));
            Assert.That(outcome.Request.Correlation.ArenaRunId, Is.EqualTo(run.Id));
            Assert.That(outcome.Request.Correlation.InteractionId.Value, Is.EqualTo(1UL));
            Assert.That(outcome.Request.Correlation.AggregateRevision, Is.EqualTo(AggregateRevision.Initial));
        }

        [Test]
        public void RequestExpressesIntentAndComputesRequestedPosition()
        {
            ArenaRun run = CreateRun();

            PlayerMovementRequest request = RequestRight(run, 1d).Request;

            Assert.That(request.From, Is.EqualTo(Position3D.Zero));
            Assert.That(request.Direction, Is.EqualTo(Direction3D.Right));
            Assert.That(request.RequestedDistance.Value, Is.EqualTo(5f));
            Assert.That(request.RequestedPosition, Is.EqualTo(new Position3D(5f, 0f, 0f)));
        }

        [Test]
        public void ResolvedMovementUpdatesPlayerPosition()
        {
            ArenaRun run = CreateRun();
            PlayerMovementRequest request = RequestRight(run, 1d).Request;

            PlayerMovementResolutionOutcome outcome = run.ApplyPlayerMovement(
                new PlayerMovementResolution(request.Correlation, request.RequestedPosition));

            Assert.That(outcome.Status, Is.EqualTo(PlayerMovementResolutionStatus.Applied));
            Assert.That(run.PlayerPosition, Is.EqualTo(new Position3D(5f, 0f, 0f)));
            Assert.That(outcome.Change.HasStateChange, Is.True);
            Assert.That(outcome.Change.DomainEvents, Is.Empty);
            Assert.That(run.HasPendingInteraction, Is.False);
        }

        [Test]
        public void AcceptedStateChangingResolutionAdvancesRevision()
        {
            ArenaRun run = CreateRun();
            PlayerMovementRequest request = RequestRight(run, 1d).Request;

            PlayerMovementResolutionOutcome outcome = run.ApplyPlayerMovement(
                new PlayerMovementResolution(request.Correlation, request.RequestedPosition));

            Assert.That(run.Revision.Value, Is.EqualTo(1UL));
            Assert.That(outcome.Change.Revision, Is.EqualTo(run.Revision));
        }

        [Test]
        public void AcceptedPositionAtPartialRequestedDistanceIsApplied()
        {
            ArenaRun run = CreateRun();
            PlayerMovementRequest request = RequestRight(run, 1d).Request;

            PlayerMovementResolutionOutcome outcome = run.ApplyPlayerMovement(
                new PlayerMovementResolution(request.Correlation, new Position3D(1.5f, 0f, 0f)));

            Assert.That(outcome.Status, Is.EqualTo(PlayerMovementResolutionStatus.Applied));
            Assert.That(run.PlayerPosition, Is.EqualTo(new Position3D(1.5f, 0f, 0f)));
        }

        [Test]
        public void AcceptedPositionAtOriginIsAcceptedWithoutStateChange()
        {
            ArenaRun run = CreateRun();
            PlayerMovementRequest request = RequestRight(run, 1d).Request;

            PlayerMovementResolutionOutcome outcome = run.ApplyPlayerMovement(
                new PlayerMovementResolution(request.Correlation, request.From));

            Assert.That(outcome.Status, Is.EqualTo(PlayerMovementResolutionStatus.AcceptedWithoutStateChange));
            Assert.That(run.Revision, Is.EqualTo(AggregateRevision.Initial));
            Assert.That(outcome.Change.HasStateChange, Is.False);
            Assert.That(run.HasPendingInteraction, Is.False);
        }

        [Test]
        public void AcceptedPositionBeyondRequestedDistanceIsRejected()
        {
            ArenaRun run = CreateRun();
            PlayerMovementRequest request = RequestRight(run, 1d).Request;

            PlayerMovementResolutionOutcome outcome = run.ApplyPlayerMovement(
                new PlayerMovementResolution(request.Correlation, new Position3D(10f, 0f, 0f)));

            Assert.That(
                outcome.RejectionReason,
                Is.EqualTo(PlayerMovementResolutionRejectionReason.AcceptedPositionBeyondRequestedDistance));
        }

        [Test]
        public void AcceptedPositionBehindRequestedDirectionIsRejected()
        {
            ArenaRun run = CreateRun();
            PlayerMovementRequest request = RequestRight(run, 1d).Request;

            PlayerMovementResolutionOutcome outcome = run.ApplyPlayerMovement(
                new PlayerMovementResolution(request.Correlation, new Position3D(-1f, 0f, 0f)));

            Assert.That(
                outcome.RejectionReason,
                Is.EqualTo(PlayerMovementResolutionRejectionReason.AcceptedPositionBehindRequest));
        }

        [Test]
        public void AcceptedPositionOutsideRequestedMovementRayIsRejected()
        {
            ArenaRun run = CreateRun();
            PlayerMovementRequest request = RequestRight(run, 1d).Request;

            PlayerMovementResolutionOutcome outcome = run.ApplyPlayerMovement(
                new PlayerMovementResolution(request.Correlation, new Position3D(1f, 0f, 1f)));

            Assert.That(
                outcome.RejectionReason,
                Is.EqualTo(PlayerMovementResolutionRejectionReason.AcceptedPositionOffMovementPath));
        }

        [Test]
        public void RejectedPayloadResolutionDoesNotClosePendingInteraction()
        {
            ArenaRun run = CreateRun();
            PlayerMovementRequest request = RequestRight(run, 1d).Request;

            run.ApplyPlayerMovement(
                new PlayerMovementResolution(request.Correlation, new Position3D(10f, 0f, 0f)));

            Assert.That(run.HasPendingInteraction, Is.True);
        }

        [Test]
        public void ValidResolutionCanBeAppliedAfterRejectedPayloadResolution()
        {
            ArenaRun run = CreateRun();
            PlayerMovementRequest request = RequestRight(run, 1d).Request;

            run.ApplyPlayerMovement(
                new PlayerMovementResolution(request.Correlation, new Position3D(10f, 0f, 0f)));

            PlayerMovementResolutionOutcome outcome = run.ApplyPlayerMovement(
                new PlayerMovementResolution(request.Correlation, request.RequestedPosition));

            Assert.That(outcome.Status, Is.EqualTo(PlayerMovementResolutionStatus.Applied));
            Assert.That(run.PlayerPosition, Is.EqualTo(new Position3D(5f, 0f, 0f)));
        }

        [Test]
        public void InvalidMovementResolutionDoesNotAdvanceAggregateRevision()
        {
            ArenaRun run = CreateRun();
            PlayerMovementRequest request = RequestRight(run, 1d).Request;

            PlayerMovementResolutionOutcome outcome = run.ApplyPlayerMovement(
                new PlayerMovementResolution(request.Correlation, new Position3D(10f, 0f, 0f)));

            Assert.That(run.Revision, Is.EqualTo(AggregateRevision.Initial));
            Assert.That(outcome.Change.HasStateChange, Is.False);
        }

        [Test]
        public void InvalidMovementResolutionDoesNotProduceDomainEvents()
        {
            ArenaRun run = CreateRun();
            PlayerMovementRequest request = RequestRight(run, 1d).Request;

            PlayerMovementResolutionOutcome outcome = run.ApplyPlayerMovement(
                new PlayerMovementResolution(request.Correlation, new Position3D(1f, 0f, 1f)));

            Assert.That(outcome.Change.DomainEvents, Is.Empty);
            Assert.That(run.PlayerPosition, Is.EqualTo(Position3D.Zero));
        }

        [Test]
        public void CancelledInteractionResolutionIsRejected()
        {
            ArenaRun run = CreateRun();
            PlayerMovementRequest request = RequestRight(run, 1d).Request;

            InteractionCancellationOutcome cancellation = run.CancelPendingInteraction(
                request.Correlation,
                InteractionCancellationReason.ExternalResolutionTimeout);

            PlayerMovementResolutionOutcome outcome = run.ApplyPlayerMovement(
                new PlayerMovementResolution(request.Correlation, request.RequestedPosition));

            Assert.That(cancellation.IsCancelled, Is.True);
            Assert.That(run.HasPendingInteraction, Is.False);
            Assert.That(run.Revision, Is.EqualTo(AggregateRevision.Initial));
            Assert.That(
                outcome.RejectionReason,
                Is.EqualTo(PlayerMovementResolutionRejectionReason.InteractionClosed));
        }

        [Test]
        public void CancelWithoutPendingInteractionReportsNoPending()
        {
            ArenaRun run = CreateRun();
            InteractionCorrelation correlation = new InteractionCorrelation(
                run.Id,
                InteractionId.None.Next(),
                AggregateRevision.Initial);

            InteractionCancellationOutcome outcome = run.CancelPendingInteraction(
                correlation,
                InteractionCancellationReason.ApplicationShutdown);

            Assert.That(outcome.Status, Is.EqualTo(InteractionCancellationStatus.NoPendingInteraction));
        }

        [Test]
        public void CancelWithMismatchedCorrelationKeepsPending()
        {
            ArenaRun run = CreateRun();
            PlayerMovementRequest request = RequestRight(run, 1d).Request;
            InteractionCorrelation mismatched = new InteractionCorrelation(
                run.Id,
                request.Correlation.InteractionId.Next(),
                request.Correlation.AggregateRevision);

            InteractionCancellationOutcome outcome = run.CancelPendingInteraction(
                mismatched,
                InteractionCancellationReason.SupersededByLifecycle);

            Assert.That(outcome.Status, Is.EqualTo(InteractionCancellationStatus.CorrelationMismatch));
            Assert.That(run.HasPendingInteraction, Is.True);
        }

        [Test]
        public void MovementCannotCrossArenaBoundary()
        {
            ArenaRun run = CreateRun(new Position3D(19f, 0f, 0f));

            PlayerMovementRequestOutcome outcome = RequestRight(run, 10d);

            Assert.That(outcome.HasRequest, Is.True);
            Assert.That(outcome.Request.RequestedDistance.Value, Is.EqualTo(0.5f).Within(0.0001f));
            Assert.That(outcome.Request.RequestedPosition.X, Is.EqualTo(19.5f).Within(0.0001f));
        }

        [Test]
        public void RequestAtBoundaryWithoutPositionChangeReturnsPositionUnchanged()
        {
            ArenaRun run = CreateRun(new Position3D(19.5f, 0f, 0f));

            PlayerMovementRequestOutcome outcome = RequestRight(run, 1d);

            Assert.That(outcome.Status, Is.EqualTo(PlayerMovementRequestStatus.PositionUnchanged));
            Assert.That(run.HasPendingInteraction, Is.False);
        }

        [Test]
        public void ZeroMovementInputDoesNotOpenInteraction()
        {
            ArenaRun run = CreateRun();

            PlayerMovementRequestOutcome zeroOutcome = run.RequestPlayerMovement(
                MovementInput.Zero,
                new GameDuration(1d));

            PlayerMovementRequestOutcome nextOutcome = RequestRight(run, 1d);

            Assert.That(zeroOutcome.Status, Is.EqualTo(PlayerMovementRequestStatus.NoMovement));
            Assert.That(nextOutcome.Request.Correlation.InteractionId.Value, Is.EqualTo(1UL));
        }

        [Test]
        public void OpenWhilePendingThrowsThroughArenaRun()
        {
            ArenaRun run = CreateRun();
            RequestRight(run, 1d);

            Assert.Throws<InvalidOperationException>(
                () =>
                {
                    RequestRight(run, 1d);
                });
        }

        [Test]
        public void DuplicateInteractionResolutionIsRejected()
        {
            ArenaRun run = CreateRun();
            PlayerMovementRequest request = RequestRight(run, 1d).Request;
            PlayerMovementResolution resolution = new PlayerMovementResolution(
                request.Correlation,
                request.RequestedPosition);

            run.ApplyPlayerMovement(resolution);
            AggregateRevision revisionAfterApply = run.Revision;
            PlayerMovementResolutionOutcome duplicate = run.ApplyPlayerMovement(resolution);

            Assert.That(
                duplicate.RejectionReason,
                Is.EqualTo(PlayerMovementResolutionRejectionReason.InteractionClosed));
            Assert.That(run.Revision, Is.EqualTo(revisionAfterApply));
        }

        [Test]
        public void ResolutionForAnotherArenaRunIsRejected()
        {
            ArenaRun run = CreateRun();
            PlayerMovementRequest request = RequestRight(run, 1d).Request;
            InteractionCorrelation foreign = new InteractionCorrelation(
                ArenaRunId.FromValue(99UL),
                request.Correlation.InteractionId,
                request.Correlation.AggregateRevision);

            PlayerMovementResolutionOutcome outcome = run.ApplyPlayerMovement(
                new PlayerMovementResolution(foreign, request.RequestedPosition));

            Assert.That(
                outcome.RejectionReason,
                Is.EqualTo(PlayerMovementResolutionRejectionReason.ForeignArenaRun));
            Assert.That(run.HasPendingInteraction, Is.True);
            Assert.That(run.PlayerPosition, Is.EqualTo(Position3D.Zero));
        }

        [Test]
        public void StaleInteractionResolutionIsRejected()
        {
            ArenaRun run = CreateRun();
            PlayerMovementRequest request = RequestRight(run, 1d).Request;
            InteractionCorrelation stale = new InteractionCorrelation(
                run.Id,
                request.Correlation.InteractionId,
                request.Correlation.AggregateRevision.Next());

            PlayerMovementResolutionOutcome outcome = run.ApplyPlayerMovement(
                new PlayerMovementResolution(stale, request.RequestedPosition));

            Assert.That(
                outcome.RejectionReason,
                Is.EqualTo(PlayerMovementResolutionRejectionReason.StaleRevision));
        }

        [Test]
        public void SecondRequestUsesUpdatedPositionAndRevision()
        {
            ArenaRun run = CreateRun();
            PlayerMovementRequest first = RequestRight(run, 1d).Request;
            run.ApplyPlayerMovement(new PlayerMovementResolution(first.Correlation, first.RequestedPosition));

            PlayerMovementRequest second = RequestRight(run, 1d).Request;

            Assert.That(second.From, Is.EqualTo(new Position3D(5f, 0f, 0f)));
            Assert.That(second.Correlation.InteractionId.Value, Is.EqualTo(2UL));
            Assert.That(second.Correlation.AggregateRevision.Value, Is.EqualTo(1UL));
        }

        private PlayerMovementRequestOutcome RequestRight(ArenaRun run, double seconds)
        {
            MovementInput input = MovementInput.FromVector(new Displacement3D(1f, 0f, 0f));

            return run.RequestPlayerMovement(input, new GameDuration(seconds));
        }

        private ArenaRun CreateRun()
        {
            return CreateRun(Position3D.Zero);
        }

        private ArenaRun CreateRun(Position3D startPosition)
        {
            return _factory.Start(
                ArenaRunId.FromValue(1UL),
                PlayerId.FromValue(2UL),
                startPosition,
                _playerSpeed,
                _playerRadius,
                _arenaBounds);
        }
    }
}
