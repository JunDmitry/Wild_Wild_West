namespace Game.Core.Model.Enums
{
    /// <summary>
    /// Represents the current phase of the game.
    /// </summary>
    public enum GamePhase
    {
        /// <summary>
        /// The game is currently loading resources and initializing.
        /// </summary>
        Loading = 0,

        /// <summary>
        /// The game is actively being played by the user.
        /// </summary>
        Playing = 1,

        /// <summary>
        /// The player has lost the game (defeat condition met).
        /// </summary>
        Defeat = 2,

        /// <summary>
        /// The player has won the game (victory condition met).
        /// </summary>
        Victory = 3,
    }
}
