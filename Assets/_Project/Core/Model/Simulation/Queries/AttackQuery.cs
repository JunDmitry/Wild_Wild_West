using System;
using Game.Core.Model.Entities;

namespace Game.Core.Model.Simulation.Queries
{
    /// <summary>
    /// Represents a query for an attack, such as a ranged raycast or a melee overlap.
    /// </summary>
    public readonly struct AttackQuery : IEquatable<AttackQuery>
    {
        /// <summary>
        /// Initializes a new instance of the <see cref="AttackQuery"/> struct.
        /// </summary>
        /// <param name="kind">The kind of attack query.</param>
        /// <param name="origin">The origin of the attack.</param>
        /// <param name="direction">The direction of the attack.</param>
        /// <param name="range">The range or radius of the attack.</param>
        public AttackQuery(
            AttackQueryKind kind,
            Position3D origin,
            Direction3D direction,
            float range)
        {
            Kind = kind;
            Origin = origin;
            Direction = direction;
            Range = range;
        }

        /// <summary>
        /// Gets an empty attack query.
        /// </summary>
        public static AttackQuery None => new(AttackQueryKind.None, Position3D.Zero, Direction3D.Zero, 0f);

        /// <summary>
        /// Gets the kind of the attack query.
        /// </summary>
        public AttackQueryKind Kind { get; }

        /// <summary>
        /// Gets the origin of the attack.
        /// </summary>
        public Position3D Origin { get; }

        /// <summary>
        /// Gets the direction of the attack.
        /// </summary>
        public Direction3D Direction { get; }

        /// <summary>
        /// Gets the range or radius of the attack.
        /// </summary>
        public float Range { get; }

        /// <summary>
        /// Creates a ranged raycast attack query.
        /// </summary>
        /// <param name="origin">The origin of the raycast.</param>
        /// <param name="direction">The direction of the raycast.</param>
        /// <param name="maxDistance">The maximum distance of the raycast.</param>
        /// <returns>A new <see cref="AttackQuery"/> representing a ranged raycast.</returns>
        public static AttackQuery Ranged(Position3D origin, Direction3D direction, float maxDistance)
        {
            return new(AttackQueryKind.RangedRaycast, origin, direction, maxDistance);
        }

        /// <summary>
        /// Creates a melee overlap attack query.
        /// </summary>
        /// <param name="center">The center of the melee overlap.</param>
        /// <param name="radius">The radius of the melee overlap.</param>
        /// <returns>A new <see cref="AttackQuery"/> representing a melee overlap.</returns>
        public static AttackQuery Melee(Position3D center, float radius)
        {
            return new(AttackQueryKind.MeleeOverlap, center, Direction3D.Zero, radius);
        }

        /// <summary>
        /// Returns the hash code for the current attack query.
        /// </summary>
        /// <returns>A hash code for the current attack query.</returns>
        public override int GetHashCode()
        {
            return HashCode.Combine(
                Kind,
                Origin,
                Direction,
                Range);
        }

        /// <summary>
        /// Determines whether the current instance is equal to another object.
        /// </summary>
        /// <param name="obj">The object to compare with the current instance.</param>
        /// <returns><see langword="true"/> if <paramref name="obj"/> is an <see cref="AttackQuery"/> and is equal to the current instance; otherwise, <see langword="false"/>.</returns>
        public override bool Equals(object obj)
        {
            return obj is AttackQuery other && Equals(other);
        }

        /// <summary>
        /// Determines whether the current instance is equal to another <see cref="AttackQuery"/>.
        /// </summary>
        /// <param name="other">The <see cref="AttackQuery"/> to compare with the current instance.</param>
        /// <returns><see langword="true"/> if the current instance is equal to <paramref name="other"/>; otherwise, <see langword="false"/>.</returns>
        public bool Equals(AttackQuery other)
        {
            return Kind == other.Kind
                && Origin.Equals(other.Origin)
                && Direction.Equals(other.Direction)
                && Range == other.Range;
        }
    }
}
