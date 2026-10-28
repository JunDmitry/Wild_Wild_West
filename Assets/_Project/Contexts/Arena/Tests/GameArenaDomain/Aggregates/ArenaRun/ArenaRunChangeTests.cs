using System;
using Game.Arena.Domain.Aggregates.ArenaRun;
using Game.Arena.Domain.Concurrency;
using Game.Arena.Domain.Events;
using Game.Arena.Domain.Identity;
using NUnit.Framework;

namespace Game.Arena.Domain.Tests.Aggregates._ArenaRun
{
    [TestFixture]
    public sealed class ArenaRunChangeTests
    {
        [Test]
        public void ConstructorStoresDomainEventCollectionWithoutCopying()
        {
            ArenaRunId runId = ArenaRunId.FromValue(1UL);
            AggregateRevision revision = AggregateRevision.Initial.Next();
            IArenaDomainEvent[] source =
            {
                new ArenaRunDefeated(runId, revision),
            };

            ArenaRunChange change = new(revision, source);
            source[0] = new ArenaRunVictorious(runId, revision);

            // ADR-0036 point 5: the change no longer copies its caller's list;
            // it is valid only until the next operation.
            Assert.That(change.DomainEvents.Count, Is.EqualTo(1));
            Assert.That(change.DomainEvents[0], Is.TypeOf<ArenaRunVictorious>());
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
            IArenaDomainEvent[] events =
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

        [Test]
        public void ConstructorStoresStateChangeFlag()
        {
            ArenaRunChange change = new(AggregateRevision.Initial.Next(), true, Array.Empty<IArenaDomainEvent>());

            Assert.That(change.HasStateChange, Is.True);
        }
    }
}
