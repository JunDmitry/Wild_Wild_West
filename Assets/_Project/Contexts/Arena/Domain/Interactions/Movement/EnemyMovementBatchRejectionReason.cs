namespace Game.Arena.Domain.Interactions.Movement
{
    public enum EnemyMovementBatchRejectionReason
    {
        None = 0,
        ForeignArenaRun = 1,
        UnknownInteraction = 2,
        InteractionClosed = 3,
        StaleRevision = 4,
        KindMismatch = 5,
        EnemyIdIsNone = 6,
        MissingEnemyId = 7,
        UnexpectedEnemyId = 8,
        DuplicateEnemyId = 9,
        EnemyNoLongerActive = 10,
        AcceptedPositionOffGroundPlane = 11,
        AcceptedPositionBehindRequest = 12,
        AcceptedPositionBeyondRequestedDistance = 13,
        AcceptedPositionOffMovementPath = 14,
    }
}
