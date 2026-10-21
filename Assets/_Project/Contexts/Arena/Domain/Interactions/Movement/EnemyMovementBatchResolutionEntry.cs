using System;
using Game.Arena.Domain.Geometry;
using Game.Arena.Domain.Identity;

namespace Game.Arena.Domain.Interactions.Movement
{
    public readonly struct EnemyMovementBatchResolutionEntry : IEquatable<EnemyMovementBatchResolutionEntry>
    {
        public EnemyMovementBatchResolutionEntry(
            EnemyId enemyId,
            Position3D acceptedPosition)
        {
            EnemyId = enemyId;
            AcceptedPosition = acceptedPosition;
        }

        public EnemyId EnemyId { get; }

        public Position3D AcceptedPosition { get; }

        public bool Equals(EnemyMovementBatchResolutionEntry other)
        {
            return EnemyId.Equals(other.EnemyId)
                && AcceptedPosition.Equals(other.AcceptedPosition);
        }

        public override bool Equals(object obj)
        {
            return obj is EnemyMovementBatchResolutionEntry other && Equals(other);
        }

        public override int GetHashCode()
        {
            return HashCode.Combine(EnemyId, AcceptedPosition);
        }

        public static bool operator ==(EnemyMovementBatchResolutionEntry left, EnemyMovementBatchResolutionEntry right)
        {
            return left.Equals(right);
        }

        public static bool operator !=(EnemyMovementBatchResolutionEntry left, EnemyMovementBatchResolutionEntry right)
        {
            return !left.Equals(right);
        }
    }
}
