using System;
using System.Collections.Generic;
using Game.Core.Model.Entities;
using Game.Core.Model.Enums;

namespace Game.Core.Model.States
{
    /// <summary>
    /// Represents an immutable snapshot of the game state.
    /// </summary>
    public readonly struct GameState : IEquatable<GameState>
    {
        /// <summary>
        /// Initializes a new instance of the <see cref="GameState"/> struct.
        /// </summary>
        /// <param name="phase">The current phase of the game.</param>
        /// <param name="player">The current state of the player.</param>
        /// <param name="enemies">The collection of enemy states keyed by entity identifier.</param>
        /// <param name="currentWave">The current wave state.</param>
        /// <param name="time">The elapsed game time.</param>
        public GameState(
            GamePhase phase,
            PlayerState player,
            IReadOnlyDictionary<EntityId, EnemyState> enemies,
            WaveState currentWave,
            float time,
            float phaseEnteredTime)
        {
            Phase = phase;
            Player = player;
            Enemies = new Dictionary<EntityId, EnemyState>(enemies);
            CurrentWave = currentWave;
            Time = time;
            PhaseEnteredTime = phaseEnteredTime;
        }

        /// <summary>
        /// Gets the current phase of the game.
        /// </summary>
        public GamePhase Phase { get; }

        /// <summary>
        /// Gets the current state of the player.
        /// </summary>
        public PlayerState Player { get; }

        /// <summary>
        /// Gets the collection of enemy states keyed by entity identifier.
        /// </summary>
        public IReadOnlyDictionary<EntityId, EnemyState> Enemies { get; }

        /// <summary>
        /// Gets the current wave state.
        /// </summary>
        public WaveState CurrentWave { get; }

        /// <summary>
        /// Gets the elapsed game time.
        /// </summary>
        public float Time { get; }
        public float PhaseEnteredTime { get; }

        private static bool NormalizedEnemiesEquals(IReadOnlyDictionary<EntityId, EnemyState> left, IReadOnlyDictionary<EntityId, EnemyState> right)
        {
            if (left.Count != right.Count)
            {
                return false;
            }

            foreach (KeyValuePair<EntityId, EnemyState> item in left)
            {
                if (right.TryGetValue(item.Key, out EnemyState otherState) == false)
                {
                    return false;
                }

                if (item.Value.Equals(otherState) == false)
                {
                    return false;
                }
            }

            return true;
        }

        /// <summary>
        /// Returns the hash code for the current game state.
        /// </summary>
        /// <returns>A hash code for the current game state.</returns>
        public override int GetHashCode()
        {
            return HashCode.Combine(
                Phase,
                Player,
                Enemies.Count,
                CurrentWave,
                Time,
                PhaseEnteredTime);
        }

        /// <summary>
        /// Determines whether the current instance is equal to another object.
        /// </summary>
        /// <param name="obj">The object to compare with the current instance.</param>
        /// <returns><see langword="true"/> if <paramref name="obj"/> is a <see cref="GameState"/> and is equal to the current instance; otherwise, <see langword="false"/>.</returns>
        public override bool Equals(object obj)
        {
            return obj is GameState other && Equals(other);
        }

        /// <summary>
        /// Determines whether the current instance is equal to another <see cref="GameState"/>.
        /// </summary>
        /// <param name="other">The <see cref="GameState"/> to compare with the current instance.</param>
        /// <returns><see langword="true"/> if the current instance is equal to <paramref name="other"/>; otherwise, <see langword="false"/>.</returns>
        public bool Equals(GameState other)
        {
            return Phase == other.Phase
                && Player.Equals(other.Player)
                && NormalizedEnemiesEquals(Enemies, other.Enemies)
                && CurrentWave.Equals(other.CurrentWave)
                && Time == other.Time
                && PhaseEnteredTime == other.PhaseEnteredTime;
        }
    }
}
