using Game.Arena.Domain.Time;

namespace Game.Arena.Application.Ports
{
    public interface IFrameTimeSource
    {
        GameDuration GetFrameElapsed();
    }
}
