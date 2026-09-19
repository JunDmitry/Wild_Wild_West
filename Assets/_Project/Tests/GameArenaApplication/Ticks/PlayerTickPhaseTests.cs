using System;
using Game.Arena.Application.Input;
using Game.Arena.Application.Ticks;
using Game.Arena.Domain.Aggregates.ArenaRun;
using Game.Arena.Domain.Combat;
using Game.Arena.Domain.Concurrency;
using Game.Arena.Domain.Events;
using Game.Arena.Domain.Geometry;
using Game.Arena.Domain.Identity;
using Game.Arena.Domain.Time;
using NUnit.Framework;

namespace Game.Arena.Application.Tests.Ticks
{
    [TestFixture]
    public sealed class PlayerTickPhaseTests
    {
        private static readonly GameDuration s_delta = new(0.1d);
        private static readonly Displacement3D s_right = new(1f, 0f, 0f);

        private TickTestKit _kit;
        private ScriptedPlayerMovementResolver _movement;
        private ScriptedTargetingResolver _targeting;
        private PlayerTickPhase _phase;
        private ArenaRunTickRecorder _recorder;

        [SetUp]
        public void SetUp()
        {
            _kit = new TickTestKit();
            _movement = new ScriptedPlayerMovementResolver();
            _targeting = new ScriptedTargetingResolver();
            _phase = new PlayerTickPhase(_movement, _targeting);
            _recorder = new ArenaRunTickRecorder();
        }

        [Test]
        public void WeaponSwitchIsAppliedBeforeAttackStart()
        {
            ArenaRun run = _kit.StartStandardRun();
            PlayerFrameInput input = _kit.Input(Displacement3D.Zero, Direction3D.Right, true, true);

            _phase.Execute(run, input, s_delta, _recorder);
            ArenaRunTickResult result = _recorder.Build(run.Id, run.Revision);

            Assert.That(run.SelectedWeapon, Is.EqualTo(WeaponKind.Melee));
            Assert.That(run.HasPendingPlayerAttack, Is.True);

            PlayerAttackStarted started = (PlayerAttackStarted)result.DomainEvents[0];

            Assert.That(started.WeaponKind, Is.EqualTo(WeaponKind.Melee));
            Assert.That(result.ExecutedStages[0], Is.EqualTo(ArenaRunTickStage.WeaponSwitch));
            Assert.That(result.ExecutedStages[1], Is.EqualTo(ArenaRunTickStage.PlayerMovement));
            Assert.That(result.ExecutedStages[2], Is.EqualTo(ArenaRunTickStage.PlayerAttackStart));
            Assert.That(result.ExecutedStages[3], Is.EqualTo(ArenaRunTickStage.PlayerAttackImpact));
        }

        [Test]
        public void PlayerMovementRequestIsResolvedAndApplied()
        {
            ArenaRun run = _kit.StartStandardRun();
            PlayerFrameInput input = _kit.Input(s_right, Direction3D.Right, false, false);

            StageExecutionStatus status = _phase.Execute(run, input, s_delta, _recorder);

            Assert.That(status, Is.EqualTo(StageExecutionStatus.Completed));
            Assert.That(_movement.CallCount, Is.EqualTo(1));
            Assert.That(run.PlayerPosition.X, Is.EqualTo(0.5f).Within(0.0001f));
            Assert.That(run.HasPendingInteraction, Is.False);
        }

        [Test]
        public void ResolutionIsBuiltFromTheSavedRequestCorrelation()
        {
            ArenaRun run = _kit.StartStandardRun();
            PlayerFrameInput input = _kit.Input(s_right, Direction3D.Right, false, false);

            _phase.Execute(run, input, s_delta, _recorder);

            Assert.That(_movement.LastRequest.Correlation.ArenaRunId, Is.EqualTo(run.Id));
            Assert.That(_movement.LastRequest.Correlation.AggregateRevision, Is.EqualTo(AggregateRevision.Initial));
            Assert.That(run.Revision.Value, Is.EqualTo(1UL));
        }

        [Test]
        public void ZeroMovementInputDoesNotCallResolver()
        {
            ArenaRun run = _kit.StartStandardRun();

            _phase.Execute(run, _kit.Idle(), s_delta, _recorder);

            Assert.That(_movement.CallCount, Is.EqualTo(0));
            Assert.That(run.Revision, Is.EqualTo(AggregateRevision.Initial));
        }

        [Test]
        public void BlockedMovementKeepsPositionAndClosesInteraction()
        {
            ArenaRun run = _kit.StartStandardRun();
            _movement.Mode = ScriptedPlayerMovementResolver.ResolveMode.StayAtOrigin;
            PlayerFrameInput input = _kit.Input(s_right, Direction3D.Right, false, false);

            StageExecutionStatus status = _phase.Execute(run, input, s_delta, _recorder);

            Assert.That(status, Is.EqualTo(StageExecutionStatus.Completed));
            Assert.That(run.PlayerPosition, Is.EqualTo(Position3D.Zero));
            Assert.That(run.HasPendingInteraction, Is.False);
            Assert.That(run.Revision, Is.EqualTo(AggregateRevision.Initial));
        }

        [Test]
        public void RejectedMovementResolutionLeavesInteractionPendingAndStopsPhase()
        {
            ArenaRun run = _kit.StartStandardRun();
            _movement.Mode = ScriptedPlayerMovementResolver.ResolveMode.ReturnFixed;
            _movement.FixedPosition = new Position3D(10f, 0f, 0f);
            PlayerFrameInput input = _kit.Input(s_right, Direction3D.Right, true, false);

            StageExecutionStatus status = _phase.Execute(run, input, s_delta, _recorder);
            ArenaRunTickResult result = _recorder.Build(run.Id, run.Revision);

            Assert.That(status, Is.EqualTo(StageExecutionStatus.InteractionLeftPending));
            Assert.That(run.HasPendingInteraction, Is.True);
            Assert.That(run.HasPendingPlayerAttack, Is.False);
            Assert.That(_targeting.CallCount, Is.EqualTo(0));
            Assert.That(result.ExecutedStages, Does.Not.Contain(ArenaRunTickStage.PlayerAttackStart.ToString()));
        }

        [Test]
        public void RangedAttackStartedThisTickIsResolvedInSameTick()
        {
            ArenaRun run = _kit.StartStandardRun();
            EnemyId enemyId = _kit.Spawn(run, 10UL, new Position3D(22f, 0f, 0f));
            _targeting.SetHits(enemyId);
            PlayerFrameInput input = _kit.Input(Displacement3D.Zero, Direction3D.Right, true, false);

            _phase.Execute(run, input, s_delta, _recorder);
            ArenaRunTickResult result = _recorder.Build(run.Id, run.Revision);

            Assert.That(_targeting.CallCount, Is.EqualTo(1));
            Assert.That(run.HasPendingPlayerAttack, Is.False);
            Assert.That(result.DomainEvents[0], Is.TypeOf<PlayerAttackStarted>());
            Assert.That(result.DomainEvents[1], Is.TypeOf<EnemyDamaged>());
            Assert.That(result.DomainEvents[2], Is.TypeOf<PlayerAttackCompleted>());
            Assert.That(result.DomainEvents[0].AggregateRevision, Is.LessThan(result.DomainEvents[1].AggregateRevision));
        }

        [Test]
        public void MeleeAttackImpactIsNotRequestedBeforeWindup()
        {
            ArenaRun run = _kit.StartStandardRun();
            PlayerFrameInput input = _kit.Input(Displacement3D.Zero, Direction3D.Right, true, true);

            _phase.Execute(run, input, s_delta, _recorder);

            Assert.That(_targeting.CallCount, Is.EqualTo(0));
            Assert.That(run.HasPendingPlayerAttack, Is.True);
            Assert.That(run.HasPendingInteraction, Is.False);
        }

        [Test]
        public void DueImpactUsesCurrentAimWithoutNewAttackRequest()
        {
            ArenaRun run = _kit.StartStandardRun();
            _phase.Execute(
                run,
                _kit.Input(Displacement3D.Zero, Direction3D.Right, true, true),
                s_delta,
                _recorder);

            run.AdvanceTime(new GameDuration(0.3d));

            _phase.Execute(
                run,
                _kit.Input(Displacement3D.Zero, Direction3D.Left, false, false),
                s_delta,
                _recorder);

            Assert.That(_targeting.CallCount, Is.EqualTo(1));
            Assert.That(_targeting.LastRequest.AimDirection, Is.EqualTo(Direction3D.Left));
            Assert.That(_targeting.LastRequest.WeaponKind, Is.EqualTo(WeaponKind.Melee));
            Assert.That(run.HasPendingPlayerAttack, Is.False);
        }

        [Test]
        public void MissResolutionCompletesAttack()
        {
            ArenaRun run = _kit.StartStandardRun();
            PlayerFrameInput input = _kit.Input(Displacement3D.Zero, Direction3D.Right, true, false);

            _phase.Execute(run, input, s_delta, _recorder);
            ArenaRunTickResult result = _recorder.Build(run.Id, run.Revision);
            PlayerAttackCompleted completed = (PlayerAttackCompleted)result.DomainEvents[1];

            Assert.That(completed.Outcome, Is.EqualTo(AttackOutcome.Miss));
            Assert.That(run.HasPendingPlayerAttack, Is.False);
        }

        [Test]
        public void NullTargetingResultThrows()
        {
            ArenaRun run = _kit.StartStandardRun();
            _targeting.ReturnNull = true;
            PlayerFrameInput input = _kit.Input(Displacement3D.Zero, Direction3D.Right, true, false);

            Assert.Throws<InvalidOperationException>(
                () =>
                {
                    _phase.Execute(run, input, s_delta, _recorder);
                });

            Assert.That(run.HasPendingInteraction, Is.True);
        }

        [Test]
        public void RejectedImpactResolutionLeavesInteractionPending()
        {
            ArenaRun run = _kit.StartStandardRun();
            _targeting.SetHits(EnemyId.FromValue(777UL));
            PlayerFrameInput input = _kit.Input(Displacement3D.Zero, Direction3D.Right, true, false);

            StageExecutionStatus status = _phase.Execute(run, input, s_delta, _recorder);

            Assert.That(status, Is.EqualTo(StageExecutionStatus.InteractionLeftPending));
            Assert.That(run.HasPendingInteraction, Is.True);
            Assert.That(run.HasPendingPlayerAttack, Is.True);
        }

        [Test]
        public void TerminalRunSkipsPlayerStages()
        {
            ArenaRun run = _kit.StartCompactRun(5);
            _kit.DefeatPlayer(run);
            AggregateRevision revisionAfterDefeat = run.Revision;
            PlayerFrameInput input = _kit.Input(s_right, Direction3D.Right, true, true);

            StageExecutionStatus status = _phase.Execute(run, input, s_delta, _recorder);
            ArenaRunTickResult result = _recorder.Build(run.Id, run.Revision);

            Assert.That(run.Status, Is.EqualTo(ArenaRunStatus.Defeat));
            Assert.That(status, Is.EqualTo(StageExecutionStatus.Completed));
            Assert.That(result.ExecutedStages, Is.Empty);
            Assert.That(run.Revision, Is.EqualTo(revisionAfterDefeat));
            Assert.That(_movement.CallCount, Is.EqualTo(0));
        }
    }
}
