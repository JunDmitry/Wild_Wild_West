using System;
using System.Collections.Generic;
using Game.Arena.Domain.Aggregates.ArenaRun;
using Game.Arena.Domain.Identity;
using Game.Arena.Domain.Repositories;

namespace Game.Arena.Application.Tests.Sessions
{
    internal sealed class TestArenaRunRepository : IArenaRunRepository
    {
        private readonly Dictionary<ArenaRunId, ArenaRun> _runs = new();

        public void Add(ArenaRun arenaRun)
        {
            if (arenaRun == null)
            {
                throw new ArgumentNullException(nameof(arenaRun));
            }

            if (_runs.ContainsKey(arenaRun.Id))
            {
                throw new InvalidOperationException("ArenaRun with the same ArenaRunId already exists.");
            }

            _runs.Add(arenaRun.Id, arenaRun);
        }

        public bool TryGet(ArenaRunId id, out ArenaRun arenaRun)
        {
            return _runs.TryGetValue(id, out arenaRun);
        }

        public bool Remove(ArenaRunId id)
        {
            return _runs.Remove(id);
        }
    }
}
