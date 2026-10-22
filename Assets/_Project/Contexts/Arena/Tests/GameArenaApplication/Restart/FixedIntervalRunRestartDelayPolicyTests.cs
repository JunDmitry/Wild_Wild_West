using System;
using Game.Arena.Application.Restart;
using Game.Arena.Domain.Time;
using NUnit.Framework;

namespace Game.Arena.Application.Tests.Restart
{
    [TestFixture]
    public sealed class FixedIntervalRunRestartDelayPolicyTests
    {
        [Test]
        public void Constructor_IfDelayIsZero_Throws()
        {
            Assert.Throws<ArgumentOutOfRangeException>(
                () =>
                {
                    _ = new FixedIntervalRunRestartDelayPolicy(new GameDuration(0d));
                });
        }

        [Test]
        public void ShouldRestart_ReturnsFalseBeforeTheDelay()
        {
            FixedIntervalRunRestartDelayPolicy policy = new(new GameDuration(1d));

            Assert.That(policy.ShouldRestart(new GameDuration(0.4d)), Is.False);
            Assert.That(policy.ShouldRestart(new GameDuration(0.4d)), Is.False);
        }

        [Test]
        public void ShouldRestart_ReturnsTrueWhenTheDelayElapses()
        {
            FixedIntervalRunRestartDelayPolicy policy = new(new GameDuration(1d));

            policy.ShouldRestart(new GameDuration(0.5d));

            Assert.That(policy.ShouldRestart(new GameDuration(0.5d)), Is.True);
        }

        [Test]
        public void Reset_DiscardsTheAccumulatedTime()
        {
            FixedIntervalRunRestartDelayPolicy policy = new(new GameDuration(1d));

            policy.ShouldRestart(new GameDuration(0.9d));
            policy.Reset();

            Assert.That(policy.ShouldRestart(new GameDuration(0.4d)), Is.False);
        }

        [Test]
        public void ShouldRestart_StaysTrueAfterTheDelayElapsed()
        {
            FixedIntervalRunRestartDelayPolicy policy = new(new GameDuration(0.5d));

            Assert.That(policy.ShouldRestart(new GameDuration(0.5d)), Is.True);
            Assert.That(policy.ShouldRestart(new GameDuration(0.1d)), Is.True);
        }
    }
}
