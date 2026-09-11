using Game.Core.Model.Enums;
using Game.Core.Model.States;

namespace Game.Core.Rules.Outcomes
{
    /// <summary>
    /// Represents the outcome of a weapon switch attempt.
    /// </summary>
    public readonly struct WeaponSwitchOutcome
    {
        /// <summary>
        /// Initializes a new instance of the <see cref="WeaponSwitchOutcome"/> struct.
        /// </summary>
        /// <param name="player">The updated player state.</param>
        /// <param name="switched">A value indicating whether the weapon was switched.</param>
        /// <param name="newWeapon">The kind of the new weapon if the switch occurred; otherwise, the previous weapon kind.</param>
        public WeaponSwitchOutcome(
            PlayerState player,
            bool switched,
            WeaponKind newWeapon)
        {
            Player = player;
            Switched = switched;
            NewWeapon = newWeapon;
        }

        /// <summary>
        /// Gets the updated player state.
        /// </summary>
        public PlayerState Player { get; }

        /// <summary>
        /// Gets a value indicating whether the weapon was switched.
        /// </summary>
        public bool Switched { get; }

        /// <summary>
        /// Gets the kind of the new weapon if the switch occurred; otherwise, the previous weapon kind.
        /// </summary>
        public WeaponKind NewWeapon { get; }
    }

    /// <summary>
    /// Represents the outcome of a movement action.
    /// </summary>
    public readonly struct MovementOutcome
    {
        /// <summary>
        /// Initializes a new instance of the <see cref="MovementOutcome"/> struct.
        /// </summary>
        /// <param name="player">The updated player state.</param>
        public MovementOutcome(PlayerState player)
        {
            Player = player;
        }

        /// <summary>
        /// Gets the updated player state.
        /// </summary>
        public PlayerState Player { get; }
    }
}
