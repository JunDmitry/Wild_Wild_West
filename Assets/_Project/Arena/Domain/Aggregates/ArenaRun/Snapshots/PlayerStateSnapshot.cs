using System;
using Game.Arena.Domain.Combat;
using Game.Arena.Domain.Geometry;
using Game.Arena.Domain.Identity;
using Game.Arena.Domain.Time;
using Game.Arena.Domain.Vitality;

namespace Game.Arena.Domain.Aggregates.ArenaRun
{
    public sealed class PlayerStateSnapshot
    {
        public PlayerStateSnapshot(
            PlayerId playerId,
            Position3D position,
            Health health,
            WeaponKind selectedWeapon,
            GameTimePoint rangedReadyAt,
            GameTimePoint meleeReadyAt,
            PendingPlayerAttack pendingAttack)
        {
            if (playerId.IsNone)
            {
                throw new ArgumentException("PlayerId cannot be None.", nameof(playerId));
            }

            PlayerId = playerId;
            Position = position;
            Health = health;
            SelectedWeapon = selectedWeapon;
            RangedReadyAt = rangedReadyAt;
            MeleeReadyAt = meleeReadyAt;
            PendingAttack = pendingAttack;
            HasPendingAttack = pendingAttack.Id.IsNone == false;
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
