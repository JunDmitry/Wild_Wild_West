namespace Game.Arena.Domain.Aggregates.ArenaRun
{
    public enum EnemyAttackImpactStatus
    {
        Resolved = 0,
        NoAttacksDue = 1,
        RunIsNotPlaying = 2,
        InteractionPending = 3,
        DefeatNotSupported = 4,
    }
}
