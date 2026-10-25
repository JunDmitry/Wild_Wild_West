using System;
using Game.Arena.Application.Sessions;
using VContainer.Unity;

namespace Game.CompositionRoot
{
    /// <summary>
    /// Creates the run when the arena is entered (ADR-0032). A run that already
    /// exists is the defeat-reload path (ADR-0029): the tick restarted it, so
    /// this driver creates nothing and the first dispatch presents it.
    /// </summary>
    public sealed class ArenaRunStartupDriver : IStartable
    {
        private readonly ArenaRunSession _session;

        public ArenaRunStartupDriver(ArenaRunSession session)
        {
            _session = session ?? throw new ArgumentNullException(nameof(session));
        }

        public void Start()
        {
            InitialRunStartOutcome outcome = _session.StartInitialRun();

            switch (outcome.Status)
            {
                case InitialRunStartStatus.Started:
                case InitialRunStartStatus.ActiveRunAlreadyExists:
                    return;

                default:
                    throw new InvalidOperationException(
                        "Unexpected initial run start status: " + outcome.Status);
            }
        }
    }
}
