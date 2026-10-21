using System;
using Game.Arena.Domain.Aggregates.ArenaRun;
using Game.Arena.Domain.Concurrency;
using Game.Arena.Domain.Identity;

namespace Game.Arena.Domain.Events
{
    public readonly struct WavePhaseChanged : IArenaDomainEvent, IEquatable<WavePhaseChanged>
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

        public ArenaRunId ArenaRunId { get; }

        public AggregateRevision AggregateRevision { get; }

        public WaveNumber WaveNumber { get; }

        public WavePhase PreviousPhase { get; }

        public WavePhase NewPhase { get; }

        public bool Equals(WavePhaseChanged other)
        {
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
    }
}
