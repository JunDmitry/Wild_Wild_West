namespace Game.Arena.Domain.Interactions.Movement
{
    public enum EnemyMovementBatchResolutionStatus
    {
        Applied = 0,
        AcceptedWithoutStateChange = 1,
        Rejected = 2,
    }
}
