using System;
using Game.Arena.Domain.Aggregates.ArenaRun;
using Game.Arena.Domain.Concurrency;
using Game.Arena.Domain.Geometry;
using Game.Arena.Domain.Identity;
using Game.Arena.Domain.Interactions;
using Game.Arena.Domain.Interactions.Movement;
using Game.Arena.Domain.Time;
using NUnit.Framework;

namespace Game.Arena.Domain.Tests.Aggregates.ArenaRun
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
            Game.Arena.Domain.Aggregates.ArenaRun.ArenaRun run = CreateRun();
            MovementInput input = MovementInput.FromVector(new Displacement3D(1f, 0f, 0f));

            PlayerMovementRequestOutcome outcome = run.RequestPlayerMovement(input, new GameDuration(1d));

            Assert.That(outcome.HasRequest, Is.True);
            Assert.That(run.Revision, Is.EqualTo(AggregateRevision.Initial));
            Assert.That(outcome.Request.Correlation.AggregateRevision, Is.EqualTo(AggregateRevision.Initial));
            Assert.That(outcome.Request.Correlation.ArenaRunId, Is.EqualTo(run.Id));
            Assert.That(outcome.Request.Correlation.InteractionId.Value, Is.EqualTo(1UL));
        }

        [Test]
        public void ResolvedMovementUpdatesPlayerPosition()
        {
            Game.Arena.Domain.Aggregates.ArenaRun.ArenaRun run = CreateRun();
            MovementInput input = MovementInput.FromVector(new Displacement3D(1f, 0f, 0f));

            PlayerMovementRequestOutcome requestOutcome = run.RequestPlayerMovement(input, new GameDuration(1d));
            PlayerMovementResolution resolution = new(requestOutcome.Request.Correlation, requestOutcome.Request.RequestedPosition);

            PlayerMovementResolutionOutcome resolutionOutcome = run.ApplyPlayerMovement(resolution);

            Assert.That(resolutionOutcome.Status, Is.EqualTo(PlayerMovementResolutionStatus.Applied));
            Assert.That(run.PlayerPosition, Is.EqualTo(new Position3D(5f, 0f, 0f)));
            Assert.That(resolutionOutcome.Change.HasStateChange, Is.True);
            Assert.That(resolutionOutcome.Change.DomainEvents, Is.Empty);
        }

        [Test]
        public void AcceptedStateChangingResolutionAdvancesRevision()
        {
            Game.Arena.Domain.Aggregates.ArenaRun.ArenaRun run = CreateRun();
            MovementInput input = MovementInput.FromVector(new Displacement3D(1f, 0f, 0f));

            PlayerMovementRequestOutcome requestOutcome = run.RequestPlayerMovement(input, new GameDuration(1d));
            PlayerMovementResolution resolution = new(requestOutcome.Request.Correlation, requestOutcome.Request.RequestedPosition);
            PlayerMovementResolutionOutcome resolutionOutcome = run.ApplyPlayerMovement(resolution);

            Assert.That(run.Revision.Value, Is.EqualTo(1UL));
            Assert.That(resolutionOutcome.Change.Revision, Is.EqualTo(run.Revision));
            Assert.That(resolutionOutcome.Change.HasStateChange, Is.True);
        }

        [Test]
        public void MovementCannotCrossArenaBoundary()
        {
            Game.Arena.Domain.Aggregates.ArenaRun.ArenaRun run = CreateRun(new Position3D(19f, 0f, 0f));
            MovementInput input = MovementInput.FromVector(new Displacement3D(1f, 0f, 0f));

            PlayerMovementRequestOutcome outcome = run.RequestPlayerMovement(input, new GameDuration(10d));

            Assert.That(outcome.HasRequest, Is.True);
            Assert.That(outcome.Request.RequestedPosition, Is.EqualTo(new Position3D(19.5f, 0f, 0f)));
        }

        [Test]
        public void RequestAtBoundaryWithoutPositionChangeReturnsPositionUnchanged()
        {
            Game.Arena.Domain.Aggregates.ArenaRun.ArenaRun run = CreateRun(new Position3D(19.5f, 0f, 0f));
            MovementInput input = MovementInput.FromVector(new Displacement3D(1f, 0f, 0f));

            PlayerMovementRequestOutcome outcome = run.RequestPlayerMovement(input, new GameDuration(1d));

            Assert.That(outcome.Status, Is.EqualTo(PlayerMovementRequestStatus.PositionUnchanged));
            Assert.That(outcome.HasRequest, Is.False);
            Assert.That(run.Revision, Is.EqualTo(AggregateRevision.Initial));
        }

        [Test]
        public void ZeroMovementInputDoesNotOpenInteraction()
        {
            Game.Arena.Domain.Aggregates.ArenaRun.ArenaRun run = CreateRun();

            PlayerMovementRequestOutcome firstOutcome = run.RequestPlayerMovement(MovementInput.Zero, new GameDuration(1d));
            MovementInput movementInput = MovementInput.FromVector(new Displacement3D(1f, 0f, 0f));
            PlayerMovementRequestOutcome secondOutcome = run.RequestPlayerMovement(movementInput, new GameDuration(1d));

            Assert.That(firstOutcome.Status, Is.EqualTo(PlayerMovementRequestStatus.NoMovement));
            Assert.That(secondOutcome.HasRequest, Is.True);
            Assert.That(secondOutcome.Request.Correlation.InteractionId.Value, Is.EqualTo(1UL));
        }

        [Test]
        public void OpenWhilePendingThrowsThroughArenaRun()
        {
            Game.Arena.Domain.Aggregates.ArenaRun.ArenaRun run = CreateRun();
            MovementInput input = MovementInput.FromVector(new Displacement3D(1f, 0f, 0f));

            run.RequestPlayerMovement(input, new GameDuration(1d));

            Assert.Throws<InvalidOperationException>(
                () =>
                {
                    run.RequestPlayerMovement(input, new GameDuration(1d));
                });
        }

        [Test]
        public void AcceptedResolutionWithoutMovementDoesNotAdvanceRevision()
        {
            Game.Arena.Domain.Aggregates.ArenaRun.ArenaRun run = CreateRun();
            MovementInput input = MovementInput.FromVector(new Displacement3D(1f, 0f, 0f));

            PlayerMovementRequestOutcome requestOutcome = run.RequestPlayerMovement(input, new GameDuration(1d));
            PlayerMovementResolution blockedResolution = new(requestOutcome.Request.Correlation, requestOutcome.Request.From);
            PlayerMovementResolutionOutcome resolutionOutcome = run.ApplyPlayerMovement(blockedResolution);

            Assert.That(resolutionOutcome.Status, Is.EqualTo(PlayerMovementResolutionStatus.AcceptedWithoutStateChange));
            Assert.That(run.PlayerPosition, Is.EqualTo(Position3D.Zero));
            Assert.That(run.Revision, Is.EqualTo(AggregateRevision.Initial));
            Assert.That(resolutionOutcome.Change.HasStateChange, Is.False);

            PlayerMovementRequestOutcome nextRequest = run.RequestPlayerMovement(input, new GameDuration(1d));

            Assert.That(nextRequest.Request.Correlation.InteractionId.Value, Is.EqualTo(2UL));
        }

        [Test]
        public void DuplicateInteractionResolutionIsRejected()
        {
            Game.Arena.Domain.Aggregates.ArenaRun.ArenaRun run = CreateRun();
            MovementInput input = MovementInput.FromVector(new Displacement3D(1f, 0f, 0f));

            PlayerMovementRequestOutcome requestOutcome = run.RequestPlayerMovement(input, new GameDuration(1d));

            PlayerMovementResolution resolution = new(requestOutcome.Request.Correlation, requestOutcome.Request.RequestedPosition);

            run.ApplyPlayerMovement(resolution);
            AggregateRevision revisionAfterFirstApply = run.Revision;

            PlayerMovementResolutionOutcome duplicateOutcome = run.ApplyPlayerMovement(resolution);

            Assert.That(duplicateOutcome.Status, Is.EqualTo(PlayerMovementResolutionStatus.Rejected));
            Assert.That(duplicateOutcome.RejectionReason, Is.EqualTo(PlayerMovementResolutionRejectionReason.InteractionClosed));
            Assert.That(run.Revision, Is.EqualTo(revisionAfterFirstApply));
            Assert.That(duplicateOutcome.Change.HasStateChange, Is.False);
        }

        [Test]
        public void ResolutionForAnotherArenaRunIsRejected()
        {
            Game.Arena.Domain.Aggregates.ArenaRun.ArenaRun run = CreateRun();
            MovementInput input = MovementInput.FromVector(new Displacement3D(1f, 0f, 0f));

            PlayerMovementRequestOutcome requestOutcome = run.RequestPlayerMovement(input, new GameDuration(1d));
            InteractionCorrelation foreignCorrelation = new(ArenaRunId.FromValue(99UL), requestOutcome.Request.Correlation.InteractionId, requestOutcome.Request.Correlation.AggregateRevision);
            PlayerMovementResolution foreignResolution = new(foreignCorrelation, requestOutcome.Request.RequestedPosition);
            PlayerMovementResolutionOutcome rejectedOutcome = run.ApplyPlayerMovement(foreignResolution);

            Assert.That(rejectedOutcome.Status, Is.EqualTo(PlayerMovementResolutionStatus.Rejected));
            Assert.That(rejectedOutcome.RejectionReason, Is.EqualTo(PlayerMovementResolutionRejectionReason.ForeignArenaRun));
            Assert.That(run.Revision, Is.EqualTo(AggregateRevision.Initial));
            Assert.That(run.PlayerPosition, Is.EqualTo(Position3D.Zero));

            PlayerMovementResolution validResolution = new(requestOutcome.Request.Correlation, requestOutcome.Request.RequestedPosition);
            PlayerMovementResolutionOutcome acceptedOutcome = run.ApplyPlayerMovement(validResolution);

            Assert.That(acceptedOutcome.Status, Is.EqualTo(PlayerMovementResolutionStatus.Applied));
        }

        [Test]
        public void RejectedInteractionResolutionDoesNotChangeState()
        {
            Game.Arena.Domain.Aggregates.ArenaRun.ArenaRun run = CreateRun();
            MovementInput input = MovementInput.FromVector(new Displacement3D(1f, 0f, 0f));

            PlayerMovementRequestOutcome requestOutcome = run.RequestPlayerMovement(input, new GameDuration(1d));
            PlayerMovementResolution invalidResolution = new(requestOutcome.Request.Correlation, new Position3D(100f, 0f, 0f));
            PlayerMovementResolutionOutcome outcome = run.ApplyPlayerMovement(invalidResolution);

            Assert.That(outcome.Status, Is.EqualTo(PlayerMovementResolutionStatus.Rejected));
            Assert.That(outcome.RejectionReason, Is.EqualTo(PlayerMovementResolutionRejectionReason.AcceptedPositionOutsideArena));
            Assert.That(run.PlayerPosition, Is.EqualTo(Position3D.Zero));
        }

        [Test]
        public void RejectedInteractionResolutionDoesNotAdvanceRevision()
        {
            Game.Arena.Domain.Aggregates.ArenaRun.ArenaRun run = CreateRun();
            MovementInput input = MovementInput.FromVector(new Displacement3D(1f, 0f, 0f));

            PlayerMovementRequestOutcome requestOutcome = run.RequestPlayerMovement(input, new GameDuration(1d));
            PlayerMovementResolution invalidResolution = new(requestOutcome.Request.Correlation, new Position3D(100f, 0f, 0f));
            PlayerMovementResolutionOutcome outcome = run.ApplyPlayerMovement(invalidResolution);

            Assert.That(run.Revision, Is.EqualTo(AggregateRevision.Initial));
            Assert.That(outcome.Change.HasStateChange, Is.False);
        }

        [Test]
        public void InvalidMovementResolutionClosesPendingInteraction()
        {
            Game.Arena.Domain.Aggregates.ArenaRun.ArenaRun run = CreateRun();
            MovementInput input = MovementInput.FromVector(new Displacement3D(1f, 0f, 0f));
            PlayerMovementRequestOutcome requestOutcome = run.RequestPlayerMovement(input, new GameDuration(1d));
            PlayerMovementResolution invalidResolution = new(requestOutcome.Request.Correlation, new Position3D(100f, 0f, 0f));

            run.ApplyPlayerMovement(invalidResolution);
            PlayerMovementResolution validResolution = new(requestOutcome.Request.Correlation, requestOutcome.Request.RequestedPosition);
            PlayerMovementResolutionOutcome secondOutcome = run.ApplyPlayerMovement(validResolution);

            Assert.That(secondOutcome.Status, Is.EqualTo(PlayerMovementResolutionStatus.Rejected));
            Assert.That(secondOutcome.RejectionReason, Is.EqualTo(PlayerMovementResolutionRejectionReason.InteractionClosed));
        }

        private Game.Arena.Domain.Aggregates.ArenaRun.ArenaRun CreateRun()
        {
            return CreateRun(Position3D.Zero);
        }

        private Game.Arena.Domain.Aggregates.ArenaRun.ArenaRun CreateRun(Position3D startPosition)
        {
            return _factory.Start(ArenaRunId.FromValue(1UL), PlayerId.FromValue(2UL), startPosition, _playerSpeed, _playerRadius, _arenaBounds);
        }
    }
}
