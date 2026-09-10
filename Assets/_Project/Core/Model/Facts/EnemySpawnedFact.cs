using Game.Core.Model.Entities;
using Game.Core.Model.Enums;

namespace Game.Core.Model.Facts
{
    /// <summary>
    /// Represents a fact describing that an enemy has spawned.
    /// </summary>
    public readonly struct EnemySpawnedFact
    {
        /// <summary>
        /// Initializes a new instance of the <see cref="EnemySpawnedFact"/> struct.
        /// </summary>
        /// <param name="id">The identifier of the spawned enemy.</param>
        /// <param name="kind">The kind of the spawned enemy.</param>
        /// <param name="position">The spawn position of the enemy.</param>
        public EnemySpawnedFact(
            EntityId id,
            EnemyKind kind,
            Position3D position)
        {
            Id = id;
            Kind = kind;
            Position = position;
        }

        /// <summary>
        /// Gets the identifier of the spawned enemy.
        /// </summary>
        public EntityId Id { get; }

        /// <summary>
        /// Gets the kind of the spawned enemy.
        /// </summary>
        public EnemyKind Kind { get; }

        /// <summary>
        /// Gets the spawn position of the enemy.
        /// </summary>
        public Position3D Position { get; }
    }
}
