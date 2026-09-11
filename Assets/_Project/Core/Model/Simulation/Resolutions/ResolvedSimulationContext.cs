namespace Game.Core.Model.Simulation.Resolutions
{
    /// <summary>
    /// Represents the resolved context of a simulation step, containing the resolutions of movement, attack, and spawn queries.
    /// </summary>
    public readonly struct ResolvedSimulationContext
    {
        /// <summary>
        /// Initializes a new instance of the <see cref="ResolvedSimulationContext"/> struct.
        /// </summary>
        /// <param name="movement">The movement resolution.</param>
        /// <param name="attack">The attack resolution.</param>
        /// <param name="spawn">The spawn resolution.</param>
        public ResolvedSimulationContext(
            MovementResolution movement,
            AttackResolution attack,
            SpawnResolution spawn)
        {
            Movement = movement;
            Attack = attack;
            Spawn = spawn;
        }

        /// <summary>
        /// Gets an empty resolved simulation context.
        /// </summary>
        public static ResolvedSimulationContext Empty { get; } = new(MovementResolution.Unblocked, AttackResolution.Empty, SpawnResolution.None);

        /// <summary>
        /// Gets the movement resolution.
        /// </summary>
        public MovementResolution Movement { get; }

        /// <summary>
        /// Gets the attack resolution.
        /// </summary>
        public AttackResolution Attack { get; }

        /// <summary>
        /// Gets the spawn resolution.
        /// </summary>
        public SpawnResolution Spawn { get; }
    }
}
