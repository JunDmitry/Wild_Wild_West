using System;

namespace Game.Arena.Domain.Interactions
{
    public sealed class InteractionCancellationOutcome
    {
        internal InteractionCancellationOutcome()
        {
        }

        private InteractionCancellationOutcome(
            InteractionCancellationStatus status,
            InteractionCancellationReason reason)
        {
            Validate(reason);
            Status = status;
            Reason = reason;
        }

        public InteractionCancellationStatus Status { get; private set; }

        public InteractionCancellationReason Reason { get; private set; }

        public bool IsCancelled => Status == InteractionCancellationStatus.Cancelled;

        internal void Fill(InteractionCancellationStatus status, InteractionCancellationReason reason)
        {
            Validate(reason);
            Status = status;
            Reason = reason;
        }

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

        private static void Validate(InteractionCancellationReason reason)
        {
            if (reason == InteractionCancellationReason.None)
            {
                throw new ArgumentOutOfRangeException(nameof(reason));
            }
        }
    }
}
