using System;
using Game.Arena.Application.Spawning;
using Game.Arena.Domain.Time;
using NUnit.Framework;

namespace Game.Arena.Application.Tests.Spawning
{
    [TestFixture]
    public sealed class FixedIntervalSpawnPacingPolicyTests
    {
        private FixedIntervalSpawnPacingPolicy _policy;

        [SetUp]
        public void SetUp()
        {
            _policy = new FixedIntervalSpawnPacingPolicy(new GameDuration(1d));
        }

        [Test]
        public void ConstructorRejectsNonPositiveInterval()
        {
            Assert.Throws<ArgumentOutOfRangeException>(
                () =>
                {
                    _ = new FixedIntervalSpawnPacingPolicy(new GameDuration(0d));
                });
        }

        [Test]
        public void FirstAttemptIsAllowedImmediately()
        {
            Assert.That(_policy.ShouldRequestSpawn(new GameTimePoint(0d)), Is.True);
        }

        [Test]
        public void AttemptIsBlockedBeforeIntervalElapses()
        {
            _policy.RecordSuccessfulSpawn(new GameTimePoint(1d));

            Assert.That(_policy.ShouldRequestSpawn(new GameTimePoint(1.5d)), Is.False);
        }

        [Test]
        public void AttemptIsAllowedWhenIntervalElapsed()
        {
            _policy.RecordSuccessfulSpawn(new GameTimePoint(1d));

            Assert.That(_policy.ShouldRequestSpawn(new GameTimePoint(2d)), Is.True);
        }

        [Test]
        public void ObservingScheduleDoesNotConsumeOpportunity()
        {
            _policy.ShouldRequestSpawn(new GameTimePoint(0d));

            Assert.That(_policy.ShouldRequestSpawn(new GameTimePoint(0d)), Is.True);
        }

        [Test]
        public void ResetAllowsImmediateAttemptAgain()
        {
            _policy.RecordSuccessfulSpawn(new GameTimePoint(5d));
            _policy.Reset();

            Assert.That(_policy.ShouldRequestSpawn(new GameTimePoint(5d)), Is.True);
        }
    }
}
