using Game.Arena.Domain.Identity;

namespace Game.Arena.Application.Identity
{
    public sealed class MonotonicEnemyIdSource : IEnemyIdSource
    {
        private readonly MonotonicIdentitySequence _sequence;

        public MonotonicEnemyIdSource()
        {
            _sequence = new MonotonicIdentitySequence(1UL);
        }

        internal MonotonicEnemyIdSource(ulong firstValue)
        {
            _sequence = new MonotonicIdentitySequence(firstValue);
        }

        public EnemyId Allocate()
        {
            return EnemyId.FromValue(_sequence.Allocate());
        }
    }
}
