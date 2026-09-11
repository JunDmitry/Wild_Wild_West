using System;
using System.Collections.Generic;
using Game.Core.Model.Entities;
using Game.Core.Model.Enums;
using Game.Core.Model.Facts;
using Game.Core.Model.States;

namespace Game.Core.Rules.Outcomes
{
    /// <summary>
    /// Represents the outcome of an attack execution.
    /// </summary>
    public readonly struct AttackExecutionOutcome
    {
        /// <summary>
        /// Initializes a new instance of the <see cref="AttackExecutionOutcome"/> struct.
        /// </summary>
        /// <param name="player">The updated player state.</param>
        /// <param name="enemies">The updated collection of enemy states keyed by entity identifier.</param>
        /// <param name="attackExecuted">A value indicating whether an attack was executed.</param>
        /// <param name="weaponKind">The kind of weapon used for the attack.</param>
        /// <param name="anyHit">A value indicating whether the attack hit any enemy.</param>
        /// <param name="damagedFacts">The facts describing damage dealt to enemies.</param>
        public AttackExecutionOutcome(
            PlayerState player,
            IReadOnlyDictionary<EntityId, EnemyState> enemies,
            bool attackExecuted,
            WeaponKind weaponKind,
            bool anyHit,
            IReadOnlyList<EnemyDamagedFact> damagedFacts)
        {
            Player = player;
            Enemies = enemies;
            AttackExecuted = attackExecuted;
            WeaponKind = weaponKind;
            AnyHit = anyHit;
            DamagedFacts = damagedFacts ?? Array.Empty<EnemyDamagedFact>();
        }

        /// <summary>
        /// Gets the updated player state.
        /// </summary>
        public PlayerState Player { get; }

        /// <summary>
        /// Gets the updated collection of enemy states keyed by entity identifier.
        /// </summary>
        public IReadOnlyDictionary<EntityId, EnemyState> Enemies { get; }

        /// <summary>
        /// Gets a value indicating whether an attack was executed.
        /// </summary>
        public bool AttackExecuted { get; }

        /// <summary>
        /// Gets the kind of weapon used for the attack.
        /// </summary>
        public WeaponKind WeaponKind { get; }

        /// <summary>
        /// Gets a value indicating whether the attack hit any enemy.
        /// </summary>
        public bool AnyHit { get; }

        /// <summary>
        /// Gets the facts describing damage dealt to enemies.
        /// </summary>
        public IReadOnlyList<EnemyDamagedFact> DamagedFacts { get; }
    }
}
