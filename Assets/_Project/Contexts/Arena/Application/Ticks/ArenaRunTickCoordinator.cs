using Game.Arena.Application.Input;
using Game.Arena.Application.ReadModels;
using Game.Arena.Domain.Aggregates.ArenaRun;
using Game.Arena.Domain.Time;

namespace Game.Arena.Application.Ticks
{
    internal sealed class ArenaRunTickCoordinator
    {
        private readonly ArenaRunTickCoordinatorDependencies _dependencies;

        public ArenaRunTickCoordinator(ArenaRunTickCoordinatorDependencies dependencies)
        {
            _dependencies = dependencies ?? throw new System.ArgumentNullException(nameof(dependencies));
        }

        public ArenaRunTickResult ExecuteTick()
        {
            ArenaRun run = _dependencies.Session.GetRequiredActiveRun();
            ArenaRunTickRecorder recorder = new();

            try
            {
                StageExecutionStatus status = _dependencies.RecoveryStage.Execute(run, recorder);

                if (status == StageExecutionStatus.InteractionLeftPending)
                {
                    throw new System.InvalidOperationException("Interaction recovery left an interaction pending.");
                }

                GameDuration delta = _dependencies.Clock.GetDelta();
                PlayerFrameInput input = _dependencies.InputSource.Read();

                if (input == null)
                {
                    throw new System.InvalidOperationException("Player input source returned null.");
                }

                StageExecutionStatus timeStatus = _dependencies.TimeAdvanceStage.Execute(run, delta, recorder);

                if (timeStatus == StageExecutionStatus.InteractionLeftPending)
                {
                    throw new System.InvalidOperationException("Time advance left an interaction pending.");
                }

                if (run.Status != ArenaRunStatus.Playing)
                {
                    return BuildResult(run, recorder);
                }

                StageExecutionStatus playerStatus = _dependencies.PlayerPhase.Execute(run, input, delta, recorder);

                if (playerStatus == StageExecutionStatus.InteractionLeftPending)
                {
                    return BuildResult(run, recorder);
                }

                if (run.Status != ArenaRunStatus.Playing)
                {
                    return BuildResult(run, recorder);
                }

                StageExecutionStatus enemyStatus = _dependencies.EnemyPhase.Execute(run, delta, recorder);

                if (enemyStatus == StageExecutionStatus.InteractionLeftPending)
                {
                    return BuildResult(run, recorder);
                }

                return BuildResult(run, recorder);
            }
            catch (System.Exception exception)
            {
                ArenaRunTickResult partialResult = BuildResult(run, recorder);

                throw new ArenaRunTickFailedException(partialResult, exception);
            }
        }

        private ArenaRunTickResult BuildResult(ArenaRun run, ArenaRunTickRecorder recorder)
        {
            ArenaRunStateSnapshot stateSnapshot = run.CreateSnapshot();
            ArenaRunSnapshot snapshot = _dependencies.SnapshotMapper.Map(stateSnapshot);

            return recorder.Build(
                run.Id,
                run.Revision,
                snapshot);
        }
    }
}
