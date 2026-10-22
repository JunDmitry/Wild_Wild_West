using System;
using Game.Arena.Domain.Geometry;

namespace Game.Arena.Application.Input
{
    public sealed class PlayerFrameInput
    {
        public PlayerFrameInput(
            MovementAxes movementAxes,
            Direction3D viewDirection,
            bool attackPressed,
            bool switchWeaponPressed)
        {
            if (viewDirection.IsValid == false)
            {
                throw new ArgumentException("View direction is invalid.", nameof(viewDirection));
            }

            if (viewDirection.Y != 0f)
            {
                throw new ArgumentException("View direction must be horizontal.", nameof(viewDirection));
            }

            MovementAxes = movementAxes;
            ViewDirection = viewDirection;
            AttackPressed = attackPressed;
            SwitchWeaponPressed = switchWeaponPressed;
        }

        public MovementAxes MovementAxes { get; }

        public Direction3D ViewDirection { get; }

        public bool AttackPressed { get; }

        public bool SwitchWeaponPressed { get; }
    }
}
