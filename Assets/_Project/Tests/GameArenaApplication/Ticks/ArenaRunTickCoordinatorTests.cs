using System;
using Game.Arena.Application.Input;
using Game.Arena.Application.Ports;
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
    public sealed class ArenaRunTickCoordinatorTests
    {
        private TickCoordinatorTestKit _kit;

        [SetUp]
        public void SetUp()
        {
            _kit = new TickCoordinatorTestKit();
            _kit.Session.StartInitialRun();
            _kit.Input.Input = new PlayerFrameInput(
                MovementInput.Zero,
                Direction3D.Right,
                false,
                false);
        }

        [Test]
        public void TickAdvancesGameTimeFirst()
        {
            ArenaRun run = _kit.Session.GetRequiredActiveRun();

            ArenaRunTickResult result = _kit.Coordinator.ExecuteTick();

            Assert.That(run.CurrentTime.Seconds, Is.EqualTo(0.1d));
            Assert.That(result.ExecutedStages[0], Is.EqualTo(ArenaRunTickStage.TimeAdvance));
        }

        [Test]
        public void TickWithoutInputDoesNotOpenInteractions()
        {
            ArenaRun run = _kit.Session.GetRequiredActiveRun();

            ArenaRunTickResult result = _kit.Coordinator.ExecuteTick();

            Assert.That(run.HasPendingInteraction, Is.False);
            Assert.That(result.DomainEvents.Count, Is.EqualTo(1));
            Assert.That(result.DomainEvents[0], Is.TypeOf<EnemySpawned>());
        }

        [Test]
        public void TickContainsEventsInOccurrenceOrder()
        {
            _kit.Input.Input = new PlayerFrameInput(
                MovementInput.Zero,
                Direction3D.Right,
                true,
                false);

            ArenaRunTickResult result = _kit.Coordinator.ExecuteTick();

            Assert.That(result.DomainEvents.Count, Is.GreaterThan(0));
            Assert.That(result.DomainEvents[0], Is.TypeOf<PlayerAttackStarted>());
        }

        [Test]
        public void TickResultRevisionMatchesAggregateRevision()
        {
            ArenaRun run = _kit.Session.GetRequiredActiveRun();

            ArenaRunTickResult result = _kit.Coordinator.ExecuteTick();

            Assert.That(result.FinalRevision, Is.EqualTo(run.Revision));
        }

        [Test]
        public void PlayerStagesRunAfterTimeAdvance()
        {
            _kit.Input.Input = new PlayerFrameInput(
                MovementInput.FromVector(new Displacement3D(1f, 0f, 0f)),
                Direction3D.Right,
                true,
                true);

            ArenaRunTickResult result = _kit.Coordinator.ExecuteTick();

            Assert.That(result.ExecutedStages[0], Is.EqualTo(ArenaRunTickStage.TimeAdvance));
            Assert.That(result.ExecutedStages[1], Is.EqualTo(ArenaRunTickStage.WeaponSwitch));
            Assert.That(result.ExecutedStages[2], Is.EqualTo(ArenaRunTickStage.PlayerMovement));
            Assert.That(result.ExecutedStages[3], Is.EqualTo(ArenaRunTickStage.PlayerAttackStart));
        }

        [Test]
        public void RejectedMovementResolutionStopsTickBeforeAttackStart()
        {
            ArenaRun run = _kit.Session.GetRequiredActiveRun();
            _kit.MovementResolver.Mode = ScriptedPlayerMovementResolver.ResolveMode.ReturnFixed;
            _kit.MovementResolver.FixedPosition = new Position3D(100f, 0f, 0f);

            _kit.Input.Input = new PlayerFrameInput(
                MovementInput.FromVector(
                    new Displacement3D(1f, 0f, 0f)),
                Direction3D.Right,
                true,
                false);

            ArenaRunTickResult result = _kit.Coordinator.ExecuteTick();

            Assert.That(run.HasPendingInteraction, Is.True);
            Assert.That(result.ExecutedStages, Does.Not.Contain(ArenaRunTickStage.PlayerAttackStart.ToString()));
        }

        [Test]
        public void PendingInteractionIsRecoveredAtBeginningOfNextTick()
        {
            ArenaRun run = _kit.Session.GetRequiredActiveRun();
            _kit.MovementResolver.Mode = ScriptedPlayerMovementResolver.ResolveMode.ThrowError;

            _kit.Input.Input = new PlayerFrameInput(
                MovementInput.FromVector(new Displacement3D(1f, 0f, 0f)),
                Direction3D.Right,
                true,
                false);

            Assert.Throws<ArenaRunTickFailedException>(
                () =>
                {
                    _kit.Coordinator.ExecuteTick();
                });

            _kit.MovementResolver.Mode = ScriptedPlayerMovementResolver.ResolveMode.AcceptRequested;
            _kit.Input.Input = new PlayerFrameInput(
                MovementInput.Zero,
                Direction3D.Right,
                false,
                false);

            ArenaRunTickResult result = _kit.Coordinator.ExecuteTick();

            Assert.That(run.HasPendingInteraction, Is.False);
            Assert.That(result.ExecutedStages[0], Is.EqualTo(ArenaRunTickStage.PendingInteractionCancellation));
        }

        [Test]
        public void NullTargetingResultProducesTickFailure()
        {
            _kit.TargetingResolver.ReturnNull = true;
            _kit.Input.Input = new PlayerFrameInput(
                MovementInput.Zero,
                Direction3D.Right,
                true,
                false);

            ArenaRunTickFailedException exception =
                Assert.Throws<ArenaRunTickFailedException>(
                    () =>
                    {
                        _kit.Coordinator.ExecuteTick();
                    });

            Assert.That(exception.PartialResult.DomainEvents.Count, Is.GreaterThan(0));
            Assert.That(exception.InnerException, Is.TypeOf<InvalidOperationException>());
        }

        [Test]
        public void PlayerAttackStartsBeforeEnemyStages()
        {
            _kit.Input.Input = new PlayerFrameInput(
                MovementInput.Zero,
                Direction3D.Right,
                true,
                false);

            ArenaRunTickResult result = _kit.Coordinator.ExecuteTick();

            int playerStartIndex = FindStageIndex(result, ArenaRunTickStage.PlayerAttackStart);
            int enemyMovementIndex = FindStageIndex(result, ArenaRunTickStage.EnemyMovement);

            Assert.That(playerStartIndex, Is.LessThan(enemyMovementIndex));
        }

        [Test]
        public void CoordinatorDoesNotPublishEvents()
        {
            _kit.Input.Input = new PlayerFrameInput(
                MovementInput.Zero,
                Direction3D.Right,
                true,
                false);

            ArenaRunTickResult result = _kit.Coordinator.ExecuteTick();

            Assert.That(result.DomainEvents.Count, Is.GreaterThan(0));
        }

        [Test]
        public void TerminalRunDoesNotExecuteAnyGameplayStage()
        {
            ArenaRun run = _kit.Session.GetRequiredActiveRun();
            run.MakeRunDefeat(reqularsToSpawn: 1);

            AggregateRevision revisionBeforeTick = run.Revision;
            GameTimePoint timeBeforeTick = run.CurrentTime;

            ArenaRunTickResult result = _kit.Coordinator.ExecuteTick();

            Assert.That(result.ExecutedStages, Is.Empty);
            Assert.That(run.Revision, Is.EqualTo(revisionBeforeTick));
            Assert.That(run.CurrentTime, Is.EqualTo(timeBeforeTick));
            Assert.That(result.DomainEvents, Is.Empty);
        }

        [Test]
        public void TerminalRunDoesNotAdvanceTime()
        {
            ArenaRun run = _kit.Session.GetRequiredActiveRun();
            run.MakeRunDefeat(reqularsToSpawn: 1);

            GameTimePoint timeBeforeTick = run.CurrentTime;

            _kit.Coordinator.ExecuteTick();

            Assert.That(run.CurrentTime, Is.EqualTo(timeBeforeTick));
        }

        [Test]
        public void TerminalRunDoesNotExecutePlayerOrEnemyStages()
        {
            ArenaRun run = _kit.Session.GetRequiredActiveRun();
            run.MakeRunDefeat(reqularsToSpawn: 1);

            ArenaRunTickResult result = _kit.Coordinator.ExecuteTick();

            Assert.That(result.ExecutedStages, Does.Not.Contain(ArenaRunTickStage.WeaponSwitch.ToString()));
            Assert.That(result.ExecutedStages, Does.Not.Contain(ArenaRunTickStage.PlayerMovement.ToString()));
            Assert.That(result.ExecutedStages, Does.Not.Contain(ArenaRunTickStage.PlayerAttackStart.ToString()));
            Assert.That(result.ExecutedStages, Does.Not.Contain(ArenaRunTickStage.PlayerAttackImpact.ToString()));
            Assert.That(result.ExecutedStages, Does.Not.Contain(ArenaRunTickStage.EnemyMovement.ToString()));
            Assert.That(result.ExecutedStages, Does.Not.Contain(ArenaRunTickStage.EnemyAttackStart.ToString()));
            Assert.That(result.ExecutedStages, Does.Not.Contain(ArenaRunTickStage.EnemyAttackImpact.ToString()));
            Assert.That(result.ExecutedStages, Does.Not.Contain(ArenaRunTickStage.EnemySpawn.ToString()));
        }

        [Test]
        public void ZeroDeltaDoesNotRecordTimeAdvance()
        {
            TickCoordinatorTestKit kit = new();
            kit.Session.StartInitialRun();
            kit.Input.Input = new PlayerFrameInput(
                MovementInput.Zero,
                Direction3D.Right,
                false,
                false);

            ArenaRunTickCoordinatorDependencies dependencies = new(
                kit.Session,
                new ZeroGameClock(),
                kit.Input,
                kit.PlayerPhase,
                kit.EnemyPhase,
                kit.RecoveryStage,
                kit.TimeAdvanceStage);

            ArenaRunTickCoordinator coordinator = new(dependencies);

            ArenaRunTickResult result = coordinator.ExecuteTick();

            Assert.That(result.ExecutedStages, Does.Not.Contain(ArenaRunTickStage.TimeAdvance.ToString()));
        }

        private int FindStageIndex(
            ArenaRunTickResult result,
            ArenaRunTickStage stage)
        {
            for (int index = 0; index < result.ExecutedStages.Count; index++)
            {
                if (result.ExecutedStages[index] == stage)
                {
                    return index;
                }
            }

            return -1;
        }
    }

    internal sealed class ZeroGameClock : IGameClock
    {
        public GameDuration GetDelta()
        {
            return new GameDuration(0d);
        }
    }
}
