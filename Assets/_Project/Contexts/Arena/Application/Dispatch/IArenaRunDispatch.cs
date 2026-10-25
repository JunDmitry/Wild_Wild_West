using Game.Arena.Application.Ticks;

namespace Game.Arena.Application.Dispatch
{
    public interface IArenaRunDispatch
    {
        void Dispatch(ArenaRunTickResult result);
    }
}
