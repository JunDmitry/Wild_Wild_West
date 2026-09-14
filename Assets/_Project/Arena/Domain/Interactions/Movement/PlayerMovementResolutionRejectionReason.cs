namespace Game.Arena.Domain.Interactions.Movement
{
    public enum PlayerMovementResolutionRejectionReason
    {
        None = 0,
        ForeignArenaRun = 1,
        UnknownInteraction = 2,
        InteractionClosed = 3,
        StaleRevision = 4,
        KindMismatch = 4,
        AcceptedPositionOutsideArena = 6,
    }
}
