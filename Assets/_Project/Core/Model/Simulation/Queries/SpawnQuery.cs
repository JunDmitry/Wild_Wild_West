using System;

namespace Game.Core.Model.Simulation.Queries
{
    /// <summary>
    /// Represents a query for spawning an enemy.
    /// </summary>
    public readonly struct SpawnQuery : IEquatable<SpawnQuery>
    {
        /// <summary>
        /// Initializes a new instance of the <see cref="SpawnQuery"/> struct.
        /// </summary>
        /// <param name="kind">The kind of spawn query.</param>
        public SpawnQuery(SpawnQueryKind kind)
        {
            Kind = kind;
        }

        /// <summary>
        /// Gets an empty spawn query.
        /// </summary>
        public static SpawnQuery None => new(SpawnQueryKind.None);

        /// <summary>
        /// Gets a spawn query for a regular enemy.
        /// </summary>
        public static SpawnQuery Regular => new(SpawnQueryKind.Regular);

        /// <summary>
        /// Gets a spawn query for a boss enemy.
        /// </summary>
        public static SpawnQuery Boss => new(SpawnQueryKind.Boss);

        /// <summary>
        /// Gets the kind of the spawn query.
        /// </summary>
        public SpawnQueryKind Kind { get; }

        /// <summary>
        /// Returns the hash code for the current spawn query.
        /// </summary>
        /// <returns>A hash code for the current spawn query.</returns>
        public override int GetHashCode()
        {
            return (int)Kind;
        }

        /// <summary>
        /// Determines whether the current instance is equal to another object.
        /// </summary>
        /// <param name="obj">The object to compare with the current instance.</param>
        /// <returns><see langword="true"/> if <paramref name="obj"/> is a <see cref="SpawnQuery"/> and is equal to the current instance; otherwise, <see langword="false"/>.</returns>
        public override bool Equals(object obj)
        {
            return obj is SpawnQuery other && Equals(other);
        }

        /// <summary>
        /// Determines whether the current instance is equal to another <see cref="SpawnQuery"/>.
        /// </summary>
        /// <param name="other">The <see cref="SpawnQuery"/> to compare with the current instance.</param>
        /// <returns><see langword="true"/> if the current instance is equal to <paramref name="other"/>; otherwise, <see langword="false"/>.</returns>
        public bool Equals(SpawnQuery other)
        {
            return Kind == other.Kind;
        }
    }
}
