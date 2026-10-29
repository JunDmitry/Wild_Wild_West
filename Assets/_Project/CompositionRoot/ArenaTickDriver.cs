using System;
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

        // TEMPORARY 3a positive control (t9-gc-metric-3-baseline): a deliberate
        // 4096-byte allocation from this game-code method, fired once inside
        // the capture window so a capture can prove it resolves a game frame.
        // Reverted before the acceptance runs.
        private static byte[] _attributionPositiveControl;
        private static int _attributionTicks;

        public ArenaTickDriver(ArenaRunTickService tickService, ArenaRunSession session)
        {
            _tickService = tickService
                ?? throw new ArgumentNullException(nameof(tickService));

            _session = session
                ?? throw new ArgumentNullException(nameof(session));
        }

        public void Tick()
        {
            if (_attributionTicks < 1350)
            {
                _attributionTicks++;

                if (_attributionTicks == 1350)
                {
                    _attributionPositiveControl = new byte[4096];
                }
            }

            if (_session.HasActiveRun == false)
            {
                return;
            }

            _tickService.ExecuteTick();
        }
    }
}
