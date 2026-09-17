using System;
using Game.Arena.Domain.Concurrency;
using Game.Arena.Domain.Identity;

namespace Game.Arena.Domain.Events
{
    public readonly struct WaveCompleted : IArenaDomainEvent, IEquatable<WaveCompleted>
    {
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

        public ArenaRunId ArenaRunId { get; }

        public AggregateRevision AggregateRevision { get; }

        public WaveNumber WaveNumber { get; }

        public bool Equals(WaveCompleted other)
        {
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
