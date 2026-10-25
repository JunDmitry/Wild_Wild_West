using Game.Arena.Application.Ports;

namespace Game.Arena.Application.Tests.Ticks
{
    internal sealed class RecordingArenaSceneReloader : IArenaSceneReloader
    {
        public int ReloadCount { get; private set; }

        public void ReloadArenaScene()
        {
            ReloadCount++;
        }
    }
}
