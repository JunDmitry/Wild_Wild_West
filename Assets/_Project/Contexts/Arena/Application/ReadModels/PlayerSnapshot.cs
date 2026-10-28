using Game.Arena.Domain.Combat;
using Game.Arena.Domain.Geometry;
using Game.Arena.Domain.Identity;
using Game.Arena.Domain.Time;
using Game.Arena.Domain.Vitality;

namespace Game.Arena.Application.ReadModels
{
    public sealed class PlayerSnapshot
    {
        internal PlayerSnapshot()
        {
        }

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
            Fill(
                playerId,
                position,
                health,
                selectedWeapon,
                rangedReadyAt,
                meleeReadyAt,
                hasPendingAttack,
                pendingAttack);
        }

        public PlayerId PlayerId { get; private set; }

        public Position3D Position { get; private set; }

        public Health Health { get; private set; }

        public WeaponKind SelectedWeapon { get; private set; }

        public GameTimePoint RangedReadyAt { get; private set; }

        public GameTimePoint MeleeReadyAt { get; private set; }

        public bool HasPendingAttack { get; private set; }

        public PendingPlayerAttack PendingAttack { get; private set; }

        internal void Fill(
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
    }
}
