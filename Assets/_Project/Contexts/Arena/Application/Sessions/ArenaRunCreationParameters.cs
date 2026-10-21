using System;
using Game.Arena.Domain.Configuration;

namespace Game.Arena.Application.Sessions
{
    public sealed class ArenaRunCreationParameters
    {
        public ArenaRunCreationParameters(
            ArenaDefinition arenaDefinition,
            PlayerDefinition playerDefinition,
            WeaponCatalog weaponCatalog,
            EnemyCatalog enemyCatalog,
            WaveCatalog waveCatalog)
        {
            if (arenaDefinition == null)
            {
                throw new ArgumentNullException(nameof(arenaDefinition));
            }

            if (playerDefinition == null)
            {
                throw new ArgumentNullException(nameof(playerDefinition));
            }

            if (weaponCatalog == null)
            {
                throw new ArgumentNullException(nameof(weaponCatalog));
            }

            if (enemyCatalog == null)
            {
                throw new ArgumentNullException(nameof(enemyCatalog));
            }

            if (waveCatalog == null)
            {
                throw new ArgumentNullException(nameof(waveCatalog));
            }

            ArenaDefinition = arenaDefinition;
            PlayerDefinition = playerDefinition;
            WeaponCatalog = weaponCatalog;
            EnemyCatalog = enemyCatalog;
            WaveCatalog = waveCatalog;
        }

        public ArenaDefinition ArenaDefinition { get; }

        public PlayerDefinition PlayerDefinition { get; }

        public WeaponCatalog WeaponCatalog { get; }

        public EnemyCatalog EnemyCatalog { get; }

        public WaveCatalog WaveCatalog { get; }
    }
}
