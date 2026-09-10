using System;

namespace Game.Core.Model.Configs
{
    /// <summary>
    /// Represents the configuration of the arena.
    /// </summary>
    public readonly struct ArenaConfig : IEquatable<ArenaConfig>
    {
        /// <summary>
        /// Initializes a new instance of the <see cref="ArenaConfig"/> struct.
        /// </summary>
        /// <param name="minX">The minimum X coordinate of the arena.</param>
        /// <param name="maxX">The maximum X coordinate of the arena.</param>
        /// <param name="minZ">The minimum Z coordinate of the arena.</param>
        /// <param name="maxZ">The maximum Z coordinate of the arena.</param>
        /// <param name="groundY">The Y coordinate of the ground.</param>
        /// <param name="playerRadius">The radius of the player.</param>
        public ArenaConfig(
            float minX,
            float maxX,
            float minZ,
            float maxZ,
            float groundY,
            float playerRadius)
        {
            MinX = minX;
            MaxX = maxX;
            MinZ = minZ;
            MaxZ = maxZ;
            GroundY = groundY;
            PlayerRadius = playerRadius;
        }

        /// <summary>
        /// Gets the minimum X coordinate of the arena.
        /// </summary>
        public float MinX { get; }

        /// <summary>
        /// Gets the maximum X coordinate of the arena.
        /// </summary>
        public float MaxX { get; }

        /// <summary>
        /// Gets the minimum Z coordinate of the arena.
        /// </summary>
        public float MinZ { get; }

        /// <summary>
        /// Gets the maximum Z coordinate of the arena.
        /// </summary>
        public float MaxZ { get; }

        /// <summary>
        /// Gets the Y coordinate of the ground.
        /// </summary>
        public float GroundY { get; }

        /// <summary>
        /// Gets the radius of the player.
        /// </summary>
        public float PlayerRadius { get; }

        /// <summary>
        /// Returns the hash code for the current arena configuration.
        /// </summary>
        /// <returns>A hash code for the current arena configuration.</returns>
        public override int GetHashCode()
        {
            return HashCode.Combine(
                MinX,
                MaxX,
                MinZ,
                MaxZ,
                GroundY,
                PlayerRadius);
        }

        /// <summary>
        /// Determines whether the current instance is equal to another object.
        /// </summary>
        /// <param name="obj">The object to compare with the current instance.</param>
        /// <returns><see langword="true"/> if <paramref name="obj"/> is an <see cref="ArenaConfig"/> and is equal to the current instance; otherwise, <see langword="false"/>.</returns>
        public override bool Equals(object obj)
        {
            return obj is ArenaConfig other && Equals(other);
        }

        /// <summary>
        /// Determines whether the current instance is equal to another <see cref="ArenaConfig"/>.
        /// </summary>
        /// <param name="other">The <see cref="ArenaConfig"/> to compare with the current instance.</param>
        /// <returns><see langword="true"/> if the current instance is equal to <paramref name="other"/>; otherwise, <see langword="false"/>.</returns>
        public bool Equals(ArenaConfig other)
        {
            return MinX == other.MinX
                && MaxX == other.MaxX
                && MinZ == other.MinZ
                && MaxZ == other.MaxZ
                && GroundY == other.GroundY
                && PlayerRadius == other.PlayerRadius;

        }
    }
}
