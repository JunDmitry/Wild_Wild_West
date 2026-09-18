using Game.Arena.Domain.Identity;

namespace Game.Arena.Application.Identity
{
    public sealed class MonotonicArenaRunIdSource : IArenaRunIdSource
    {
        private readonly MonotonicIdentitySequence _sequence;

        public MonotonicArenaRunIdSource()
        {
            _sequence = new MonotonicIdentitySequence(1UL);
        }

        internal MonotonicArenaRunIdSource(ulong firstValue)
        {
            _sequence = new MonotonicIdentitySequence(firstValue);
        }

        public ArenaRunId Allocate()
        {
            return ArenaRunId.FromValue(_sequence.Allocate());
        }
    }
}
