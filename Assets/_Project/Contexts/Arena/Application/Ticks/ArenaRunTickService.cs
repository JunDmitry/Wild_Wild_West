using System;
using Game.Arena.Application.Dispatch;
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
        private readonly IArenaRunDispatch _dispatcher;

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
            DefeatRestartStage defeatRestartStage = new(
                    dependencies.Session,
                    dependencies.RestartDelayPolicy);

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
                    defeatRestartStage,
                    snapshotMapper);

            _coordinator = new ArenaRunTickCoordinator(coordinatorDependencies);
            _dispatcher = dependencies.Dispatcher;
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
                    _dependencies.RestartDelayPolicy.Reset();
                    _lastArenaRunId = activeId;
                }
            }

            ArenaRunTickResult result = _coordinator.ExecuteTick();

            try
            {
                _dispatcher.Dispatch(result);
            }
            catch (Exception exception)
            {
                throw new ArenaRunTickFailedException(result, exception);
            }

            return result;
        }
    }
}
