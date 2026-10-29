using System;
using Game.Arena.Application.Diagnostics;
using Game.Arena.Application.Sessions;
using Game.Arena.Application.Ticks;
using VContainer.Unity;

namespace Game.CompositionRoot
{
    /// <summary>
    /// The Arena frame loop (ADR-0031). One tick per frame while the process
    /// session owns an active run; T-8.3 starts that run. The driver never
    /// starts or restarts a run itself.
    /// </summary>
    public sealed class ArenaTickDriver : ITickable
    {
        private readonly ArenaRunTickService _tickService;
        private readonly ArenaRunSession _session;

        public ArenaTickDriver(ArenaRunTickService tickService, ArenaRunSession session)
        {
            _tickService = tickService
                ?? throw new ArgumentNullException(nameof(tickService));

            _session = session
                ?? throw new ArgumentNullException(nameof(session));
        }

        public void Tick()
        {
            if (_session.HasActiveRun == false)
            {
                return;
            }

            // TEMPORARY attribution switch (t9-gc-residual-b); reverted before
            // the acceptance runs.
            if (AttributionFlags.NotTick)
            {
                return;
            }

            _tickService.ExecuteTick();
        }
    }
}
