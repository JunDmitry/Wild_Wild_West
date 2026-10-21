using System;
using Game.Arena.Domain.Aggregates.ArenaRun;
using Game.Arena.Domain.Concurrency;
using Game.Arena.Domain.Events;
using Game.Arena.Domain.Identity;
using NUnit.Framework;

namespace Game.Arena.Domain.Tests.Events
{
    [TestFixture]
    public sealed class WaveEventTests
    {
        [Test]
        public void WavePhaseChangedRejectsNoneArenaRunId()
        {
            Assert.Throws<ArgumentException>(
                () =>
                {
                    WavePhaseChanged unused = new(
                        ArenaRunId.None,
                        AggregateRevision.Initial.Next(),
                        WaveNumber.First,
                        WavePhase.RegularCombat,
                        WavePhase.BossCombat);
                });
        }

        [Test]
        public void WavePhaseChangedRejectsSamePhase()
        {
            Assert.Throws<ArgumentException>(
                () =>
                {
                    WavePhaseChanged unused = new(
                        ArenaRunId.FromValue(1UL),
                        AggregateRevision.Initial.Next(),
                        WaveNumber.First,
                        WavePhase.RegularCombat,
                        WavePhase.RegularCombat);
                });
        }

        [Test]
        public void WaveCompletedRejectsNoneArenaRunId()
        {
            Assert.Throws<ArgumentException>(
                () =>
                {
                    WaveCompleted unused = new(
                        ArenaRunId.None,
                        AggregateRevision.Initial.Next(),
                        WaveNumber.First);
                });
        }

        [Test]
        public void WaveStartedRejectsNoneArenaRunId()
        {
            Assert.Throws<ArgumentException>(
                () =>
                {
                    WaveStarted unused = new(
                        ArenaRunId.None,
                        AggregateRevision.Initial.Next(),
                        WaveNumber.First,
                        WavePhase.RegularCombat);
                });
        }

        [Test]
        public void EqualWaveCompletedEventsAreEqual()
        {
            ArenaRunId runId = ArenaRunId.FromValue(1UL);
            AggregateRevision revision = AggregateRevision.Initial.Next();

            WaveCompleted left = new(runId, revision, WaveNumber.First);
            WaveCompleted right = new(runId, revision, WaveNumber.First);

            Assert.That(left, Is.EqualTo(right));
        }
    }
}
