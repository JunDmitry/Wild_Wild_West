using System;
using Game.Arena.Domain.Identity;

namespace Game.Arena.Domain.Combat
{
    public readonly struct AttackActor : IEquatable<AttackActor>
    {
        private AttackActor(AttackActorKind kind, PlayerId playerId, EnemyId enemyId)
        {
            Kind = kind;
            PlayerId = playerId;
            EnemyId = enemyId;
        }

        public AttackActorKind Kind { get; }

        public PlayerId PlayerId { get; }

        public EnemyId EnemyId { get; }

        public static AttackActor Player(PlayerId playerId)
        {
            if (playerId.IsNone)
            {
                throw new ArgumentException("PlayerId cannot be None.", nameof(playerId));
            }

            return new AttackActor(AttackActorKind.Player, playerId, EnemyId.None);
        }

        public static AttackActor Enemy(EnemyId enemyId)
        {
            if (enemyId.IsNone)
            {
                throw new ArgumentException("EnemyId cannot be None.", nameof(enemyId));
            }

            return new AttackActor(AttackActorKind.Enemy, PlayerId.None, enemyId);
        }

        public bool Equals(AttackActor other)
        {
            return Kind == other.Kind
                && PlayerId.Equals(other.PlayerId)
                && EnemyId.Equals(other.EnemyId);
        }

        public override bool Equals(object obj)
        {
            return obj is AttackActor other && Equals(other);
        }

        public override int GetHashCode()
        {
            return HashCode.Combine(Kind, PlayerId, EnemyId);
        }
    }
}
