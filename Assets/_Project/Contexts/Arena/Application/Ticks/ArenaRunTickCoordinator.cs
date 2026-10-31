using Game.Arena.Application.Input;
using Game.Arena.Domain.Aggregates.ArenaRun;
using Game.Arena.Domain.Identity;
using Game.Arena.Domain.Time;

namespace Game.Arena.Application.Ticks
{
    internal sealed class ArenaRunTickCoordinator
    {
        private readonly ArenaRunTickCoordinatorDependencies _dependencies;

        private ArenaTickFrameBuffers _buffers;
        private ArenaRunId _buffersRunId;

        public ArenaRunTickCoordinator(ArenaRunTickCoordinatorDependencies dependencies)
        {
            _dependencies = dependencies ?? throw new System.ArgumentNullException(nameof(dependencies));
            _buffersRunId = ArenaRunId.None;
        }

        public ArenaRunTickResult ExecuteTick()
        {
            ArenaRun run = _dependencies.Session.GetRequiredActiveRun();
            ArenaTickFrameBuffers buffers = EnsureBuffers(run);

            // ADR-0036 point 6: advance the per-run event pool before the
            // recovery stage, so events filled two ticks ago are released.
            run.BeginTick();
            buffers.BeginTick();

            ArenaRunTickRecorder recorder = buffers.Recorder;

            try
            {
                StageExecutionStatus status = _dependencies.RecoveryStage.Execute(run, recorder);

                if (status == StageExecutionStatus.InteractionLeftPending)
                {
                    throw new System.InvalidOperationException("Interaction recovery left an interaction pending.");
                }

                GameDuration delta = _dependencies.Clock.GetDelta();

                if (_dependencies.DefeatRestartStage.Execute(run, delta, recorder))
                {
                    ArenaRun restartedRun = _dependencies.Session.GetRequiredActiveRun();
                    buffers = EnsureBuffers(restartedRun);
                    buffers.BeginTick();
                    buffers.Recorder.RecordStage(ArenaRunTickStage.DefeatRestart);

                    return BuildResult(buffers, restartedRun);
                }

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
                    return BuildResult(buffers, run);
                }

                StageExecutionStatus playerStatus = _dependencies.PlayerPhase.Execute(run, input, delta, recorder);

                if (playerStatus == StageExecutionStatus.InteractionLeftPending)
                {
                    return BuildResult(buffers, run);
                }

                if (run.Status != ArenaRunStatus.Playing)
                {
                    return BuildResult(buffers, run);
                }

                StageExecutionStatus enemyStatus = _dependencies.EnemyPhase.Execute(run, delta, recorder);

                if (enemyStatus == StageExecutionStatus.InteractionLeftPending)
                {
                    return BuildResult(buffers, run);
                }

                return BuildResult(buffers, run);
            }
            catch (System.Exception exception)
            {
                ArenaRunTickResult partialResult = BuildResult(buffers, run);

                throw new ArenaRunTickFailedException(partialResult, exception);
            }
        }

        private ArenaRunTickResult BuildResult(ArenaTickFrameBuffers buffers, ArenaRun run)
        {
            ArenaRunStateSnapshot stateSnapshot = run.CreateSnapshot(buffers.StateSnapshot);

            return buffers.BuildResult(run, stateSnapshot);
        }

        private ArenaTickFrameBuffers EnsureBuffers(ArenaRun run)
        {
            if (_buffers != null && _buffersRunId == run.Id)
            {
                return _buffers;
            }

            _buffers = new ArenaTickFrameBuffers(
                run.EnemyCapacity,
                run.CreateSnapshotBuffer());
            _buffersRunId = run.Id;

            return _buffers;
        }
    }
}
