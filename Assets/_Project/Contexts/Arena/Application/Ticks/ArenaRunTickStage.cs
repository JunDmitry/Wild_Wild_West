namespace Game.Arena.Application.Ticks
{
    public enum ArenaRunTickStage
    {
        PendingInteractionCancellation = 0,
        TimeAdvance = 1,
        WeaponSwitch = 2,
        PlayerMovement = 3,
        PlayerAttackStart = 4,
        PlayerAttackImpact = 5,
        EnemyMovement = 6,
        EnemyAttackStart = 7,
        EnemyAttackImpact = 8,
        EnemySpawn = 9,
    }
}
