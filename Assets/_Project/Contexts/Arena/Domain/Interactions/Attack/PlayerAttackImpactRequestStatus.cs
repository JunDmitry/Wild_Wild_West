namespace Game.Arena.Domain.Interactions.Attack
{
    public enum PlayerAttackImpactRequestStatus
    {
        Requested = 0,
        RunIsNotPlaying = 1,
        NoPendingAttack = 2,
        ImpactNotDue = 3,
    }
}
