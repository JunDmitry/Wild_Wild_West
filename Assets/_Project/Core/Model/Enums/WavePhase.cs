namespace Game.Core.Model.Enums
{
    /// <summary>
    /// Represents the current phase of a wave in the game.
    /// </summary>
    public enum WavePhase
    {
        /// <summary>
        /// The wave consists of regular enemies and standard combat.
        /// </summary>
        RegularCombat = 0,

        /// <summary>
        /// The wave features a boss encounter.
        /// </summary>
        BossCombat = 1,

        /// <summary>
        /// The wave has been completed successfully.
        /// </summary>
        Completed = 2,
    }
}
