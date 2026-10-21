namespace Game.Arena.Domain.Interactions.Attack
{
    public enum PlayerAttackImpactRejectionReason
    {
        None = 0,
        ForeignArenaRun = 1,
        UnknownInteraction = 2,
        InteractionClosed = 3,
        StaleRevision = 4,
        KindMismatch = 5,
        UnknownTarget = 6,
        TargetOutOfRange = 7,
        TooManyTargets = 8,
    }
}
