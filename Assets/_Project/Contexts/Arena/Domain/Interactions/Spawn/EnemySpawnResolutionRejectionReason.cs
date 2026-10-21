namespace Game.Arena.Domain.Interactions.Spawn
{
    public enum EnemySpawnResolutionRejectionReason
    {
        None = 0,
        ForeignArenaRun = 1,
        UnknownInteraction = 2,
        InteractionClosed = 3,
        StaleRevision = 4,
        KindMismatch = 5,
        EnemyIdIsNone = 6,
        DuplicateEnemyId = 7,
        SpawnPositionOutsidePerimeter = 8,
    }
}
