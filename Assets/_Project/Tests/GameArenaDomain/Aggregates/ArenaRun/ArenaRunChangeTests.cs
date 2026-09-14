using System;
using Game.Arena.Domain.Aggregates.ArenaRun;
using Game.Arena.Domain.Concurrency;
using Game.Arena.Domain.Events;
using Game.Arena.Domain.Identity;
using NUnit.Framework;

namespace Game.Arena.Domain.Tests.Aggregates.ArenaRun
{
    [TestFixture]
    public sealed class ArenaRunChangeTests
    {
        [Test]
        public void ConstructorCopiesDomainEventCollection()
        {
            ArenaRunId runId = ArenaRunId.FromValue(1UL);
            AggregateRevision revision = AggregateRevision.Initial.Next();
            IDomainEvent[] source =
            {
                new ArenaRunDefeated(runId, revision),
            };

            ArenaRunChange change = new(revision, source);
            source[0] = new ArenaRunVictorious(runId, revision);

            Assert.That(change.DomainEvents.Count, Is.EqualTo(1));
            Assert.That(change.DomainEvents[0], Is.TypeOf<ArenaRunDefeated>());
        }

        [Test]
        public void ConstructorRejectsNullCollection()
        {
            Assert.Throws<ArgumentNullException>(
                () =>
                {
                    ArenaRunChange unused = new(
                        AggregateRevision.Initial,
                        null);
                });
        }

        [Test]
        public void ConstructorRejectsNullDomainEvent()
        {
            IDomainEvent[] events =
            {
                null,
            };

            Assert.Throws<ArgumentException>(
                () =>
                {
                    ArenaRunChange unused = new(
                        AggregateRevision.Initial,
                        events);
                });
        }
    }
}
