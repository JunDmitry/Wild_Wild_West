using System.Collections;
using Game.Arena.Infrastructure.Time;
using NUnit.Framework;
using UnityEngine;
using UnityEngine.TestTools;

namespace Game.Arena.PlayMode.Tests
{
    [TestFixture]
    public sealed class UnityFrameTimeSourceTests
    {
        private const int SampleFrames = 5;
        private const double ToleranceSeconds = 0.25d;

        private float _previousTimeScale;

        [SetUp]
        public void SetUp()
        {
            _previousTimeScale = Time.timeScale;
        }

        [TearDown]
        public void TearDown()
        {
            Time.timeScale = _previousTimeScale;
        }

        [UnityTest]
        public IEnumerator GetFrameElapsed_AcrossFrames_TracksUnscaledTime()
        {
            UnityFrameTimeSource source = new();

            double start = Time.unscaledTime;
            double total = 0d;
            bool anyPositive = false;

            for (int frame = 0; frame < SampleFrames; frame++)
            {
                yield return null;

                double delta = source.GetFrameElapsed().Seconds;
                Assert.That(delta, Is.GreaterThanOrEqualTo(0d));

                anyPositive |= delta > 0d;
                total += delta;
            }

            double elapsed = Time.unscaledTime - start;

            Assert.That(anyPositive, Is.True);
            Assert.That(total, Is.GreaterThan(0d));
            Assert.That(total, Is.EqualTo(elapsed).Within(ToleranceSeconds));
        }

        [UnityTest]
        public IEnumerator GetFrameElapsed_WithZeroTimeScale_DoesNotZero()
        {
            Time.timeScale = 0f;

            UnityFrameTimeSource source = new();
            double total = 0d;

            for (int frame = 0; frame < SampleFrames; frame++)
            {
                yield return null;

                double delta = source.GetFrameElapsed().Seconds;
                Assert.That(delta, Is.GreaterThanOrEqualTo(0d));

                total += delta;
            }

            Assert.That(total, Is.GreaterThan(0d));
        }
    }
}
