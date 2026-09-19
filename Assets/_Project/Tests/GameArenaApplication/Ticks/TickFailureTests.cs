using System;
using Game.Arena.Application.Input;
using Game.Arena.Application.Ticks;
using Game.Arena.Domain.Aggregates.ArenaRun;
using Game.Arena.Domain.Concurrency;
using Game.Arena.Domain.Events;
using Game.Arena.Domain.Geometry;
using Game.Arena.Domain.Time;
using NUnit.Framework;

namespace Game.Arena.Application.Tests.Ticks
{
    [TestFixture]
    public sealed class TickFailureTests
    {
        private static readonly GameDuration s_delta = new GameDuration(0.1d);

        private TickTestKit _kit;
        private PendingInteractionTracker _tracker;
        private ScriptedPlayerMovementResolver _movement;
        private ScriptedTargetingResolver _targeting;
        private PlayerTickPhase _phase;
        private ArenaRunTickRecorder _recorder;

        [SetUp]
        public void SetUp()
        {
            _kit = new TickTestKit();
            _tracker = new PendingInteractionTracker();
            _movement = new ScriptedPlayerMovementResolver();
            _targeting = new ScriptedTargetingResolver();
            _recorder = new ArenaRunTickRecorder();
            _phase = new PlayerTickPhase(_movement, _targeting, _tracker);
        }

        [Test]
        public void EventsBeforeFailureAreRetainedInRecorder()
        {
            ArenaRun run = _kit.StartStandardRun();
            _targeting.ReturnNull = true;

            PlayerFrameInput input = _kit.Input(
                Displacement3D.Zero,
                Direction3D.Right,
                true,
                false);

            Assert.Throws<InvalidOperationException>(
                () =>
                {
                    _phase.Execute(run, input, s_delta, _recorder);
                });

            ArenaRunTickResult partial = _recorder.Build(run.Id, run.Revision);

            Assert.That(partial.DomainEvents.Count, Is.GreaterThan(0));
            Assert.That(partial.DomainEvents[partial.DomainEvents.Count - 1], Is.TypeOf<PlayerAttackStarted>());
        }

        [Test]
        public void PartialResultRevisionMatchesAggregateAfterFailure()
        {
            ArenaRun run = _kit.StartStandardRun();
            _targeting.ReturnNull = true;

            PlayerFrameInput input = _kit.Input(
                Displacement3D.Zero,
                Direction3D.Right,
                true,
                false);

            Assert.Throws<InvalidOperationException>(
                () =>
                {
                    _phase.Execute(run, input, s_delta, _recorder);
                });

            ArenaRunTickResult partial = _recorder.Build(run.Id, run.Revision);

            Assert.That(partial.FinalRevision, Is.EqualTo(run.Revision));
        }

        [Test]
        public void FailureLeavesInteractionPendingForNextStepRecovery()
        {
            ArenaRun run = _kit.StartStandardRun();
            _targeting.ReturnNull = true;

            PlayerFrameInput input = _kit.Input(
                Displacement3D.Zero,
                Direction3D.Right,
                true,
                false);

            Assert.Throws<InvalidOperationException>(
                () =>
                {
                    _phase.Execute(run, input, s_delta, _recorder);
                });

            Assert.That(run.HasPendingInteraction, Is.True);
            Assert.That(_tracker.HasCorrelation, Is.True);
        }

        [Test]
        public void TickFailedExceptionCarriesPartialResultAndCause()
        {
            ArenaRun run = _kit.StartStandardRun();
            ArenaRunTickResult partial = new(
                run.Id,
                AggregateRevision.Initial,
                Array.Empty<IArenaDomainEvent>(),
                Array.Empty<ArenaRunTickStage>());

            InvalidOperationException cause = new("adapter failure");
            ArenaRunTickFailedException exception = new(partial, cause);

            Assert.That(exception.PartialResult, Is.SameAs(partial));
            Assert.That(exception.InnerException, Is.SameAs(cause));
        }
    }
}
