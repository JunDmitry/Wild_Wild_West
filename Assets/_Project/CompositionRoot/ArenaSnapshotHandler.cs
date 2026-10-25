using System;
using Game.Arena.Application.Dispatch;
using Game.Arena.Application.ReadModels;

namespace Game.CompositionRoot
{
    /// <summary>
    /// Gives a Presentation presenter's snapshot method the dispatcher's
    /// handler shape without changing Presentation (ADR-0031).
    /// </summary>
    public sealed class ArenaSnapshotHandler : IArenaRunSnapshotHandler
    {
        private readonly Action<ArenaRunSnapshot> _present;

        public ArenaSnapshotHandler(Action<ArenaRunSnapshot> present)
        {
            _present = present ?? throw new ArgumentNullException(nameof(present));
        }

        public void Handle(ArenaRunSnapshot snapshot)
        {
            _present(snapshot);
        }
    }
}
