using System;
using Game.Core.Model.Entities;

namespace Game.Core.Model.Simulation.Resolutions
{
    /// <summary>
    /// Represents the resolution of a spawn query, indicating whether an entity was spawned and its details.
    /// </summary>
    public readonly struct SpawnResolution : IEquatable<SpawnResolution>
    {
        /// <summary>
        /// Initializes a new instance of the <see cref="SpawnResolution"/> struct.
        /// </summary>
        /// <param name="hasSpawn">A value indicating whether a spawn occurred.</param>
        /// <param name="entityId">The identifier of the spawned entity.</param>
        /// <param name="position">The spawn position of the entity.</param>
        public SpawnResolution(
            bool hasSpawn,
            EntityId entityId,
            Position3D position)
        {
            HasSpawn = hasSpawn;
            EntityId = entityId;
            Position = position;
        }

        /// <summary>
        /// Gets a spawn resolution representing no spawn.
        /// </summary>
        public static SpawnResolution None { get; } = new(false, EntityId.None, Position3D.Zero);

        /// <summary>
        /// Gets a value indicating whether a spawn occurred.
        /// </summary>
        public bool HasSpawn { get; }

        /// <summary>
        /// Gets the identifier of the spawned entity.
        /// </summary>
        public EntityId EntityId { get; }

        /// <summary>
        /// Gets the spawn position of the entity.
        /// </summary>
        public Position3D Position { get; }

        /// <summary>
        /// Creates a spawn resolution for a successful spawn.
        /// </summary>
        /// <param name="id">The identifier of the spawned entity.</param>
        /// <param name="position">The spawn position of the entity.</param>
        /// <returns>A new <see cref="SpawnResolution"/> representing a successful spawn.</returns>
        public static SpawnResolution Successful(EntityId id, Position3D position)
        {
            return new(hasSpawn: true, entityId: id, position: position);
        }

        /// <summary>
        /// Returns the hash code for the current spawn resolution.
        /// </summary>
        /// <returns>A hash code for the current spawn resolution.</returns>
        public override int GetHashCode()
        {
            return HashCode.Combine(
                HasSpawn,
                EntityId,
                Position);
        }

        /// <summary>
        /// Determines whether the current instance is equal to another object.
        /// </summary>
        /// <param name="obj">The object to compare with the current instance.</param>
        /// <returns><see langword="true"/> if <paramref name="obj"/> is a <see cref="SpawnResolution"/> and is equal to the current instance; otherwise, <see langword="false"/>.</returns>
        public override bool Equals(object obj)
        {
            return obj is SpawnResolution other && Equals(other);
        }

        /// <summary>
        /// Determines whether the current instance is equal to another <see cref="SpawnResolution"/>.
        /// </summary>
        /// <param name="other">The <see cref="SpawnResolution"/> to compare with the current instance.</param>
        /// <returns><see langword="true"/> if the current instance is equal to <paramref name="other"/>; otherwise, <see langword="false"/>.</returns>
        public bool Equals(SpawnResolution other)
        {
            return HasSpawn == other.HasSpawn
                && EntityId == other.EntityId
                && Position == other.Position;
        }
    }
}
