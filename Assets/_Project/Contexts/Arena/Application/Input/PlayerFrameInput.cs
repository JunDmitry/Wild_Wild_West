using System;
using Game.Arena.Domain.Geometry;

namespace Game.Arena.Application.Input
{
    public sealed class PlayerFrameInput
    {
        public PlayerFrameInput()
        {
            ViewDirection = Direction3D.Forward;
        }

        public PlayerFrameInput(
            MovementAxes movementAxes,
            Direction3D viewDirection,
            bool attackPressed,
            bool switchWeaponPressed)
            : this()
        {
            Fill(movementAxes, viewDirection, attackPressed, switchWeaponPressed);
        }

        public MovementAxes MovementAxes { get; private set; }

        public Direction3D ViewDirection { get; private set; }

        public bool AttackPressed { get; private set; }

        public bool SwitchWeaponPressed { get; private set; }

        /// <summary>
        /// Refills this instance for the next frame (ADR-0035 point 9). The
        /// value is valid for one tick; no consumer may retain it.
        /// </summary>
        public void Fill(
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
    }
}
