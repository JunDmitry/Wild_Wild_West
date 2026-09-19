using System.Collections.Generic;
using Game.Arena.Application.Identity;
using Game.Arena.Domain.Identity;

namespace Game.Arena.Application.Tests.Sessions
{
    internal sealed class RecordingArenaRunIdSource : IArenaRunIdSource
    {
        private readonly List<ArenaRunId> _allocated = new();

        private ulong _nextValue = 1UL;

        public IReadOnlyList<ArenaRunId> Allocated => _allocated;

        public ArenaRunId Allocate()
        {
            ArenaRunId id = ArenaRunId.FromValue(_nextValue);
            _nextValue++;
            _allocated.Add(id);

            return id;
        }
    }
}
