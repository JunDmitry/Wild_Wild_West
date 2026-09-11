namespace Game.Core.Model.Simulation.Queries
{
    /// <summary>
    /// Specifies the kind of attack query.
    /// </summary>
    public enum AttackQueryKind
    {
        /// <summary>
        /// No attack query.
        /// </summary>
        None = 0,

        /// <summary>
        /// A ranged raycast attack query.
        /// </summary>
        RangedRaycast = 1,

        /// <summary>
        /// A melee overlap attack query.
        /// </summary>
        MeleeOverlap = 2,
    }

    /// <summary>
    /// Specifies the kind of spawn query.
    /// </summary>
    public enum SpawnQueryKind
    {
        /// <summary>
        /// No spawn query.
        /// </summary>
        None = 0,

        /// <summary>
        /// A regular enemy spawn query.
        /// </summary>
        Regular = 1,

        /// <summary>
        /// A boss enemy spawn query.
        /// </summary>
        Boss = 2,
    }
}
