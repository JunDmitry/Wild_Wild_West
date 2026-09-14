using System;
using Game.Arena.Domain.Concurrency;
using Game.Arena.Domain.Identity;

namespace Game.Arena.Domain.Interactions
{
    public readonly struct InteractionCorrelation : IEquatable<InteractionCorrelation>
    {
        public InteractionCorrelation(
            ArenaRunId arenaRunId,
            InteractionId interactionId,
            AggregateRevision aggregateRevision)
        {
            if (arenaRunId.IsNone)
            {
                throw new ArgumentException("ArenaRunId cannot be None.", nameof(arenaRunId));
            }

            if (interactionId.IsNone)
            {
                throw new ArgumentException("InteractionId cannot be None.", nameof(interactionId));
            }

            ArenaRunId = arenaRunId;
            InteractionId = interactionId;
            AggregateRevision = aggregateRevision;
        }

        public ArenaRunId ArenaRunId { get; }
        public InteractionId InteractionId { get; }
        public AggregateRevision AggregateRevision { get; }

        public bool Equals(InteractionCorrelation other)
        {
            return ArenaRunId.Equals(other.ArenaRunId)
                && InteractionId.Equals(other.InteractionId)
                && AggregateRevision.Equals(other.AggregateRevision);
        }

        public override bool Equals(object obj)
        {
            return obj is InteractionCorrelation other && Equals(other);
        }

        public override int GetHashCode()
        {
            return HashCode.Combine(
                ArenaRunId,
                InteractionId,
                AggregateRevision);
        }

        public override string ToString()
        {
            return "Interaction(" + ArenaRunId.ToString()
                + ", " + InteractionId.ToString()
                + ", " + AggregateRevision.ToString() + ")";
        }

        public static bool operator ==(InteractionCorrelation left, InteractionCorrelation right)
        {
            return left.Equals(right);
        }

        public static bool operator !=(InteractionCorrelation left, InteractionCorrelation right)
        {
            return !left.Equals(right);
        }
    }
}
