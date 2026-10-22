using System;
using Game.Arena.Domain.Time;

namespace Game.Arena.Application.Restart
{
    public sealed class FixedIntervalRunRestartDelayPolicy : IRunRestartDelayPolicy
    {
        private readonly GameTimePoint _delay;

        private GameTimePoint _elapsed;

        public FixedIntervalRunRestartDelayPolicy(GameDuration delay)
        {
            if (delay.Seconds <= 0d)
            {
                throw new ArgumentOutOfRangeException(nameof(delay));
            }

            _delay = new GameTimePoint(delay.Seconds);
            _elapsed = new GameTimePoint(0d);
        }

        public bool ShouldRestart(GameDuration delta)
        {
            _elapsed += delta;

            return _elapsed >= _delay;
        }

        public void Reset()
        {
            _elapsed = new GameTimePoint(0d);
        }
    }
}
