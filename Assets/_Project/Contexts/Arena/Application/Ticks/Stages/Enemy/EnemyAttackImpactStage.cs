using Game.Arena.Domain.Aggregates.ArenaRun;

namespace Game.Arena.Application.Ticks.Stages
{
    internal sealed class EnemyAttackImpactStage
    {
        public StageExecutionStatus Execute(
            ArenaRun run,
            ArenaRunTickRecorder recorder)
        {
            recorder.RecordStage(ArenaRunTickStage.EnemyAttackImpact);

            EnemyAttackImpactOutcome outcome = run.ResolveDueEnemyAttackImpacts();
            recorder.RecordChange(outcome.Change);

            return StageExecutionStatus.Completed;
        }
    }
}
