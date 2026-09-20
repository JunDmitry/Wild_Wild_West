using System;
using Game.Arena.Application.Ticks;
using Game.Arena.Application.Ticks.Stages;
using Game.Arena.Domain.Aggregates.ArenaRun;
using Game.Arena.Domain.Concurrency;
using Game.Arena.Domain.Geometry;
using Game.Arena.Domain.Interactions.Movement;
using Game.Arena.Domain.Time;
using NUnit.Framework;

namespace Game.Arena.Application.Tests.Ticks
{
    [TestFixture]
    public sealed class PendingInteractionRecoveryTests
    {
        private TickTestKit _kit;
        private PendingInteractionTracker _tracker;
        private PendingInteractionRecoveryStage _stage;
        private ArenaRunTickRecorder _recorder;

        [SetUp]
        public void SetUp()
        {
            _kit = new TickTestKit();
            _tracker = new PendingInteractionTracker();
            _stage = new PendingInteractionRecoveryStage(_tracker);
            _recorder = new ArenaRunTickRecorder();
        }

        [Test]
        public void NoPendingInteractionRecordsNoStage()
        {
            ArenaRun run = _kit.StartStandartRun();

            StageExecutionStatus status = _stage.Execute(run, _recorder);
            ArenaRunTickResult result = _recorder.Build(run.Id, run.Revision);

            Assert.That(status, Is.EqualTo(StageExecutionStatus.Completed));
            Assert.That(result.ExecutedStages, Is.Empty);
        }

        [Test]
        public void PendingInteractionIsCancelledAndStageIsRecorded()
        {
            ArenaRun run = _kit.StartStandartRun();
            PlayerMovementRequest request = OpenMovementInteraction(run);
            _tracker.Track(request.Correlation);

            _stage.Execute(run, _recorder);
            ArenaRunTickResult result = _recorder.Build(run.Id, run.Revision);

            Assert.That(run.HasPendingInteraction, Is.False);
            Assert.That(result.ExecutedStages[0], Is.EqualTo(ArenaRunTickStage.PendingInteractionCancellation));
        }

        [Test]
        public void CancellationProducesNoEventsAndNoRevisionChange()
        {
            ArenaRun run = _kit.StartStandartRun();
            PlayerMovementRequest request = OpenMovementInteraction(run);
            _tracker.Track(request.Correlation);
            AggregateRevision revisionBeforeRecovery = run.Revision;

            _stage.Execute(run, _recorder);
            ArenaRunTickResult result = _recorder.Build(run.Id, run.Revision);

            Assert.That(run.Revision, Is.EqualTo(revisionBeforeRecovery));
            Assert.That(result.DomainEvents, Is.Empty);
        }

        [Test]
        public void PendingInteractionWithoutTrackedCorrelationThrows()
        {
            ArenaRun run = _kit.StartStandartRun();
            OpenMovementInteraction(run);

            Assert.Throws<InvalidOperationException>(
                () =>
                {
                    _stage.Execute(run, _recorder);
                });
        }

        [Test]
        public void MismatchedCorrelationThrowsAndKeepsInteractionPending()
        {
            ArenaRun run = _kit.StartStandartRun();
            PlayerMovementRequest request = OpenMovementInteraction(run);

            _tracker.Track(
                new Domain.Interactions.InteractionCorrelation(
                    run.Id,
                    request.Correlation.InteractionId.Next(),
                    request.Correlation.AggregateRevision));

            Assert.Throws<InvalidOperationException>(
                () =>
                {
                    _stage.Execute(run, _recorder);
                });

            Assert.That(run.HasPendingInteraction, Is.True);
        }

        [Test]
        public void StaleTrackerIsClearedWhenNoInteractionIsPending()
        {
            ArenaRun run = _kit.StartStandartRun();
            PlayerMovementRequest request = OpenMovementInteraction(run);
            _tracker.Track(request.Correlation);

            run.ApplyPlayerMovement(
                new PlayerMovementResolution(
                    request.Correlation,
                    request.Intent.RequestedPosition));

            _stage.Execute(run, _recorder);

            Assert.That(_tracker.HasCorrelation, Is.False);
        }

        [Test]
        public void RecoveredRunAcceptsNewInteractionInSameStep()
        {
            ArenaRun run = _kit.StartStandartRun();
            PlayerMovementRequest first = OpenMovementInteraction(run);
            _tracker.Track(first.Correlation);

            _stage.Execute(run, _recorder);

            PlayerMovementRequestOutcome second = run.RequestPlayerMovement(
                MovementInput.FromVector(new Displacement3D(1f, 0f, 0f)),
                new GameDuration(0.1d));

            Assert.That(second.HasRequest, Is.True);
            Assert.That(second.Request.Correlation.InteractionId, Is.Not.EqualTo(first.Correlation.InteractionId));
        }

        private PlayerMovementRequest OpenMovementInteraction(ArenaRun run)
        {
            return run.RequestPlayerMovement(
                MovementInput.FromVector(new Displacement3D(1f, 0f, 0f)),
                new GameDuration(0.1d)).Request;
        }
    }
}
