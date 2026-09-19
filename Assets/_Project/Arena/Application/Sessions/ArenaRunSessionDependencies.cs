using System;
using Game.Arena.Application.Identity;
using Game.Arena.Domain.Aggregates.ArenaRun;
using Game.Arena.Domain.Repositories;

namespace Game.Arena.Application.Sessions
{
    public sealed class ArenaRunSessionDependencies
    {
        public ArenaRunSessionDependencies(
            IArenaRunRepository repository,
            IArenaRunIdSource arenaRunIdSource,
            IPlayerIdSource playerIdSource,
            ArenaRunFactory arenaRunFactory)
        {
            if (repository == null)
            {
                throw new ArgumentNullException(nameof(repository));
            }

            if (arenaRunIdSource == null)
            {
                throw new ArgumentNullException(nameof(arenaRunIdSource));
            }

            if (playerIdSource == null)
            {
                throw new ArgumentNullException(nameof(playerIdSource));
            }

            if (arenaRunFactory == null)
            {
                throw new ArgumentNullException(nameof(arenaRunFactory));
            }

            Repository = repository;
            ArenaRunIdSource = arenaRunIdSource;
            PlayerIdSource = playerIdSource;
            ArenaRunFactory = arenaRunFactory;
        }

        public IArenaRunRepository Repository { get; }

        public IArenaRunIdSource ArenaRunIdSource { get; }

        public IPlayerIdSource PlayerIdSource { get; }

        public ArenaRunFactory ArenaRunFactory { get; }
    }
}
