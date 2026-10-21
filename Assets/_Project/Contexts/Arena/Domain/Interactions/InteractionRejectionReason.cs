namespace Game.Arena.Domain.Interactions
{
    public enum InteractionRejectionReason
    {
        None = 0,
        ForeignArenaRun = 1,
        UnknownInteraction = 2,
        InteractionClosed = 3,
        StaleRevision = 4,
        KindMismatch = 5,
    }
}
