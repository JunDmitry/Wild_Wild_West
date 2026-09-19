using System.Collections.Generic;
using Game.Arena.Application.Identity;
using Game.Arena.Domain.Identity;

namespace Game.Arena.Application.Tests.Sessions
{
    internal sealed class RecordingEnemyIdSource : IEnemyIdSource
    {
        private readonly List<EnemyId> _allocated = new();
        private ulong _nextValue = 1UL;

        public IReadOnlyList<EnemyId> Allocated => _allocated;

        public EnemyId Allocate()
        {
            EnemyId id = EnemyId.FromValue(_nextValue);
            _nextValue++;
            _allocated.Add(id);

            return id;
        }
    }
}
