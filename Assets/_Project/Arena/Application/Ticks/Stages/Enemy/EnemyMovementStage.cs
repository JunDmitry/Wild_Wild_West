using System.Collections.Generic;
using Game.Arena.Application.Ports;
using Game.Arena.Domain.Aggregates.ArenaRun;
using Game.Arena.Domain.Interactions.Movement;
using Game.Arena.Domain.Time;

namespace Game.Arena.Application.Ticks.Stages
{
    internal sealed class EnemyMovementStage
    {
        private readonly IEnemyMovementResolver _resolver;

        public EnemyMovementStage(IEnemyMovementResolver resolver)
        {
            _resolver = resolver ?? throw new System.ArgumentNullException(nameof(resolver));
        }

        public StageExecutionStatus Execute(
            ArenaRun run,
            GameDuration delta,
            ArenaRunTickRecorder recorder)
        {
            recorder.RecordStage(ArenaRunTickStage.EnemyMovement);

            EnemyMovementBatchRequestOutcome requestOutcome = run.RequestEnemyMovementBatch(delta);

            if (requestOutcome.HasRequest == false)
            {
                return StageExecutionStatus.Completed;
            }

            EnemyMovementBatchRequest request = requestOutcome.Request;
            IReadOnlyList<EnemyMovementBatchResolutionEntry> entries = _resolver.Resolve(request);

            if (entries == null)
            {
                throw new System.InvalidOperationException("Enemy movement resolver returned null.");
            }

            EnemyMovementBatchResolution resolution = new(request.Correlation, entries);
            EnemyMovementBatchResolutionOutcome outcome = run.ApplyEnemyMovementBatch(resolution);

            recorder.RecordChange(outcome.Change);

            if (outcome.IsAccepted == false)
            {
                return StageExecutionStatus.InteractionLeftPending;
            }

            return StageExecutionStatus.Completed;
        }
    }
}
