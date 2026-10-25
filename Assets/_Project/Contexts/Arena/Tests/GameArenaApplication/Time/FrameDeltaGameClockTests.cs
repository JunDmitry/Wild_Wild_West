using System;
using Game.Arena.Application.Ports;
using Game.Arena.Application.Time;
using Game.Arena.Domain.Time;
using NUnit.Framework;

namespace Game.Arena.Application.Tests.Time
{
    [TestFixture]
    public sealed class FrameDeltaGameClockTests
    {
        [Test]
        public void Constructor_DefaultsToFullScaleAndRunning()
        {
            FrameDeltaGameClock clock = new(new ScriptedFrameTimeSource(0.5d));

            Assert.That(clock.Scale, Is.EqualTo(1d));
            Assert.That(clock.IsPaused, Is.False);
        }

        [Test]
        public void Constructor_IfSourceIsNull_Throws()
        {
            Assert.Throws<ArgumentNullException>(
                () =>
                {
                    _ = new FrameDeltaGameClock(null);
                });
        }

        [Test]
        public void GetDelta_WithDefaultScale_PassesFrameElapsedThrough()
        {
            FrameDeltaGameClock clock = new(new ScriptedFrameTimeSource(0.25d));

            Assert.That(clock.GetDelta(), Is.EqualTo(new GameDuration(0.25d)));
        }

        [Test]
        public void GetDelta_WithScale_MultipliesFrameElapsed()
        {
            FrameDeltaGameClock clock = new(new ScriptedFrameTimeSource(0.2d));

            clock.SetScale(0.5d);

            Assert.That(clock.GetDelta(), Is.EqualTo(new GameDuration(0.1d)));
        }

        [Test]
        public void GetDelta_WhilePaused_IsZero()
        {
            FrameDeltaGameClock clock = new(new ScriptedFrameTimeSource(0.2d));

            clock.Pause();

            Assert.That(clock.IsPaused, Is.True);
            Assert.That(clock.GetDelta(), Is.EqualTo(new GameDuration(0d)));
        }

        [Test]
        public void GetDelta_AfterResume_RestoresTheScaledValue()
        {
            FrameDeltaGameClock clock = new(new ScriptedFrameTimeSource(0.2d));
            clock.SetScale(2d);
            clock.Pause();

            clock.Resume();

            Assert.That(clock.IsPaused, Is.False);
            Assert.That(clock.GetDelta(), Is.EqualTo(new GameDuration(0.4d)));
        }

        [Test]
        public void GetDelta_WithZeroScale_IsZero()
        {
            FrameDeltaGameClock clock = new(new ScriptedFrameTimeSource(0.2d));

            clock.SetScale(0d);

            Assert.That(clock.GetDelta(), Is.EqualTo(new GameDuration(0d)));
        }

        [Test]
        public void SetScale_IfNegative_Throws()
        {
            FrameDeltaGameClock clock = new(new ScriptedFrameTimeSource(0.2d));

            Assert.Throws<ArgumentOutOfRangeException>(() => clock.SetScale(-0.1d));
        }

        [Test]
        public void SetScale_IfNaN_Throws()
        {
            FrameDeltaGameClock clock = new(new ScriptedFrameTimeSource(0.2d));

            Assert.Throws<ArgumentOutOfRangeException>(() => clock.SetScale(double.NaN));
        }

        [Test]
        public void SetScale_IfPositiveInfinity_Throws()
        {
            FrameDeltaGameClock clock = new(new ScriptedFrameTimeSource(0.2d));

            Assert.Throws<ArgumentOutOfRangeException>(
                () => clock.SetScale(double.PositiveInfinity));
        }

        [Test]
        public void GetDelta_ReadsTheSourceOnEveryCall()
        {
            ScriptedFrameTimeSource source = new(0.1d, 0.25d);
            FrameDeltaGameClock clock = new(source);

            GameDuration first = clock.GetDelta();
            GameDuration second = clock.GetDelta();

            Assert.That(first, Is.EqualTo(new GameDuration(0.1d)));
            Assert.That(second, Is.EqualTo(new GameDuration(0.25d)));
            Assert.That(source.CallCount, Is.EqualTo(2));
        }

        private sealed class ScriptedFrameTimeSource : IFrameTimeSource
        {
            private readonly double[] _values;

            private int _index;

            public ScriptedFrameTimeSource(params double[] values)
            {
                _values = values;
            }

            public int CallCount { get; private set; }

            public GameDuration GetFrameElapsed()
            {
                int lastIndex = _values.Length - 1;
                double value = _values[Math.Min(_index, lastIndex)];
                _index++;
                CallCount++;

                return new GameDuration(value);
            }
        }
    }
}
