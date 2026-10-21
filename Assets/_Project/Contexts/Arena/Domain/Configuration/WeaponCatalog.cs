using System;
using Game.Arena.Domain.Combat;

namespace Game.Arena.Domain.Configuration
{
    public sealed class WeaponCatalog
    {
        private readonly WeaponDefinition _ranged;
        private readonly WeaponDefinition _melee;

        public WeaponCatalog(WeaponDefinition ranged, WeaponDefinition melee)
        {
            if (ranged == null)
            {
                throw new ArgumentNullException(nameof(ranged));
            }

            if (melee == null)
            {
                throw new ArgumentNullException(nameof(melee));
            }

            if (ranged.Kind != WeaponKind.Ranged)
            {
                throw new ArgumentException("Definition kind must be Ranged.", nameof(ranged));
            }

            if (melee.Kind != WeaponKind.Melee)
            {
                throw new ArgumentException("Definition kind must be Melee.", nameof(melee));
            }

            _ranged = ranged;
            _melee = melee;
        }

        public WeaponDefinition Get(WeaponKind kind)
        {
            return kind switch
            {
                WeaponKind.Ranged => _ranged,
                WeaponKind.Melee => _melee,
                _ => throw new ArgumentOutOfRangeException(nameof(kind)),
            };
        }
    }
}
