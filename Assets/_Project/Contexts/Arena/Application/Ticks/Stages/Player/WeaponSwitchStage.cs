using Game.Arena.Application.Input;
using Game.Arena.Domain.Aggregates.ArenaRun;

namespace Game.Arena.Application.Ticks.Stages
{
    internal sealed class WeaponSwitchStage
    {
        public StageExecutionStatus Execute(
            ArenaRun run,
            PlayerFrameInput input,
            ArenaRunTickRecorder recorder)
        {
            if (input.SwitchWeaponPressed == false)
            {
                return StageExecutionStatus.Completed;
            }

            recorder.RecordStage(ArenaRunTickStage.WeaponSwitch);

            WeaponSwitchOutcome outcome = run.SwitchWeapon();
            recorder.RecordChange(outcome.Change);

            return StageExecutionStatus.Completed;
        }
    }
}
