using Game.Arena.Application.ReadModels;

namespace Game.Arena.Application.Dispatch
{
    public interface IArenaRunSnapshotHandler
    {
        void Handle(ArenaRunSnapshot snapshot);
    }
}
