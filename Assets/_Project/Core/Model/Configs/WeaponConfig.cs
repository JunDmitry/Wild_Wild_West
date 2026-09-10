using System;
using Game.Core.Model.Enums;

namespace Game.Core.Model.Configs
{
    /// <summary>
    /// Represents the configuration of a weapon.
    /// </summary>
    public readonly struct WeaponConfig : IEquatable<WeaponConfig>
    {
        /// <summary>
        /// Initializes a new instance of the <see cref="WeaponConfig"/> struct.
        /// </summary>
        /// <param name="kind">The kind of the weapon.</param>
        /// <param name="cooldown">The cooldown time of the weapon.</param>
        public WeaponConfig(
            WeaponKind kind,
            float cooldown,
            float damage,
            float range)
        {
            Kind = kind;
            Cooldown = cooldown;
            Damage = damage;
            Range = range;
        }

        /// <summary>
        /// Gets the kind of the weapon.
        /// </summary>
        public WeaponKind Kind { get; }

        /// <summary>
        /// Gets the cooldown time of the weapon.
        /// </summary>
        public float Cooldown { get; }
        public float Damage { get; }
        public float Range { get; }

        /// <summary>
        /// Returns the hash code for the current weapon configuration.
        /// </summary>
        /// <returns>A hash code for the current weapon configuration.</returns>
        public override int GetHashCode()
        {
            return HashCode.Combine(
                Kind,
                Cooldown,
                Damage,
                Range);
        }

        /// <summary>
        /// Determines whether the current instance is equal to another object.
        /// </summary>
        /// <param name="obj">The object to compare with the current instance.</param>
        /// <returns><see langword="true"/> if <paramref name="obj"/> is a <see cref="WeaponConfig"/> and is equal to the current instance; otherwise, <see langword="false"/>.</returns>
        public override bool Equals(object obj)
        {
            return obj is WeaponConfig other && Equals(other);
        }

        /// <summary>
        /// Determines whether the current instance is equal to another <see cref="WeaponConfig"/>.
        /// </summary>
        /// <param name="other">The <see cref="WeaponConfig"/> to compare with the current instance.</param>
        /// <returns><see langword="true"/> if the current instance is equal to <paramref name="other"/>; otherwise, <see langword="false"/>.</returns>
        public bool Equals(WeaponConfig other)
        {
            return Kind == other.Kind
                && Cooldown == other.Cooldown
                && Damage == other.Damage
                && Range == other.Range;
        }
    }
}
