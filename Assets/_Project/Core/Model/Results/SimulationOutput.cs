using Game.Core.Model.Entities;
using Game.Core.Model.States;

namespace Game.Core.Model.Results
{
    /// <summary>
    /// Represents the output of a simulation step, containing the updated state, the result of the step, and the next entity identifier.
    /// </summary>
    public readonly struct SimulationOutput
    {
        /// <summary>
        /// Initializes a new instance of the <see cref="SimulationOutput"/> struct.
        /// </summary>
        /// <param name="state">The updated game state.</param>
        /// <param name="result">The result of the simulation step.</param>
        /// <param name="nextEntityId">The next entity identifier to be used.</param>
        public SimulationOutput(
            GameState state,
            SimulationResult result,
            EntityId nextEntityId)
        {
            State = state;
            Result = result;
            NextEntityId = nextEntityId;
        }

        /// <summary>
        /// Gets the updated game state.
        /// </summary>
        public GameState State { get; }

        /// <summary>
        /// Gets the result of the simulation step.
        /// </summary>
        public SimulationResult Result { get; }

        /// <summary>
        /// Gets the next entity identifier to be used.
        /// </summary>
        public EntityId NextEntityId { get; }
    }
}
