using System;
using Game.Arena.Domain.Concurrency;
using Game.Arena.Domain.Events;
using Game.Arena.Domain.Identity;
using NUnit.Framework;

namespace Game.Arena.Domain.Tests.Events
{
    [TestFixture]
    public sealed class ArenaRunLifecycleEventTests
    {
        [Test]
        public void DefeatedEventRejectsNoneArenaRunId()
        {
            Assert.Throws<ArgumentException>(
                () =>
                {
                    ArenaRunDefeated unused = new(
                        ArenaRunId.None,
                        AggregateRevision.Initial.Next());
                });
        }

        [Test]
        public void VictoriousEventRejectsNoneArenaRunId()
        {
            Assert.Throws<ArgumentException>(
                () =>
                {
                    ArenaRunVictorious unused = new(
                        ArenaRunId.None,
                        AggregateRevision.Initial.Next());
                });
        }

        [Test]
        public void EqualDefeatedEventsAreEqual()
        {
            ArenaRunId runId = ArenaRunId.FromValue(10UL);
            AggregateRevision revision = AggregateRevision.Initial.Next();

            ArenaRunDefeated left = new(runId, revision);
            ArenaRunDefeated right = new(runId, revision);

            Assert.That(left, Is.EqualTo(right));
        }
    }
}
