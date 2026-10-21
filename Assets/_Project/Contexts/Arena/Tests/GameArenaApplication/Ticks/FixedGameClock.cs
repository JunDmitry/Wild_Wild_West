using Game.Arena.Application.Ports;
using Game.Arena.Domain.Time;

namespace Game.Arena.Application.Tests.Ticks
{
    internal sealed class FixedGameClock : IGameClock
    {
        public FixedGameClock(GameDuration delta)
        {
            Delta = delta;
        }

        public GameDuration Delta { get; set; }

        public GameDuration GetDelta()
        {
            return Delta;
        }
    }
}
