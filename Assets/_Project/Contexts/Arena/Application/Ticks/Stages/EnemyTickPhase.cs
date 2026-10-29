using Game.Arena.Application.Identity;
using Game.Arena.Application.Ports;
using Game.Arena.Application.Spawning;
using Game.Arena.Application.Ticks.Stages;
using Game.Arena.Domain.Aggregates.ArenaRun;
using Game.Arena.Domain.Time;

namespace Game.Arena.Application.Ticks
{
    internal sealed class EnemyTickPhase
    {
        private readonly EnemyMovementStage _movementStage;
        private readonly EnemyAttackStartStage _enemyAttackStartStage;
        private readonly EnemyAttackImpactStage _enemyAttackImpactStage;
        private readonly EnemySpawnStage _enemySpawnStage;

        public EnemyTickPhase(
            IEnemyMovementResolver enemyMovementResolver,
            IEnemySpawnPlacementResolver placementResolver,
            IEnemyIdSource enemyIdSource,
            IEnemySpawnPacingPolicy spawnPacingPolicy,
            PendingInteractionTracker interactionTracker)
        {
            _movementStage = new(enemyMovementResolver, interactionTracker);
            _enemyAttackStartStage = new();
            _enemyAttackImpactStage = new();
            _enemySpawnStage = new(placementResolver, enemyIdSource, spawnPacingPolicy, interactionTracker);
        }

        public StageExecutionStatus Execute(
            ArenaRun run,
            GameDuration delta,
            ArenaRunTickRecorder recorder)
        {
            if (run.Status != ArenaRunStatus.Playing)
            {
                return StageExecutionStatus.Completed;
            }

            StageExecutionStatus movementStatus = _movementStage.Execute(run, delta, recorder);

            if (movementStatus == StageExecutionStatus.InteractionLeftPending)
            {
                return movementStatus;
            }

            _enemyAttackStartStage.Execute(run, recorder);
            _enemyAttackImpactStage.Execute(run, recorder);

            if (run.Status != ArenaRunStatus.Playing)
            {
                return StageExecutionStatus.Completed;
            }

            return _enemySpawnStage.Execute(run, recorder);
        }
    }
}
