using System;
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
        private readonly PendingInteractionTracker _interactionTracker;

        private ArenaRunStateSnapshot _snapshotBuffer;
        private EnemyFootprint[] _footprints;
        private int _footprintCapacity = -1;

        public EnemySpawnStage(
            IEnemySpawnPlacementResolver placementResolver,
            IEnemyIdSource enemyIdSource,
            IEnemySpawnPacingPolicy enemySpawnPacing,
            PendingInteractionTracker interactionTracker)
        {
            _placementResolver = placementResolver ?? throw new System.ArgumentNullException(nameof(placementResolver));
            _enemyIdSource = enemyIdSource ?? throw new System.ArgumentNullException(nameof(enemyIdSource));
            _enemySpawnPacing = enemySpawnPacing ?? throw new System.ArgumentNullException(nameof(enemySpawnPacing));
            _interactionTracker = interactionTracker ?? throw new System.ArgumentNullException(nameof(interactionTracker));
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
            _interactionTracker.Track(request.Correlation);
            ReadOnlySpan<EnemyFootprint> occupied = BuildOccupancy(run);
            Position3D spawnPosition = _placementResolver.ResolveSpawnPosition(request, occupied);
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

            _interactionTracker.Clear();
            _enemySpawnPacing.RecordSuccessfulSpawn(run.CurrentTime);

            return StageExecutionStatus.Completed;
        }

        /// <summary>
        /// Fills a reusable run snapshot and footprint buffer with the live
        /// enemies, so the placement resolver can space the spawn against them.
        /// The buffers are allocated once per enemy capacity and reused on every
        /// later spawn (ADR-0039, ADR-0035).
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
