namespace Game.Arena.Application.Dispatch
{
    public interface IArenaRunDispatchDiagnostics
    {
        void OnHandlerFailed(ArenaRunDispatchFailure failure);
    }
}
