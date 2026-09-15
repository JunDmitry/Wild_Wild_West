using System;
using Game.Arena.Domain.Configuration;
using Game.Arena.Domain.Geometry;
using Game.Arena.Domain.Identity;
using Game.Arena.Domain.Interactions.Movement;
using Game.Arena.Domain.Vitality;

namespace Game.Arena.Domain.Aggregates.ArenaRun
{
    public sealed class ArenaRunFactory
    {
        public ArenaRun Start(
            ArenaRunId arenaRunId,
            PlayerId playerId,
            ArenaDefinition arenaDefinition,
            PlayerDefinition playerDefinition,
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

            if (enemyCatalog == null)
            {
                throw new ArgumentNullException(nameof(enemyCatalog));
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
                enemyCatalog,
                waveCatalog);
        }
    }
}
