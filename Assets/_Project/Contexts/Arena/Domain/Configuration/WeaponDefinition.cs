using System;
using Game.Arena.Domain.Combat;
using Game.Arena.Domain.Geometry;
using Game.Arena.Domain.Time;
using Game.Arena.Domain.Vitality;

namespace Game.Arena.Domain.Configuration
{
    public sealed class WeaponDefinition
    {
        public WeaponDefinition(
            WeaponKind kind,
            DamageAmount damage,
            Distance range,
            GameDuration cooldown,
            GameDuration windupDuration)
        {
            if (damage.Points <= 0)
            {
                throw new ArgumentException("DamageAmount is invalid.", nameof(damage));
            }

            if (range.IsZero)
            {
                throw new ArgumentOutOfRangeException(nameof(range));
            }

            if (cooldown.Seconds <= 0d)
            {
                throw new ArgumentOutOfRangeException(nameof(cooldown));
            }

            Kind = kind;
            Damage = damage;
            Range = range;
            Cooldown = cooldown;
            WindupDuration = windupDuration;
        }

        public WeaponKind Kind { get; }
        public DamageAmount Damage { get; }
        public Distance Range { get; }
        public GameDuration Cooldown { get; }
        public GameDuration WindupDuration { get; }
    }
}
