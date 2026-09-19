using System;
using System.Collections.Generic;
using Game.Arena.Domain.Identity;
using Game.Arena.Domain.Repositories;
using ArenaRunAggregate = Game.Arena.Domain.Aggregates.ArenaRun.ArenaRun;

namespace Game.Arena.Infrastructure.Repositories
{
    public sealed class InMemoryArenaRunRepository : IArenaRunRepository
    {
        private readonly object _sync;
        private readonly Dictionary<ArenaRunId, ArenaRunAggregate> _runs;

        public InMemoryArenaRunRepository()
        {
            _sync = new object();
            _runs = new Dictionary<ArenaRunId, ArenaRunAggregate>();
        }

        public void Add(ArenaRunAggregate arenaRun)
        {
            if (arenaRun == null)
            {
                throw new ArgumentNullException(nameof(arenaRun));
            }

            lock (_sync)
            {
                if (_runs.ContainsKey(arenaRun.Id))
                {
                    throw new InvalidOperationException("ArenaRun with the same ArenaRunId already exists.");
                }

                _runs.Add(arenaRun.Id, arenaRun);
            }
        }

        public bool TryGet(
            ArenaRunId id,
            out ArenaRunAggregate arenaRun)
        {
            lock (_sync)
            {
                return _runs.TryGetValue(id, out arenaRun);
            }
        }

        public bool Remove(ArenaRunId id)
        {
            lock (_sync)
            {
                return _runs.Remove(id);
            }
        }
    }
}
