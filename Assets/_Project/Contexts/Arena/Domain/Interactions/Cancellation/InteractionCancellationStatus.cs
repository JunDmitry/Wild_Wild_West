namespace Game.Arena.Domain.Interactions
{
    public enum InteractionCancellationStatus
    {
        Cancelled = 0,
        NoPendingInteraction = 1,
        CorrelationMismatch = 2,
    }
}
