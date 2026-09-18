using Game.Arena.Domain.Identity;

namespace Game.Arena.Application.Identity
{
    public sealed class MonotonicPlayerIdSource : IPlayerIdSource
    {
        private readonly MonotonicIdentitySequence _sequence;

        public MonotonicPlayerIdSource()
        {
            _sequence = new MonotonicIdentitySequence(1UL);
        }

        internal MonotonicPlayerIdSource(ulong firstValue)
        {
            _sequence = new MonotonicIdentitySequence(firstValue);
        }

        public PlayerId Allocate()
        {
            return PlayerId.FromValue(_sequence.Allocate());
        }
    }
}
