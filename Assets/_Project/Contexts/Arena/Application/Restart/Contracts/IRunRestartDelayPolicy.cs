using Game.Arena.Domain.Time;

namespace Game.Arena.Application.Restart
{
    public interface IRunRestartDelayPolicy
    {
        bool ShouldRestart(GameDuration delta);
        void Reset();
    }
}
