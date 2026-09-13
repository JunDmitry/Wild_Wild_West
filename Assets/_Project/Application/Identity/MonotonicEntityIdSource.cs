using Game.Core.Model.Entities;
using System;

namespace Game.Application.Identity
{
    /// <summary>
    /// Provides a thread-safe, monotonic source of entity identifiers.
    /// </summary>
    public sealed class MonotonicEntityIdSource : IEntityIdSource
    {
        private readonly object _gate = new();
        private long _nextValue;

        /// <summary>
        /// Initializes a new instance of the <see cref="MonotonicEntityIdSource"/> class starting from the value 1.
        /// </summary>
        public MonotonicEntityIdSource()
            : this(1)
        {
        }

        internal MonotonicEntityIdSource(int firstValue)
        {
            if (firstValue <= 0)
            {
                throw new ArgumentOutOfRangeException(nameof(firstValue), "Entity identifiers must be positive.");
            }

            _nextValue = firstValue;
        }

        /// <summary>
        /// Allocates a new entity identifier.
        /// </summary>
        /// <returns>A newly allocated <see cref="EntityId"/>.</returns>
        /// <exception cref="InvalidOperationException">Thrown when the entity identifier range has been exhausted.</exception>
        public EntityId Allocate()
        {
            lock (_gate)
            {
                if (_nextValue > int.MaxValue)
                {
                    throw new InvalidOperationException("The EntityId range has been exhausted.");
                }

                int allocatedValue = (int)_nextValue;
                _nextValue++;

                return new EntityId(allocatedValue);
            }
        }
    }
}