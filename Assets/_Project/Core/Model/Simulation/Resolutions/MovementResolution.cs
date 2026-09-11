using System;

namespace Game.Core.Model.Simulation.Resolutions
{
    /// <summary>
    /// Represents the resolution of a movement query, indicating whether the movement was blocked.
    /// </summary>
    public readonly struct MovementResolution : IEquatable<MovementResolution>
    {
        /// <summary>
        /// Initializes a new instance of the <see cref="MovementResolution"/> struct.
        /// </summary>
        /// <param name="isBlocked">A value indicating whether the movement is blocked.</param>
        public MovementResolution(
            bool isBlocked)
        {
            IsBlocked = isBlocked;
        }

        /// <summary>
        /// Gets a resolution representing unblocked movement.
        /// </summary>
        public static MovementResolution Unblocked { get; } = new(false);

        /// <summary>
        /// Gets a resolution representing blocked movement.
        /// </summary>
        public static MovementResolution Blocked { get; } = new(true);

        /// <summary>
        /// Gets a value indicating whether the movement is blocked.
        /// </summary>
        public bool IsBlocked { get; }

        /// <summary>
        /// Returns the hash code for the current movement resolution.
        /// </summary>
        /// <returns>A hash code for the current movement resolution.</returns>
        public override int GetHashCode()
        {
            return IsBlocked.GetHashCode();
        }

        /// <summary>
        /// Determines whether the current instance is equal to another object.
        /// </summary>
        /// <param name="obj">The object to compare with the current instance.</param>
        /// <returns><see langword="true"/> if <paramref name="obj"/> is a <see cref="MovementResolution"/> and is equal to the current instance; otherwise, <see langword="false"/>.</returns>
        public override bool Equals(object obj)
        {
            return obj is MovementResolution other && Equals(other);
        }

        /// <summary>
        /// Determines whether the current instance is equal to another <see cref="MovementResolution"/>.
        /// </summary>
        /// <param name="other">The <see cref="MovementResolution"/> to compare with the current instance.</param>
        /// <returns><see langword="true"/> if the current instance is equal to <paramref name="other"/>; otherwise, <see langword="false"/>.</returns>
        public bool Equals(MovementResolution other)
        {
            return other.IsBlocked == IsBlocked;
        }
    }
}
