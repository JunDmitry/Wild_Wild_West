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

            _weaponSwitch.Execute(run, input, recorder);
            StageExecutionStatus movementStatus = _movement.Execute(run, input, delta, recorder);

            if (movementStatus == StageExecutionStatus.InteractionLeftPending)
            {
                return movementStatus;
            }

            _attackStart.Execute(run, input, recorder);

            return _attackImpact.Execute(run, input, recorder);
        }
    }
}
