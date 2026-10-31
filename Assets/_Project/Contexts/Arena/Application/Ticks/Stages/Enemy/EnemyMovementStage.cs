using System;
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

        private EnemyMovementBatchResolution _resolution;

        private ArenaRunStateSnapshot _snapshotBuffer;
        private EnemyFootprint[] _footprints;
        private int _footprintCapacity = -1;

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
            ReadOnlySpan<EnemyFootprint> occupied = BuildOccupancy(run);
            IReadOnlyList<EnemyMovementBatchResolutionEntry> entries = _resolver.Resolve(request, occupied);

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

        /// <summary>
        /// Fills a reusable run snapshot and footprint buffer with every live
        /// enemy, so the movement resolver caps an intent against an Enemy the
        /// Domain omitted from the batch or Presentation did not render
        /// (ADR-0040). The buffers are allocated once per enemy capacity and
        /// reused on every later tick (ADR-0035).
        /// </summary>
        private ReadOnlySpan<EnemyFootprint> BuildOccupancy(ArenaRun run)
        {
            int capacity = run.EnemyCapacity;

            if (_footprints == null || _footprintCapacity != capacity)
            {
                _snapshotBuffer = run.CreateSnapshotBuffer();
                _footprints = new EnemyFootprint[capacity];
                _footprintCapacity = capacity;
            }

            ArenaRunStateSnapshot snapshot = run.CreateSnapshot(_snapshotBuffer);
            int count = 0;

            for (int index = 0; index < snapshot.Enemies.Count; index++)
            {
                EnemyStateSnapshot enemy = snapshot.Enemies[index];
                _footprints[count++] = new EnemyFootprint(
                    enemy.EnemyId,
                    enemy.Position,
                    enemy.CollisionRadius);
            }

            return new ReadOnlySpan<EnemyFootprint>(_footprints, 0, count);
        }
    }
}
