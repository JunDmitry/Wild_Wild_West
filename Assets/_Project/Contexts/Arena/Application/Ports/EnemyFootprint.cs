using Game.Arena.Domain.Geometry;
using Game.Arena.Domain.Identity;

namespace Game.Arena.Application.Ports
{
    /// <summary>
    /// A live Enemy's planar footprint: its identity, ground position, and
    /// collision radius. Spawn placement receives these so a new spawn stays
    /// clear of the enemies that already exist (ADR-0039); movement resolution
    /// receives them so every live Enemy is blocking geometry, whether or not it
    /// is presented or in the movement batch (ADR-0040).
    /// </summary>
    public readonly struct EnemyFootprint
    {
        public EnemyFootprint(EnemyId id, Position3D position, CollisionRadius radius)
        {
            Id = id;
            Position = position;
            Radius = radius;
        }

        public EnemyId Id { get; }

        public Position3D Position { get; }

        public CollisionRadius Radius { get; }
    }
}
