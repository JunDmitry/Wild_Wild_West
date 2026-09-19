using System;
using System.Collections.Generic;
using Game.Arena.Application.Ticks;
using Game.Arena.Domain.Combat;
using Game.Arena.Domain.Concurrency;
using Game.Arena.Domain.Events;
using Game.Arena.Domain.Identity;
using Game.Arena.Domain.Time;
using NUnit.Framework;

namespace Game.Arena.Application.Tests.Ticks
{
    [TestFixture]
    public sealed class ArenaRunTickResultTests
    {
        private ArenaRunId _arenaRunId;
        private AggregateRevision _firstRevision;

        [SetUp]
        public void SetUp()
        {
            _arenaRunId = ArenaRunId.FromValue(1UL);
            _firstRevision = AggregateRevision.Initial.Next();
        }

        [Test]
        public void ConstructorCopiesEventAndStageCollections()
        {
            IArenaDomainEvent[] events =
            {
                CreateStartedEvent(_arenaRunId, _firstRevision),
            };

            ArenaRunTickStage[] stages =
            {
                ArenaRunTickStage.PlayerAttackStart,
            };

            ArenaRunTickResult result = new(
                _arenaRunId,
                _firstRevision,
                events,
                stages);

            events[0] = CreateCompletedEvent(_firstRevision);
            stages[0] = ArenaRunTickStage.EnemySpawn;

            Assert.That(result.DomainEvents[0], Is.TypeOf<PlayerAttackStarted>());
            Assert.That(result.ExecutedStages[0], Is.EqualTo(ArenaRunTickStage.PlayerAttackStart));
        }

        [Test]
        public void PublishedCollectionsCannotBeModifiedThroughCollectionInterfaces()
        {
            ArenaRunTickResult result = new(
                _arenaRunId,
                _firstRevision,
                new IArenaDomainEvent[]
                {
                    CreateStartedEvent(_arenaRunId, _firstRevision),
                },
                new[]
                {
                    ArenaRunTickStage.PlayerAttackStart,
                });

            Assert.Throws<NotSupportedException>(
                () =>
                {
                    ((IList<IArenaDomainEvent>)result.DomainEvents).Clear();
                });

            Assert.Throws<NotSupportedException>(
                () =>
                {
                    ((IList<ArenaRunTickStage>)result.ExecutedStages).Clear();
                });
        }

        [Test]
        public void EventsKeepTheirOwnRevisionsAndOrder()
        {
            AggregateRevision secondRevision = _firstRevision.Next();
            AggregateRevision finalRevision = secondRevision.Next();

            ArenaRunTickResult result = new(
                _arenaRunId,
                finalRevision,
                new IArenaDomainEvent[]
                {
                    CreateStartedEvent(_arenaRunId, _firstRevision),
                    CreateCompletedEvent(secondRevision),
                },
                new[]
                {
                    ArenaRunTickStage.PlayerAttackStart,
                    ArenaRunTickStage.PlayerAttackImpact,
                    ArenaRunTickStage.EnemyMovement,
                });

            Assert.That(result.DomainEvents[0].AggregateRevision, Is.EqualTo(_firstRevision));
            Assert.That(result.DomainEvents[1].AggregateRevision, Is.EqualTo(secondRevision));
            Assert.That(result.FinalRevision, Is.EqualTo(finalRevision));
            Assert.That(result.DomainEvents[0], Is.TypeOf<PlayerAttackStarted>());
            Assert.That(result.DomainEvents[1], Is.TypeOf<PlayerAttackCompleted>());
        }

        [Test]
        public void ConstructorRejectsEventFromAnotherArenaRun()
        {
            Assert.Throws<ArgumentException>(
                () =>
                {
                    _ = new ArenaRunTickResult(
                        _arenaRunId,
                        _firstRevision,
                        new IArenaDomainEvent[]
                        {
                            CreateStartedEvent(
                                ArenaRunId.FromValue(2UL),
                                _firstRevision),
                        },
                        Array.Empty<ArenaRunTickStage>());
                });
        }

        [Test]
        public void ConstructorRejectsEventBeyondFinalRevision()
        {
            Assert.Throws<ArgumentException>(
                () =>
                {
                    _ = new ArenaRunTickResult(
                        _arenaRunId,
                        _firstRevision,
                        new IArenaDomainEvent[]
                        {
                            CreateStartedEvent(
                                _arenaRunId,
                                _firstRevision.Next()),
                        },
                        Array.Empty<ArenaRunTickStage>());
                });
        }

        [Test]
        public void ConstructorRejectsDecreasingEventRevisions()
        {
            AggregateRevision secondRevision = _firstRevision.Next();

            Assert.Throws<ArgumentException>(
                () =>
                {
                    _ = new ArenaRunTickResult(
                        _arenaRunId,
                        secondRevision,
                        new IArenaDomainEvent[]
                        {
                            CreateCompletedEvent(secondRevision),
                            CreateStartedEvent(
                                _arenaRunId,
                                _firstRevision),
                        },
                        Array.Empty<ArenaRunTickStage>());
                });
        }

        [Test]
        public void ConstructorRejectsNoneArenaRunId()
        {
            Assert.Throws<ArgumentException>(
                () =>
                {
                    _ = new ArenaRunTickResult(
                        ArenaRunId.None,
                        AggregateRevision.Initial,
                        Array.Empty<IArenaDomainEvent>(),
                        Array.Empty<ArenaRunTickStage>());
                });
        }

        [Test]
        public void ConstructorRejectsUnknownStage()
        {
            Assert.Throws<ArgumentException>(
                () =>
                {
                    _ = new ArenaRunTickResult(
                        _arenaRunId,
                        AggregateRevision.Initial,
                        Array.Empty<IArenaDomainEvent>(),
                        new[]
                        {
                            (ArenaRunTickStage)999,
                        });
                });
        }

        [Test]
        public void EmptyCollectionsAreAllowed()
        {
            ArenaRunTickResult result = new(
                _arenaRunId,
                AggregateRevision.Initial,
                Array.Empty<IArenaDomainEvent>(),
                Array.Empty<ArenaRunTickStage>());

            Assert.That(result.DomainEvents, Is.Empty);
            Assert.That(result.ExecutedStages, Is.Empty);
        }

        private PlayerAttackStarted CreateStartedEvent(
            ArenaRunId arenaRunId,
            AggregateRevision revision)
        {
            return new PlayerAttackStarted(
                arenaRunId,
                revision,
                PlayerId.FromValue(1UL),
                AttackId.None.Next(),
                WeaponKind.Ranged,
                new GameTimePoint(0d),
                new GameTimePoint(0d));
        }

        private PlayerAttackCompleted CreateCompletedEvent(
            AggregateRevision revision)
        {
            return new PlayerAttackCompleted(
                _arenaRunId,
                revision,
                PlayerId.FromValue(1UL),
                AttackId.None.Next(),
                WeaponKind.Ranged,
                AttackOutcome.Miss,
                new GameTimePoint(0d));
        }
    }
}
