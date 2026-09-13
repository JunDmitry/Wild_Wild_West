using Game.Core.Model.Entities;

namespace Game.Application.Identity
{
    /// <summary>
    /// Defines a source for allocating entity identifiers.
    /// </summary>
    public interface IEntityIdSource
    {
        /// <summary>
        /// Allocates a new entity identifier.
        /// </summary>
        /// <returns>A newly allocated <see cref="EntityId"/>.</returns>
        EntityId Allocate();
    }
}