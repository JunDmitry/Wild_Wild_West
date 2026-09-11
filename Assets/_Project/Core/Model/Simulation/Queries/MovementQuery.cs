using System;
using Game.Core.Model.Entities;

namespace Game.Core.Model.Simulation.Queries
{
    /// <summary>
    /// Represents a query for movement from one position to another with a given radius.
    /// </summary>
    public readonly struct MovementQuery : IEquatable<MovementQuery>
    {
        /// <summary>
        /// Initializes a new instance of the <see cref="MovementQuery"/> struct.
        /// </summary>
        /// <param name="from">The starting position of the movement.</param>
        /// <param name="to">The target position of the movement.</param>
        /// <param name="radius">The radius of the moving entity.</param>
        public MovementQuery(
            Position3D from,
            Position3D to,
            float radius)
        {
            From = from;
            To = to;
            Radius = radius;
        }

        /// <summary>
        /// Gets the starting position of the movement.
        /// </summary>
        public Position3D From { get; }

        /// <summary>
        /// Gets the target position of the movement.
        /// </summary>
        public Position3D To { get; }

        /// <summary>
        /// Gets the radius of the moving entity.
        /// </summary>
        public float Radius { get; }

        /// <summary>
        /// Returns the hash code for the current movement query.
        /// </summary>
        /// <returns>A hash code for the current movement query.</returns>
        public override int GetHashCode()
        {
            return HashCode.Combine(
                From,
                To,
                Radius);
        }

        /// <summary>
        /// Determines whether the current instance is equal to another object.
        /// </summary>
        /// <param name="obj">The object to compare with the current instance.</param>
        /// <returns><see langword="true"/> if <paramref name="obj"/> is a <see cref="MovementQuery"/> and is equal to the current instance; otherwise, <see langword="false"/>.</returns>
        public override bool Equals(object obj)
        {
            return obj is MovementQuery other && Equals(other);
        }

        /// <summary>
        /// Determines whether the current instance is equal to another <see cref="MovementQuery"/>.
        /// </summary>
        /// <param name="other">The <see cref="MovementQuery"/> to compare with the current instance.</param>
        /// <returns><see langword="true"/> if the current instance is equal to <paramref name="other"/>; otherwise, <see langword="false"/>.</returns>
        public bool Equals(MovementQuery other)
        {
            return From.Equals(other.From)
                && To.Equals(other.To)
                && Radius == other.Radius;
        }
    }
}
