using System;
using Game.Arena.Application.Dispatch;
using Game.Arena.Application.Hud;
using Game.Arena.Application.ReadModels;

namespace Game.CompositionRoot
{
    /// <summary>
    /// Gives a Presentation HUD presenter the dispatcher's HUD handler shape
    /// (ADR-0027 amendment, ADR-0031).
    /// </summary>
    public sealed class ArenaHudHandler : IArenaRunHudHandler
    {
        private readonly Action<ArenaRunSnapshot, ArenaRunHudSnapshot> _present;

        public ArenaHudHandler(Action<ArenaRunSnapshot, ArenaRunHudSnapshot> present)
        {
            _present = present ?? throw new ArgumentNullException(nameof(present));
        }

        public void Handle(ArenaRunSnapshot snapshot, ArenaRunHudSnapshot hud)
        {
            _present(snapshot, hud);
        }
    }
}
