namespace Game.Core.Model.Entities
{
    /// <summary>
    /// Represents the input for a single frame.
    /// </summary>
    public readonly struct FrameInput
    {
        /// <summary>
        /// Initializes a new instance of the <see cref="FrameInput"/> struct.
        /// </summary>
        /// <param name="moveDirection">The direction in which to move.</param>
        /// <param name="aimOrigin">The origin point for aiming.</param>
        /// <param name="aimDirection">The direction in which to aim.</param>
        /// <param name="attackPressed">A value indicating whether the attack button was pressed.</param>
        /// <param name="switchWeaponPressed">A value indicating whether the switch weapon button was pressed.</param>
        public FrameInput(
            Direction3D moveDirection,
            Position3D aimOrigin,
            Direction3D aimDirection,
            bool attackPressed,
            bool switchWeaponPressed)
        {
            MoveDirection = moveDirection;
            AimOrigin = aimOrigin;
            AimDirection = aimDirection;
            AttackPressed = attackPressed;
            SwitchWeaponPressed = switchWeaponPressed;
        }

        /// <summary>
        /// Gets the direction in which to move.
        /// </summary>
        public Direction3D MoveDirection { get; }

        /// <summary>
        /// Gets the origin point for aiming.
        /// </summary>
        public Position3D AimOrigin { get; }

        /// <summary>
        /// Gets the direction in which to aim.
        /// </summary>
        public Direction3D AimDirection { get; }

        /// <summary>
        /// Gets a value indicating whether the attack button was pressed.
        /// </summary>
        public bool AttackPressed { get; }

        /// <summary>
        /// Gets a value indicating whether the switch weapon button was pressed.
        /// </summary>
        public bool SwitchWeaponPressed { get; }
    }
}
