using Game.Arena.Application.Diagnostics;
using Game.Arena.Application.Input;
using Game.Arena.Application.Ports;
using Game.Arena.Application.Ticks.Stages;
using Game.Arena.Domain.Aggregates.ArenaRun;
using Game.Arena.Domain.Time;

namespace Game.Arena.Application.Ticks
{
    internal sealed class PlayerTickPhase
    {
        private readonly WeaponSwitchStage _weaponSwitch;
        private readonly PlayerMovementStage _movement;
        private readonly PlayerAttackStartStage _attackStart;
        private readonly PlayerAttackImpactStage _attackImpact;

        public PlayerTickPhase(IPlayerMovementResolver movementResolver, IPlayerAttackTargetingResolver targetingResolver, PendingInteractionTracker interactionTracker)
        {
            _weaponSwitch = new();
            _movement = new(movementResolver, interactionTracker);
            _attackStart = new();
            _attackImpact = new(targetingResolver, interactionTracker);
        }

        public StageExecutionStatus Execute(
            ArenaRun run,
            PlayerFrameInput input,
            GameDuration delta,
            ArenaRunTickRecorder recorder)
        {
            if (run.Status != ArenaRunStatus.Playing)
            {
                return StageExecutionStatus.Completed;
            }

            // TEMPORARY attribution switches (t9-gc-residual-b); reverted before
            // the acceptance runs.
            if (AttributionFlags.SkipsTickStage(nameof(ArenaRunTickStage.WeaponSwitch)) == false)
            {
                _weaponSwitch.Execute(run, input, recorder);
            }

            StageExecutionStatus movementStatus = StageExecutionStatus.Completed;

            if (AttributionFlags.SkipsTickStage(nameof(ArenaRunTickStage.PlayerMovement)) == false)
            {
                movementStatus = _movement.Execute(run, input, delta, recorder);
            }

            if (movementStatus == StageExecutionStatus.InteractionLeftPending)
            {
                return movementStatus;
            }

            if (AttributionFlags.SkipsTickStage(nameof(ArenaRunTickStage.PlayerAttackStart)) == false)
            {
                _attackStart.Execute(run, input, recorder);
            }

            if (AttributionFlags.SkipsTickStage(nameof(ArenaRunTickStage.PlayerAttackImpact)))
            {
                return StageExecutionStatus.Completed;
            }

            return _attackImpact.Execute(run, input, recorder);
        }
    }
}
