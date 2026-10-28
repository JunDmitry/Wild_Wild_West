using System;
using Game.Arena.Domain.Concurrency;
using Game.Arena.Domain.Identity;

namespace Game.Arena.Domain.Events
{
    public sealed class WaveCompleted : IPooledArenaDomainEvent, IEquatable<WaveCompleted>
    {
        internal WaveCompleted()
        {
        }

        public WaveCompleted(
            ArenaRunId arenaRunId,
            AggregateRevision aggregateRevision,
            WaveNumber waveNumber)
        {
            if (arenaRunId.IsNone)
            {
                throw new ArgumentException("ArenaRunId cannot be None.", nameof(arenaRunId));
            }

            ArenaRunId = arenaRunId;
            AggregateRevision = aggregateRevision;
            WaveNumber = waveNumber;
        }

        public ArenaRunId ArenaRunId { get; private set; }

        public AggregateRevision AggregateRevision { get; private set; }

        public WaveNumber WaveNumber { get; private set; }

        internal void Fill(
            ArenaRunId arenaRunId,
            AggregateRevision aggregateRevision,
            WaveNumber waveNumber)
        {
            ArenaRunId = arenaRunId;
            AggregateRevision = aggregateRevision;
            WaveNumber = waveNumber;
        }

        void IPooledArenaDomainEvent.Reset()
        {
            ArenaRunId = default;
            AggregateRevision = default;
            WaveNumber = default;
        }

        public bool Equals(WaveCompleted other)
        {
            if (other is null)
            {
                return false;
            }

            return ArenaRunId.Equals(other.ArenaRunId)
                && AggregateRevision.Equals(other.AggregateRevision)
                && WaveNumber.Equals(other.WaveNumber);
        }

        public override bool Equals(object obj)
        {
            return obj is WaveCompleted other && Equals(other);
        }

        public override int GetHashCode()
        {
            return HashCode.Combine(ArenaRunId, AggregateRevision, WaveNumber);
        }
    }
}
