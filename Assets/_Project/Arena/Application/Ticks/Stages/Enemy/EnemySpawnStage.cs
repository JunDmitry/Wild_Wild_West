using Game.Arena.Application.Identity;
using Game.Arena.Application.Ports;
using Game.Arena.Application.Spawning;
using Game.Arena.Domain.Aggregates.ArenaRun;
using Game.Arena.Domain.Geometry;
using Game.Arena.Domain.Identity;
using Game.Arena.Domain.Interactions.Spawn;

namespace Game.Arena.Application.Ticks.Stages
{
    internal sealed class EnemySpawnStage
    {
        private readonly IEnemySpawnPlacementResolver _placementResolver;
        private readonly IEnemyIdSource _enemyIdSource;
        private readonly IEnemySpawnPacingPolicy _enemySpawnPacing;

        public EnemySpawnStage(
            IEnemySpawnPlacementResolver placementResolver,
            IEnemyIdSource enemyIdSource,
            IEnemySpawnPacingPolicy enemySpawnPacing)
        {
            _placementResolver = placementResolver ?? throw new System.ArgumentNullException(nameof(placementResolver));
            _enemyIdSource = enemyIdSource ?? throw new System.ArgumentNullException(nameof(enemyIdSource));
            _enemySpawnPacing = enemySpawnPacing ?? throw new System.ArgumentNullException(nameof(enemySpawnPacing));
        }

        public StageExecutionStatus Execute(
            ArenaRun run,
            ArenaRunTickRecorder recorder)
        {
            if (_enemySpawnPacing.ShouldRequestSpawn(run.CurrentTime) == false)
            {
                return StageExecutionStatus.Completed;
            }

            recorder.RecordStage(ArenaRunTickStage.EnemySpawn);
            EnemySpawnRequestOutcome requestOutcome = run.RequestEnemySpawn();

            if (requestOutcome.HasRequest == false)
            {
                return StageExecutionStatus.Completed;
            }

            EnemySpawnRequest request = requestOutcome.Request;
            Position3D spawnPosition = _placementResolver.ResolveSpawnPosition(request);
            EnemyId enemyId = _enemyIdSource.Allocate();
            EnemySpawnResolution spawnResolution = new(
                request.Correlation,
                enemyId,
                spawnPosition);

            EnemySpawnResolutionOutcome outcome = run.ApplyEnemySpawn(spawnResolution);
            recorder.RecordChange(outcome.Change);

            if (outcome.IsSpawned == false)
            {
                return StageExecutionStatus.InteractionLeftPending;
            }

            _enemySpawnPacing.RecordSuccessfulSpawn(run.CurrentTime);

            return StageExecutionStatus.Completed;
        }
    }
}
