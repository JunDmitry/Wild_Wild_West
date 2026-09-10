namespace Game.Core.Model.Enums
{
    /// <summary>
    /// Specifies the status of the boss.
    /// </summary>
    public enum BossStatus
    {
        /// <summary>
        /// The boss has not spawned yet.
        /// </summary>
        NotSpawned = 0,

        /// <summary>
        /// The boss is currently alive.
        /// </summary>
        Alive = 1,

        /// <summary>
        /// The boss has been defeated.
        /// </summary>
        Defeated = 2
    }
}
