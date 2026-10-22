using System;
using Game.Arena.Application.Restart;
using Game.Arena.Application.Sessions;
using Game.Arena.Domain.Aggregates.ArenaRun;
using Game.Arena.Domain.Time;

namespace Game.Arena.Application.Ticks.Stages
{
    internal sealed class DefeatRestartStage
    {
        private readonly ArenaRunSession _session;
        private readonly IRunRestartDelayPolicy _restartDelayPolicy;

        public DefeatRestartStage(
            ArenaRunSession session,
            IRunRestartDelayPolicy restartDelayPolicy)
        {
            _session = session ?? throw new ArgumentNullException(nameof(session));

            _restartDelayPolicy = restartDelayPolicy
                ?? throw new ArgumentNullException(nameof(restartDelayPolicy));
        }

        public bool Execute(
            ArenaRun run,
            GameDuration delta,
            ArenaRunTickRecorder recorder)
        {
            if (run == null)
            {
                throw new ArgumentNullException(nameof(run));
            }

            if (recorder == null)
            {
                throw new ArgumentNullException(nameof(recorder));
            }

            if (run.Status != ArenaRunStatus.Defeat)
            {
                return false;
            }

            recorder.RecordStage(ArenaRunTickStage.DefeatRestart);

            if (_restartDelayPolicy.ShouldRestart(delta) == false)
            {
                return false;
            }

            DefeatedRunRestartOutcome outcome = _session.RestartDefeatedRun();

            if (outcome.IsRestarted == false)
            {
                throw new InvalidOperationException(
                    "The defeated ArenaRun could not be restarted.");
            }

            return true;
        }
    }
}
