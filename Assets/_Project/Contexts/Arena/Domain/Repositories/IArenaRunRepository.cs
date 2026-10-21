using Game.Arena.Domain.Identity;
using ArenaRunAggregate = Game.Arena.Domain.Aggregates.ArenaRun.ArenaRun;

namespace Game.Arena.Domain.Repositories
{
    public interface IArenaRunRepository
    {
        void Add(ArenaRunAggregate arenaRun);
        bool TryGet(ArenaRunId id, out ArenaRunAggregate arenaRun);
        bool Remove(ArenaRunId id);
    }
}
