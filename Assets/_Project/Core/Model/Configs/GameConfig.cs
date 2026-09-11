using System;
using System.Collections.Generic;
using System.Linq;

namespace Game.Core.Model.Configs
{
    /// <summary>
    /// Represents the configuration of the game.
    /// </summary>
    public readonly struct GameConfig : IEquatable<GameConfig>
    {
        /// <summary>
        /// Initializes a new instance of the <see cref="GameConfig"/> struct.
        /// </summary>
        /// <param name="arena">The arena configuration.</param>
        /// <param name="player">The player configuration.</param>
        /// <param name="rangedWeapon">The configuration of the ranged weapon.</param>
        /// <param name="meleeWeapon">The configuration of the melee weapon.</param>
        /// <param name="regularEnemy">The configuration of the regular enemy.</param>
        /// <param name="bossEnemy">The configuration of the boss enemy.</param>
        /// <param name="waves">The wave configurations.</param>
        /// <param name="restartDelay">The delay before the game restarts.</param>
        public GameConfig(
            ArenaConfig arena,
            PlayerConfig player,
            WeaponConfig rangedWeapon,
            WeaponConfig meleeWeapon,
            EnemyConfig regularEnemy,
            EnemyConfig bossEnemy,
            IReadOnlyList<WaveConfig> waves,
            float restartDelay)
        {
            Arena = arena;
            Player = player;
            RangedWeapon = rangedWeapon;
            MeleeWeapon = meleeWeapon;
            RegularEnemy = regularEnemy;
            BossEnemy = bossEnemy;
            Waves = waves.ToArray();
            RestartDelay = restartDelay;
        }

        /// <summary>
        /// Gets the arena configuration.
        /// </summary>
        public ArenaConfig Arena { get; }

        /// <summary>
        /// Gets the player configuration.
        /// </summary>
        public PlayerConfig Player { get; }

        /// <summary>
        /// Gets the configuration of the ranged weapon.
        /// </summary>
        public WeaponConfig RangedWeapon { get; }

        /// <summary>
        /// Gets the configuration of the melee weapon.
        /// </summary>
        public WeaponConfig MeleeWeapon { get; }

        /// <summary>
        /// Gets the configuration of the regular enemy.
        /// </summary>
        public EnemyConfig RegularEnemy { get; }

        /// <summary>
        /// Gets the configuration of the boss enemy.
        /// </summary>
        public EnemyConfig BossEnemy { get; }

        /// <summary>
        /// Gets the wave configurations.
        /// </summary>
        public IReadOnlyList<WaveConfig> Waves { get; }

        /// <summary>
        /// Gets the delay before the game restarts.
        /// </summary>
        public float RestartDelay { get; }

        /// <summary>
        /// Returns the hash code for the current game configuration.
        /// </summary>
        /// <returns>A hash code for the current game configuration.</returns>
        public override int GetHashCode()
        {
            return HashCode.Combine(
                Arena,
                Player,
                RangedWeapon,
                MeleeWeapon,
                RegularEnemy,
                BossEnemy,
                Waves.Count,
                RestartDelay);
        }

        /// <summary>
        /// Determines whether the current instance is equal to another object.
        /// </summary>
        /// <param name="obj">The object to compare with the current instance.</param>
        /// <returns><see langword="true"/> if <paramref name="obj"/> is a <see cref="GameConfig"/> and is equal to the current instance; otherwise, <see langword="false"/>.</returns>
        public override bool Equals(object obj)
        {
            return obj is GameConfig other && Equals(other);
        }

        /// <summary>
        /// Determines whether the current instance is equal to another <see cref="GameConfig"/>.
        /// </summary>
        /// <param name="other">The <see cref="GameConfig"/> to compare with the current instance.</param>
        /// <returns><see langword="true"/> if the current instance is equal to <paramref name="other"/>; otherwise, <see langword="false"/>.</returns>
        public bool Equals(GameConfig other)
        {
            return Arena.Equals(other.Arena)
                && Player.Equals(other.Player)
                && RangedWeapon.Equals(other.RangedWeapon)
                && MeleeWeapon.Equals(other.MeleeWeapon)
                && RegularEnemy.Equals(other.RegularEnemy)
                && BossEnemy.Equals(other.BossEnemy)
                && WaveArrayEquals(Waves, other.Waves)
                && RestartDelay == other.RestartDelay;
        }

        private static bool WaveArrayEquals(IReadOnlyList<WaveConfig> left, IReadOnlyList<WaveConfig> right)
        {
            if (left.Count != right.Count)
            {
                return false;
            }

            for (int i = 0; i < left.Count; i++)
            {
                if (left[i].Equals(right[i]) == false)
                {
                    return false;
                }
            }

            return true;
        }
    }
}
