using System;
using System.Collections.Generic;
using Game.Core.Model.Entities;

namespace Game.Core.Model.Simulation.Resolutions
{
    /// <summary>
    /// Represents the resolution of an attack query, indicating whether a ranged hit occurred and which entities were hit by melee.
    /// </summary>
    public readonly struct AttackResolution : IEquatable<AttackResolution>
    {
        /// <summary>
        /// Initializes a new instance of the <see cref="AttackResolution"/> struct.
        /// </summary>
        /// <param name="hasRangedHit">A value indicating whether a ranged hit occurred.</param>
        /// <param name="rangedHitId">The identifier of the entity hit by the ranged attack.</param>
        /// <param name="meleeHitIds">The identifiers of the entities hit by the melee attack.</param>
        public AttackResolution(
            bool hasRangedHit,
            EntityId rangedHitId,
            IReadOnlyList<EntityId> meleeHitIds)
        {
            HasRangedHit = hasRangedHit;
            RangedHitId = rangedHitId;
            MeleeHitIds = meleeHitIds ?? Array.Empty<EntityId>();
        }

        /// <summary>
        /// Gets an empty attack resolution.
        /// </summary>
        public static AttackResolution Empty { get; } = new(false, EntityId.None, Array.Empty<EntityId>());

        /// <summary>
        /// Gets an attack resolution representing a ranged miss.
        /// </summary>
        public static AttackResolution RangedMiss { get; } = new(hasRangedHit: false, rangedHitId: EntityId.None, meleeHitIds: Array.Empty<EntityId>());

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
        /// Creates an attack resolution for a successful ranged hit.
        /// </summary>
        /// <param name="targetId">The identifier of the hit target.</param>
        /// <returns>A new <see cref="AttackResolution"/> representing a ranged hit.</returns>
        public static AttackResolution RangedHit(EntityId targetId)
        {
            return new(hasRangedHit: true, rangedHitId: targetId, meleeHitIds: Array.Empty<EntityId>());
        }

        /// <summary>
        /// Creates an attack resolution for successful melee hits.
        /// </summary>
        /// <param name="hitIds">The identifiers of the hit targets.</param>
        /// <returns>A new <see cref="AttackResolution"/> representing melee hits.</returns>
        public static AttackResolution MeleeHits(IReadOnlyList<EntityId> hitIds)
        {
            return new(hasRangedHit: false, rangedHitId: EntityId.None, meleeHitIds: hitIds);
        }

        /// <summary>
        /// Returns the hash code for the current attack resolution.
        /// </summary>
        /// <returns>A hash code for the current attack resolution.</returns>
        public override int GetHashCode()
        {
            return HashCode.Combine(
                HasRangedHit,
                RangedHitId,
                MeleeHitIds.Count);
        }

        /// <summary>
        /// Determines whether the current instance is equal to another object.
        /// </summary>
        /// <param name="obj">The object to compare with the current instance.</param>
        /// <returns><see langword="true"/> if <paramref name="obj"/> is an <see cref="AttackResolution"/> and is equal to the current instance; otherwise, <see langword="false"/>.</returns>
        public override bool Equals(object obj)
        {
            return obj is AttackResolution other && Equals(other);
        }

        /// <summary>
        /// Determines whether the current instance is equal to another <see cref="AttackResolution"/>.
        /// </summary>
        /// <param name="other">The <see cref="AttackResolution"/> to compare with the current instance.</param>
        /// <returns><see langword="true"/> if the current instance is equal to <paramref name="other"/>; otherwise, <see langword="false"/>.</returns>
        public bool Equals(AttackResolution other)
        {
            return HasRangedHit == other.HasRangedHit
                && RangedHitId.Equals(other.RangedHitId)
                && HitIdsEqual(MeleeHitIds, other.MeleeHitIds);
        }

        private static bool HitIdsEqual(IReadOnlyList<EntityId> left, IReadOnlyList<EntityId> right)
        {
            if (ReferenceEquals(left, right))
            {
                return true;
            }

            if (left == null || right == null || left.Count != right.Count)
            {
                return false;
            }

            for (int i = 0; i < left.Count; i++)
            {
                if (left[i] != right[i])
                {
                    return false;
                }
            }

            return true;
        }
    }
}
