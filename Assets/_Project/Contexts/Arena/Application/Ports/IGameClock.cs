using Game.Arena.Domain.Time;

namespace Game.Arena.Application.Ports
{
    public interface IGameClock
    {
        GameDuration GetDelta();
    }
}
