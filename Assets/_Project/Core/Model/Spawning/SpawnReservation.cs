using System;
using Game.Core.Model.Entities;

namespace Game.Core.Model.Spawning
{
    /// <summary>
    /// Represents an externally prepared spawn reservation.
    /// </summary>
    public readonly struct SpawnReservation : IEquatable<SpawnReservation>
    {
        public SpawnReservation(
            EntityId entityId,
            Position3D position)
        {
            EntityId = entityId;
            Position = position;
        }

        public EntityId EntityId { get; }
        public Position3D Position { get; }

        public override int GetHashCode()
        {
            return HashCode.Combine(
                EntityId,
                Position);
        }

        public override bool Equals(object obj)
        {
            return obj is SpawnReservation other && Equals(other);
        }

        public bool Equals(SpawnReservation other)
        {
            return EntityId == other.EntityId && Position == other.Position;
        }
    }
}
