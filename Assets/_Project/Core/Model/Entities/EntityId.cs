using System;

namespace Game.Core.Model.Entities
{
    /// <summary>
    /// Represents a unique identifier for an entity.
    /// </summary>
    public readonly struct EntityId : IEquatable<EntityId>
    {
        public static readonly EntityId Identity = new EntityId(-1);

        /// <summary>
        /// Initializes a new instance of the <see cref="EntityId"/> struct with the specified value.
        /// </summary>
        /// <param name="value">The numeric value of the entity identifier.</param>
        public EntityId(int value)
        {
            Value = value;
        }

        /// <summary>
        /// Gets the numeric value of the entity identifier.
        /// </summary>
        public int Value { get; }

        /// <summary>
        /// Returns the hash code for this <see cref="EntityId"/> instance.
        /// </summary>
        /// <returns>The hash code based on the <see cref="Value"/> property.</returns>
        public override int GetHashCode()
        {
            return Value.GetHashCode();
        }

        /// <summary>
        /// Determines whether the specified object is equal to this instance.
        /// </summary>
        /// <param name="obj">The object to compare with this instance.</param>
        /// <returns>True if the objects are equal; otherwise, false.</returns>
        public override bool Equals(object obj)
        {
            return obj is EntityId entityId && Equals(entityId);
        }

        /// <summary>
        /// Determines whether the current <see cref="EntityId"/> is equal to another <see cref="EntityId"/>.
        /// </summary>
        /// <param name="other">The <see cref="EntityId"/> to compare with this instance.</param>
        /// <returns>True if both identifiers have the same <see cref="Value"/>; otherwise, false.</returns>
        public bool Equals(EntityId other)
        {
            return Value == other.Value;
        }
    }
}
