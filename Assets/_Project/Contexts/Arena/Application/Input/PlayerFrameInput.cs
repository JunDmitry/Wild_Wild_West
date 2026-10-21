using System;
using Game.Arena.Domain.Geometry;

namespace Game.Arena.Application.Input
{
    public sealed class PlayerFrameInput
    {
        public PlayerFrameInput(
            MovementInput movement,
            Direction3D aimDirection,
            bool attackPressed,
            bool switchWeaponPressed)
        {
            if (aimDirection.IsValid == false)
            {
                throw new ArgumentException("Aim direction is invalid.", nameof(aimDirection));
            }

            Movement = movement;
            AimDirection = aimDirection;
            AttackPressed = attackPressed;
            SwitchWeaponPressed = switchWeaponPressed;
        }

        public MovementInput Movement { get; }
        public Direction3D AimDirection { get; }
        public bool AttackPressed { get; }
        public bool SwitchWeaponPressed { get; }
    }
}
