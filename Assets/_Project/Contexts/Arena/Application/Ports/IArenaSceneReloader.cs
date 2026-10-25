namespace Game.Arena.Application.Ports
{
    /// <summary>
    /// Requests a reload of the arena scene. Implemented by the host adapter;
    /// requested once per defeat restart, immediately after the restarted run
    /// exists. The request is a command: it carries no fact about the run.
    /// </summary>
    public interface IArenaSceneReloader
    {
        void ReloadArenaScene();
    }
}
