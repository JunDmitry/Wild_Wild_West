using System;

namespace Game.Core.Model.Simulation.Queries
{
    /// <summary>
    /// Represents a plan for a simulation step, containing optional movement, attack, and spawn queries.
    /// </summary>
    public readonly struct SimulationPlan : IEquatable<SimulationPlan>
    {
        /// <summary>
        /// Initializes a new instance of the <see cref="SimulationPlan"/> struct.
        /// </summary>
        /// <param name="hasMovementQuery">A value indicating whether the plan contains a movement query.</param>
        /// <param name="movement">The movement query.</param>
        /// <param name="attack">The attack query.</param>
        /// <param name="spawn">The spawn query.</param>
        public SimulationPlan(
            bool hasMovementQuery,
            MovementQuery movement,
            AttackQuery attack,
            SpawnQuery spawn)
        {
            HasMovementQuery = hasMovementQuery;
            Movement = movement;
            Attack = attack;
            Spawn = spawn;
        }

        /// <summary>
        /// Gets an empty simulation plan.
        /// </summary>
        public static SimulationPlan Empty { get; } = new(
            hasMovementQuery: false,
            movement: default,
            attack: AttackQuery.None,
            spawn: SpawnQuery.None);

        /// <summary>
        /// Gets a value indicating whether the plan contains a movement query.
        /// </summary>
        public bool HasMovementQuery { get; }

        /// <summary>
        /// Gets the movement query.
        /// </summary>
        public MovementQuery Movement { get; }

        /// <summary>
        /// Gets the attack query.
        /// </summary>
        public AttackQuery Attack { get; }

        /// <summary>
        /// Gets the spawn query.
        /// </summary>
        public SpawnQuery Spawn { get; }

        /// <summary>
        /// Returns the hash code for the current simulation plan.
        /// </summary>
        /// <returns>A hash code for the current simulation plan.</returns>
        public override int GetHashCode()
        {
            return HashCode.Combine(
                HasMovementQuery,
                Movement,
                Attack,
                Spawn);
        }

        /// <summary>
        /// Determines whether the current instance is equal to another object.
        /// </summary>
        /// <param name="obj">The object to compare with the current instance.</param>
        /// <returns><see langword="true"/> if <paramref name="obj"/> is a <see cref="SimulationPlan"/> and is equal to the current instance; otherwise, <see langword="false"/>.</returns>
        public override bool Equals(object obj)
        {
            return obj is SimulationPlan other && Equals(other);
        }

        /// <summary>
        /// Determines whether the current instance is equal to another <see cref="SimulationPlan"/>.
        /// </summary>
        /// <param name="other">The <see cref="SimulationPlan"/> to compare with the current instance.</param>
        /// <returns><see langword="true"/> if the current instance is equal to <paramref name="other"/>; otherwise, <see langword="false"/>.</returns>
        public bool Equals(SimulationPlan other)
        {
            return HasMovementQuery == other.HasMovementQuery
                && Movement.Equals(other.Movement)
                && Attack.Equals(other.Attack)
                && Spawn.Equals(other.Spawn);
        }
    }
}
