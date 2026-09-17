namespace Game.Arena.Domain.Aggregates.ArenaRun
{
    public enum EnemyAttackStartStatus
    {
        Started = 0,
        NoEligibleEnemies = 1,
        RunIsNotPlaying = 2,
        InteractionPending = 3,
    }
}
