using Game.Arena.Domain.Combat;
using Game.Arena.Domain.Geometry;
using Game.Arena.Domain.Identity;
using Game.Arena.Domain.Time;
using Game.Arena.Domain.Vitality;

namespace Game.Arena.Application.ReadModels
{
    public sealed class PlayerSnapshot
    {
        public PlayerSnapshot(
            PlayerId playerId,
            Position3D position,
            Health health,
            WeaponKind selectedWeapon,
            GameTimePoint rangedReadyAt,
            GameTimePoint meleeReadyAt,
            bool hasPendingAttack,
            PendingPlayerAttack pendingAttack)
        {
            PlayerId = playerId;
            Position = position;
            Health = health;
            SelectedWeapon = selectedWeapon;
            RangedReadyAt = rangedReadyAt;
            MeleeReadyAt = meleeReadyAt;
            HasPendingAttack = hasPendingAttack;
            PendingAttack = pendingAttack;
        }

        public PlayerId PlayerId { get; }

        public Position3D Position { get; }

        public Health Health { get; }

        public WeaponKind SelectedWeapon { get; }

        public GameTimePoint RangedReadyAt { get; }

        public GameTimePoint MeleeReadyAt { get; }

        public bool HasPendingAttack { get; }

        public PendingPlayerAttack PendingAttack { get; }
    }
}
