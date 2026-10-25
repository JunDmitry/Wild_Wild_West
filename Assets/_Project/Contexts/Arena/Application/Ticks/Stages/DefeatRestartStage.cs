using System;
using Game.Arena.Application.Ports;
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
        private readonly IArenaSceneReloader _sceneReloader;

        public DefeatRestartStage(
            ArenaRunSession session,
            IRunRestartDelayPolicy restartDelayPolicy,
            IArenaSceneReloader sceneReloader)
        {
            _session = session ?? throw new ArgumentNullException(nameof(session));

            _restartDelayPolicy = restartDelayPolicy
                ?? throw new ArgumentNullException(nameof(restartDelayPolicy));

            _sceneReloader = sceneReloader
                ?? throw new ArgumentNullException(nameof(sceneReloader));
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

            // The restarted run exists now; request its scene exactly once, on
            // the only edge where a defeat becomes a live run.
            _sceneReloader.ReloadArenaScene();

            return true;
        }
    }
}
