using Game.Arena.Domain.Geometry;

namespace Game.Arena.Application.Ports
{
    /// <summary>
    /// A live Enemy's planar footprint: its ground position and collision
    /// radius. Spawn placement receives these so a new spawn stays clear of the
    /// enemies that already exist (ADR-0039).
    /// </summary>
    public readonly struct EnemyFootprint
    {
        public EnemyFootprint(Position3D position, CollisionRadius radius)
        {
            Position = position;
            Radius = radius;
        }

        public Position3D Position { get; }

        public CollisionRadius Radius { get; }
    }
}
