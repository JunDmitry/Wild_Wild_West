using System;
using Game.Arena.Domain.Configuration;
using Game.Arena.Domain.Identity;
using Game.Arena.Domain.Movement;

namespace Game.Arena.Domain.Aggregates.ArenaRun
{
    public sealed class ArenaRunFactory
    {
        private readonly MovementPathPolicy _movementPathPolicy;

        public ArenaRunFactory(MovementPathPolicy movementPathPolicy = null)
        {
            _movementPathPolicy = movementPathPolicy ?? new MovementPathPolicy();
        }

        public ArenaRun Start(
            ArenaRunId arenaRunId,
            PlayerId playerId,
            ArenaDefinition arenaDefinition,
            PlayerDefinition playerDefinition,
            EnemyCatalog enemyCatalog,
            WeaponCatalog weaponCatalog,
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

            if (enemyCatalog == null)
            {
                throw new ArgumentNullException(nameof(enemyCatalog));
            }

            if (weaponCatalog == null)
            {
                throw new ArgumentNullException(nameof(weaponCatalog));
            }

            if (waveCatalog == null)
            {
                throw new ArgumentNullException(nameof(waveCatalog));
            }

            Player player = new(
                playerId,
                playerDefinition.StartPosition,
                playerDefinition.InitialHealth,
                Combat.WeaponKind.Ranged,
                playerDefinition.MovementSpeed,
                playerDefinition.CollisionRadius);

            Wave wave = new(waveCatalog.Get(WaveNumber.First));

            return new(
                arenaRunId,
                player,
                wave,
                arenaDefinition,
                weaponCatalog,
                enemyCatalog,
                waveCatalog,
                _movementPathPolicy);
        }
    }
}
