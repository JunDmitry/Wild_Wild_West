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
        private readonly PendingInteractionTracker _interactionTracker;
        private readonly List<EnemyMovementBatchResolutionEntry> _skipEntries = new();

        private EnemyMovementBatchResolution _resolution;

        public EnemyMovementStage(IEnemyMovementResolver resolver, PendingInteractionTracker interactionTracker)
        {
            _resolver = resolver ?? throw new System.ArgumentNullException(nameof(resolver));
            _interactionTracker = interactionTracker ?? throw new System.ArgumentNullException(nameof(interactionTracker));
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
            _interactionTracker.Track(request.Correlation);
            IReadOnlyList<EnemyMovementBatchResolutionEntry> entries = Game.Arena.Application.Diagnostics.AttributionFlags.SkipMovementResolve
                ? UnresolvedEntries(request)
                : _resolver.Resolve(request);

            if (entries == null)
            {
                throw new System.InvalidOperationException("Enemy movement resolver returned null.");
            }

            EnemyMovementBatchResolution resolution = _resolution;

            if (resolution == null)
            {
                resolution = new EnemyMovementBatchResolution(entries.Count);
                _resolution = resolution;
            }

            resolution.Fill(request.Correlation, entries);
            EnemyMovementBatchResolutionOutcome outcome = run.ApplyEnemyMovementBatch(resolution);

            recorder.RecordChange(outcome.Change);

            if (outcome.IsAccepted == false)
            {
                return StageExecutionStatus.InteractionLeftPending;
            }

            _interactionTracker.Clear();

            return StageExecutionStatus.Completed;
        }

        // TEMPORARY attribution path (t9-gc-residual-b); reverted before the
        // acceptance runs. Accepts each intent unchanged, skipping the Unity
        // resolver and its physics query.
        private IReadOnlyList<EnemyMovementBatchResolutionEntry> UnresolvedEntries(
            EnemyMovementBatchRequest request)
        {
            _skipEntries.Clear();

            IReadOnlyList<EnemyMovementIntent> intents = request.Intents;

            for (int index = 0; index < intents.Count; index++)
            {
                EnemyMovementIntent intent = intents[index];
                _skipEntries.Add(new EnemyMovementBatchResolutionEntry(
                    intent.EnemyId,
                    intent.Intent.RequestedPosition));
            }

            return _skipEntries;
        }
    }
}
