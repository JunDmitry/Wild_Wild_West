using System;
using System.Collections.Generic;

namespace Game.Core.Model.Entities
{
    /// <summary>
    /// Represents the context of a single frame.
    /// </summary>
    public readonly struct FrameContext
    {
        /// <summary>
        /// Initializes a new instance of the <see cref="FrameContext"/> struct.
        /// </summary>
        /// <param name="movementBlocked">A value indicating whether movement is blocked.</param>
        /// <param name="hasRangedHit">A value indicating whether a ranged hit occurred.</param>
        /// <param name="rangedHitId">The identifier of the entity hit by the ranged attack.</param>
        /// <param name="meleeHitIds">The identifiers of the entities hit by the melee attack.</param>
        /// <param name="hasSpawnPosition">A value indicating whether a spawn position is available.</param>
        /// <param name="nextSpawnPosition">The next spawn position.</param>
        public FrameContext(
            bool movementBlocked,
            bool hasRangedHit,
            EntityId rangedHitId,
            IReadOnlyList<EntityId> meleeHitIds,
            bool hasSpawnPosition,
            Position3D nextSpawnPosition)
        {
            MovementBlocked = movementBlocked;
            HasRangedHit = hasRangedHit;
            RangedHitId = rangedHitId;
            MeleeHitIds = meleeHitIds;
            HasSpawnPosition = hasSpawnPosition;
            NextSpawnPosition = nextSpawnPosition;
        }

        /// <summary>
        /// Gets an empty <see cref="FrameContext"/> instance.
        /// </summary>
        public static FrameContext Empty { get; } = new(false, false, default, Array.Empty<EntityId>(), false, Position3D.Zero);

        /// <summary>
        /// Gets a value indicating whether movement is blocked.
        /// </summary>
        public bool MovementBlocked { get; }

        /// <summary>
        /// Gets a value indicating whether a ranged hit occurred.
        /// </summary>
        public bool HasRangedHit { get; }

        /// <summary>
        /// Gets the identifier of the entity hit by the ranged attack.
        /// </summary>
        public EntityId RangedHitId { get; }

        /// <summary>
        /// Gets the identifiers of the entities hit by the melee attack.
        /// </summary>
        public IReadOnlyList<EntityId> MeleeHitIds { get; }

        /// <summary>
        /// Gets a value indicating whether a spawn position is available.
        /// </summary>
        public bool HasSpawnPosition { get; }

        /// <summary>
        /// Gets the next spawn position.
        /// </summary>
        public Position3D NextSpawnPosition { get; }
    }
}
