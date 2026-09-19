using Game.Arena.Application.Input;
using Game.Arena.Domain.Aggregates.ArenaRun;

namespace Game.Arena.Application.Ticks.Stages
{
    internal sealed class PlayerAttackStartStage
    {
        public StageExecutionStatus Execute(
            ArenaRun run,
            PlayerFrameInput input,
            ArenaRunTickRecorder recorder)
        {
            if (input.AttackPressed == false)
            {
                return StageExecutionStatus.Completed;
            }

            recorder.RecordStage(ArenaRunTickStage.PlayerAttackStart);

            PlayerAttackStartOutcome attackStartOutcome = run.StartPlayerAttack();
            recorder.RecordChange(attackStartOutcome.Change);

            return StageExecutionStatus.Completed;
        }
    }
}
