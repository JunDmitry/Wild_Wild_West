using System;
using Game.Arena.Domain.Identity;
using Game.Arena.Domain.Time;

namespace Game.Arena.Domain.Combat
{
    public readonly struct PendingEnemyAttack : IEquatable<PendingEnemyAttack>
    {
        public PendingEnemyAttack(
            AttackId id,
            EnemyId enemyId,
            GameTimePoint startedAt,
            GameTimePoint impactAt)
        {
            if (id.IsNone)
            {
                throw new ArgumentException("AttackId cannot be None.", nameof(id));
            }

            if (enemyId.IsNone)
            {
                throw new ArgumentException("EnemyId cannot be None.", nameof(enemyId));
            }

            if (impactAt < startedAt)
            {
                throw new ArgumentOutOfRangeException(nameof(impactAt));
            }

            Id = id;
            EnemyId = enemyId;
            StartedAt = startedAt;
            ImpactAt = impactAt;
        }

        public AttackId Id { get; }

        public EnemyId EnemyId { get; }

        public GameTimePoint StartedAt { get; }

        public GameTimePoint ImpactAt { get; }

        public bool IsReadyToImpact(GameTimePoint currentTime)
        {
            return currentTime >= ImpactAt;
        }

        public bool Equals(PendingEnemyAttack other)
        {
            return Id.Equals(other.Id)
                && EnemyId.Equals(other.EnemyId)
                && StartedAt.Equals(other.StartedAt)
                && ImpactAt.Equals(other.ImpactAt);
        }

        public override bool Equals(object obj)
        {
            return obj is PendingEnemyAttack other && Equals(other);
        }

        public override int GetHashCode()
        {
            return HashCode.Combine(
                Id,
                EnemyId,
                StartedAt,
                ImpactAt);
        }

        public static bool operator ==(
            PendingEnemyAttack left,
            PendingEnemyAttack right)
        {
            return left.Equals(right);
        }

        public static bool operator !=(
            PendingEnemyAttack left,
            PendingEnemyAttack right)
        {
            return !left.Equals(right);
        }
    }
}
