using System.Collections.Generic;
using Game.Arena.Application.Identity;
using Game.Arena.Domain.Identity;

namespace Game.Arena.Application.Tests.Sessions
{
    internal sealed class RecordingPlayerIdSource : IPlayerIdSource
    {
        private readonly List<PlayerId> _allocated = new();

        private ulong _nextValue = 1UL;

        public IReadOnlyList<PlayerId> Allocated => _allocated;

        public PlayerId Allocate()
        {
            PlayerId id = PlayerId.FromValue(_nextValue);
            _nextValue++;
            _allocated.Add(id);

            return id;
        }
    }
}
