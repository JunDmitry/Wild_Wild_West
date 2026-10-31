using System;
using Game.Arena.Domain.Aggregates.ArenaRun;
using Game.Arena.Domain.Concurrency;
using Game.Arena.Domain.Identity;

namespace Game.Arena.Domain.Events
{
    public sealed class WaveStarted : IPooledArenaDomainEvent, IEquatable<WaveStarted>
    {
        public WaveStarted(
            ArenaRunId arenaRunId,
            AggregateRevision aggregateRevision,
            WaveNumber waveNumber,
            WavePhase initialPhase)
        {
            if (arenaRunId.IsNone)
            {
                throw new ArgumentException("ArenaRunId cannot be None.", nameof(arenaRunId));
            }

            ArenaRunId = arenaRunId;
            AggregateRevision = aggregateRevision;
            WaveNumber = waveNumber;
            InitialPhase = initialPhase;
        }

        internal WaveStarted()
        {
        }

        public ArenaRunId ArenaRunId { get; private set; }

        public AggregateRevision AggregateRevision { get; private set; }

        public WaveNumber WaveNumber { get; private set; }

        public WavePhase InitialPhase { get; private set; }

        public bool Equals(WaveStarted other)
        {
            if (other is null)
            {
                return false;
            }

            return ArenaRunId.Equals(other.ArenaRunId)
                && AggregateRevision.Equals(other.AggregateRevision)
                && WaveNumber.Equals(other.WaveNumber)
                && InitialPhase == other.InitialPhase;
        }

        public override bool Equals(object obj)
        {
            return obj is WaveStarted other && Equals(other);
        }

        public override int GetHashCode()
        {
            return HashCode.Combine(ArenaRunId, AggregateRevision, WaveNumber, InitialPhase);
        }

        internal void Fill(
            ArenaRunId arenaRunId,
            AggregateRevision aggregateRevision,
            WaveNumber waveNumber,
            WavePhase initialPhase)
        {
            ArenaRunId = arenaRunId;
            AggregateRevision = aggregateRevision;
            WaveNumber = waveNumber;
            InitialPhase = initialPhase;
        }

        void IPooledArenaDomainEvent.Reset()
        {
            ArenaRunId = default;
            AggregateRevision = default;
            WaveNumber = default;
            InitialPhase = default;
        }
    }
}
