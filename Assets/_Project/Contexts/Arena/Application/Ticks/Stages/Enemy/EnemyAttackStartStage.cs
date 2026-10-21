using Game.Arena.Domain.Aggregates.ArenaRun;

namespace Game.Arena.Application.Ticks.Stages
{
    internal sealed class EnemyAttackStartStage
    {
        public StageExecutionStatus Execute(
            ArenaRun run,
            ArenaRunTickRecorder recorder)
        {
            recorder.RecordStage(ArenaRunTickStage.EnemyAttackStart);

            EnemyAttackStartOutcome outcome = run.StartEligibleEnemyAttacks();
            recorder.RecordChange(outcome.Change);

            return StageExecutionStatus.Completed;
        }
    }
}
