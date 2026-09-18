using Game.Arena.Domain.Identity;

namespace Game.Arena.Application.Identity
{
    public interface IArenaRunIdSource
    {
        ArenaRunId Allocate();
    }
}
