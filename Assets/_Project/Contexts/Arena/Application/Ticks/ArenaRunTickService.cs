using System;
using Game.Arena.Application.ReadModels;
using Game.Arena.Application.Ticks.Stages;
using Game.Arena.Domain.Identity;

namespace Game.Arena.Application.Ticks
{
    public sealed class ArenaRunTickService
    {
        private readonly ArenaRunTickServiceDependencies _dependencies;
        private readonly PendingInteractionTracker _tracker;
        private readonly ArenaRunTickCoordinator _coordinator;

        private ArenaRunId _lastArenaRunId;

        public ArenaRunTickService(ArenaRunTickServiceDependencies dependencies)
        {
            if (dependencies == null)
            {
                throw new ArgumentNullException(nameof(dependencies));
            }

            _dependencies = dependencies;
            _tracker = new PendingInteractionTracker();

            PendingInteractionRecoveryStage recoveryStage = new(_tracker);
            TimeAdvanceStage timeAdvanceStage = new();

            PlayerTickPhase playerPhase = new(
                    dependencies.MovementResolver,
                    dependencies.TargetingResolver,
                    _tracker);

            EnemyTickPhase enemyPhase = new(
                    dependencies.EnemyMovementResolver,
                    dependencies.SpawnPlacementResolver,
                    dependencies.EnemyIdSource,
                    dependencies.PacingPolicy,
                    _tracker);

            ArenaRunSnapshotMapper snapshotMapper = new();

            ArenaRunTickCoordinatorDependencies coordinatorDependencies = new(
                    dependencies.Session,
                    dependencies.Clock,
                    dependencies.InputSource,
                    playerPhase,
                    enemyPhase,
                    recoveryStage,
                    timeAdvanceStage,
                    snapshotMapper);

            _coordinator = new ArenaRunTickCoordinator(coordinatorDependencies);
            _lastArenaRunId = ArenaRunId.None;
        }

        public ArenaRunTickResult ExecuteTick()
        {
            if (_dependencies.Session.HasActiveRun)
            {
                ArenaRunId activeId = _dependencies.Session.ActiveArenaRunId;

                if (activeId != _lastArenaRunId)
                {
                    _dependencies.PacingPolicy.Reset();
                    _lastArenaRunId = activeId;
                }
            }

            return _coordinator.ExecuteTick();
        }
    }
}
