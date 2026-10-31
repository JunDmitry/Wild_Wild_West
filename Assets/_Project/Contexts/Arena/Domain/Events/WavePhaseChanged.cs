using System;
using Game.Arena.Domain.Aggregates.ArenaRun;
using Game.Arena.Domain.Concurrency;
using Game.Arena.Domain.Identity;

namespace Game.Arena.Domain.Events
{
    public sealed class WavePhaseChanged : IPooledArenaDomainEvent, IEquatable<WavePhaseChanged>
    {
        public WavePhaseChanged(
            ArenaRunId arenaRunId,
            AggregateRevision aggregateRevision,
            WaveNumber waveNumber,
            WavePhase previousPhase,
            WavePhase newPhase)
        {
            if (arenaRunId.IsNone)
            {
                throw new ArgumentException("ArenaRunId cannot be None.", nameof(arenaRunId));
            }

            if (previousPhase == newPhase)
            {
                throw new ArgumentException("Wave phase must change.", nameof(newPhase));
            }

            ArenaRunId = arenaRunId;
            AggregateRevision = aggregateRevision;
            WaveNumber = waveNumber;
            PreviousPhase = previousPhase;
            NewPhase = newPhase;
        }

        internal WavePhaseChanged()
        {
        }

        public ArenaRunId ArenaRunId { get; private set; }

        public AggregateRevision AggregateRevision { get; private set; }

        public WaveNumber WaveNumber { get; private set; }

        public WavePhase PreviousPhase { get; private set; }

        public WavePhase NewPhase { get; private set; }

        public bool Equals(WavePhaseChanged other)
        {
            if (other is null)
            {
                return false;
            }

            return ArenaRunId.Equals(other.ArenaRunId)
                && AggregateRevision.Equals(other.AggregateRevision)
                && WaveNumber.Equals(other.WaveNumber)
                && PreviousPhase == other.PreviousPhase
                && NewPhase == other.NewPhase;
        }

        public override bool Equals(object obj)
        {
            return obj is WavePhaseChanged other && Equals(other);
        }

        public override int GetHashCode()
        {
            return HashCode.Combine(ArenaRunId, AggregateRevision, WaveNumber, PreviousPhase, NewPhase);
        }

        internal void Fill(
            ArenaRunId arenaRunId,
            AggregateRevision aggregateRevision,
            WaveNumber waveNumber,
            WavePhase previousPhase,
            WavePhase newPhase)
        {
            ArenaRunId = arenaRunId;
            AggregateRevision = aggregateRevision;
            WaveNumber = waveNumber;
            PreviousPhase = previousPhase;
            NewPhase = newPhase;
        }

        void IPooledArenaDomainEvent.Reset()
        {
            ArenaRunId = default;
            AggregateRevision = default;
            WaveNumber = default;
            PreviousPhase = default;
            NewPhase = default;
        }
    }
}
