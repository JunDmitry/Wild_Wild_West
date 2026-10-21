namespace Game.Arena.Domain.Aggregates.ArenaRun
{
    public enum PlayerAttackStartStatus
    {
        Started = 0,
        RunIsNotPlaying = 1,
        InteractionPending = 2,
        AttackAlreadyPending = 3,
        WeaponNotReady = 4,
    }
}
