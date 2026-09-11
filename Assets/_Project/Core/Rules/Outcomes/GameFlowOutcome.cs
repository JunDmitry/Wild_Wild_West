using Game.Core.Model.Enums;
using Game.Core.Model.States;

namespace Game.Core.Rules.Outcomes
{
    /// <summary>
    /// Represents the outcome of the game flow update, including phase changes and restart requests.
    /// </summary>
    public readonly struct GameFlowOutcome
    {
        /// <summary>
        /// Initializes a new instance of the <see cref="GameFlowOutcome"/> struct.
        /// </summary>
        /// <param name="state">The updated game state.</param>
        /// <param name="gamePhaseChanged">A value indicating whether the game phase changed.</param>
        /// <param name="newGamePhase">The new game phase if it changed; otherwise, the previous phase.</param>
        /// <param name="waveStarted">A value indicating whether a wave started.</param>
        /// <param name="waveNumber">The number of the started wave, if any.</param>
        /// <param name="restartRequested">A value indicating whether a restart was requested.</param>
        public GameFlowOutcome(
            GameState state,
            bool gamePhaseChanged,
            GamePhase newGamePhase,
            bool waveStarted,
            int waveNumber,
            bool restartRequested)
        {
            State = state;
            GamePhaseChanged = gamePhaseChanged;
            NewGamePhase = newGamePhase;
            WaveStarted = waveStarted;
            WaveNumber = waveNumber;
            RestartRequested = restartRequested;
        }

        /// <summary>
        /// Gets the updated game state.
        /// </summary>
        public GameState State { get; }

        /// <summary>
        /// Gets a value indicating whether the game phase changed.
        /// </summary>
        public bool GamePhaseChanged { get; }

        /// <summary>
        /// Gets the new game phase if it changed; otherwise, the previous phase.
        /// </summary>
        public GamePhase NewGamePhase { get; }

        /// <summary>
        /// Gets a value indicating whether a wave started.
        /// </summary>
        public bool WaveStarted { get; }

        /// <summary>
        /// Gets the number of the started wave, if any.
        /// </summary>
        public int WaveNumber { get; }

        /// <summary>
        /// Gets a value indicating whether a restart was requested.
        /// </summary>
        public bool RestartRequested { get; }
    }
}
