using System;
using Game.Arena.Application.Ports;
using Game.Arena.Domain.Time;

namespace Game.Arena.Application.Time
{
    public sealed class FrameDeltaGameClock : IGameClock
    {
        private readonly IFrameTimeSource _source;

        private double _scale;

        private bool _isPaused;

        public FrameDeltaGameClock(IFrameTimeSource source)
        {
            _source = source ?? throw new ArgumentNullException(nameof(source));
            _scale = 1d;
        }

        public double Scale => _scale;

        public bool IsPaused => _isPaused;

        public void SetScale(double scale)
        {
            if (double.IsNaN(scale) || double.IsInfinity(scale) || scale < 0d)
            {
                throw new ArgumentOutOfRangeException(nameof(scale));
            }

            _scale = scale;
        }

        public void Pause()
        {
            _isPaused = true;
        }

        public void Resume()
        {
            _isPaused = false;
        }

        public GameDuration GetDelta()
        {
            GameDuration frameElapsed = _source.GetFrameElapsed();

            if (_isPaused || _scale == 0d)
            {
                return new GameDuration(0d);
            }

            return new GameDuration(frameElapsed.Seconds * _scale);
        }
    }
}
