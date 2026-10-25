using Game.Arena.Application.Ports;
using Game.Arena.Domain.Time;

namespace Game.Arena.Infrastructure.Time
{
    /// <summary>
    /// Unity adapter behind <see cref="IFrameTimeSource"/>: reports the frame's
    /// raw elapsed seconds from <c>Time.unscaledDeltaTime</c>, which the engine's
    /// time scale does not affect. Pausing and scaling belong to the Application
    /// clock built over this port.
    /// </summary>
    public sealed class UnityFrameTimeSource : IFrameTimeSource
    {
        public GameDuration GetFrameElapsed()
        {
            return new GameDuration(UnityEngine.Time.unscaledDeltaTime);
        }
    }
}
