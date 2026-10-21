namespace Game.Arena.Domain.Interactions.Movement
{
    public enum EnemyMovementBatchRequestStatus
    {
        Requested = 0,
        RunIsNotPlaying = 1,
        NoEligibleEnemies = 2,
    }
}
