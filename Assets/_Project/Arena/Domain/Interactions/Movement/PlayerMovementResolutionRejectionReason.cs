namespace Game.Arena.Domain.Interactions.Movement
{
    public enum PlayerMovementResolutionRejectionReason
    {
        None = 0,
        ForeignArenaRun = 1,
        UnknownInteraction = 2,
        InteractionClosed = 3,
        StaleRevision = 4,
        KindMismatch = 5,
        AcceptedPositionBehindRequest = 6,
        AcceptedPositionBeyondRequestedDistance = 7,
        AcceptedPositionOffMovementPath = 8,
        AcceptedPositionOutsideArena = 9,
    }
}
