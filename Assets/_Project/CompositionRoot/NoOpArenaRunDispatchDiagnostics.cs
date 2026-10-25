using Game.Arena.Application.Dispatch;

namespace Game.CompositionRoot
{
    /// <summary>
    /// Production null object for the optional dispatch diagnostics port
    /// (ADR-0027, ADR-0030 amendment). It expresses "no diagnostics configured"
    /// to the container without bypassing VContainer's constructor selection;
    /// a later adapter with a logging policy replaces its one registration.
    /// </summary>
    public sealed class NoOpArenaRunDispatchDiagnostics : IArenaRunDispatchDiagnostics
    {
        public void OnHandlerFailed(ArenaRunDispatchFailure failure)
        {
        }
    }
}
