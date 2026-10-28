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
        internal PlayerStateSnapshot()
        {
        }

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

            Fill(playerId, position, health, selectedWeapon, rangedReadyAt, meleeReadyAt, pendingAttack);
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
            PendingPlayerAttack pendingAttack)
        {
            PlayerId = playerId;
            Position = position;
            Health = health;
            SelectedWeapon = selectedWeapon;
            RangedReadyAt = rangedReadyAt;
            MeleeReadyAt = meleeReadyAt;
            PendingAttack = pendingAttack;
            HasPendingAttack = pendingAttack.Id.IsNone == false;
        }
    }
}
