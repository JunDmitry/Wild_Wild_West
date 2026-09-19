using Game.Arena.Domain.Aggregates.ArenaRun;
using Game.Arena.Domain.Time;

namespace Game.Arena.Application.Ticks.Stages
{
    internal sealed class TimeAdvanceStage
    {
        public StageExecutionStatus Execute(
            ArenaRun run,
            GameDuration delta,
            ArenaRunTickRecorder recorder)
        {
            if (delta.Seconds <= 0d)
            {
                return StageExecutionStatus.Completed;
            }

            if (run.Status != ArenaRunStatus.Playing)
            {
                return StageExecutionStatus.Completed;
            }

            recorder.RecordStage(ArenaRunTickStage.TimeAdvance);

            TimeAdvanceOutcome outcome = run.AdvanceTime(delta);
            recorder.RecordChange(outcome.Change);

            return StageExecutionStatus.Completed;
        }
    }
}
