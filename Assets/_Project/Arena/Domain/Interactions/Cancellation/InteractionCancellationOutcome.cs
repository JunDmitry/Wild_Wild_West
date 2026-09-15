using System;

namespace Game.Arena.Domain.Interactions
{
    public sealed class InteractionCancellationOutcome
    {
        private InteractionCancellationOutcome(
            InteractionCancellationStatus status,
            InteractionCancellationReason reason)
        {
            if (reason == InteractionCancellationReason.None)
            {
                throw new ArgumentOutOfRangeException(nameof(reason));
            }

            Status = status;
            Reason = reason;
        }

        public InteractionCancellationStatus Status { get; }

        public InteractionCancellationReason Reason { get; }

        public bool IsCancelled => Status == InteractionCancellationStatus.Cancelled;

        public static InteractionCancellationOutcome Cancelled(InteractionCancellationReason reason)
        {
            return new InteractionCancellationOutcome(InteractionCancellationStatus.Cancelled, reason);
        }

        public static InteractionCancellationOutcome NoPendingInteraction(InteractionCancellationReason reason)
        {
            return new InteractionCancellationOutcome(InteractionCancellationStatus.NoPendingInteraction, reason);
        }

        public static InteractionCancellationOutcome CorrelationMismatch(InteractionCancellationReason reason)
        {
            return new InteractionCancellationOutcome(InteractionCancellationStatus.CorrelationMismatch, reason);
        }
    }
}
