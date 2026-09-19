using System;
using Game.Arena.Domain.Combat;
using Game.Arena.Domain.Identity;
using Game.Arena.Infrastructure.Repositories;
using Game.Arena.Infrastructure.Tests.Support;
using NUnit.Framework;
using ArenaRunAggregate = Game.Arena.Domain.Aggregates.ArenaRun.ArenaRun;

namespace Game.Arena.Infrastructure.Tests.Repositories
{
    [TestFixture]
    public sealed class InMemoryArenaRunRepositoryTests
    {
        private InMemoryArenaRunRepository _repository;
        private InfrastructureArenaRunFactory _factory;

        [SetUp]
        public void SetUp()
        {
            _repository = new InMemoryArenaRunRepository();
            _factory = new InfrastructureArenaRunFactory();
        }

        [Test]
        public void AddAndTryGetReturnSameAggregateReference()
        {
            ArenaRunAggregate run = _factory.Create(ArenaRunId.FromValue(1UL), PlayerId.FromValue(1UL));

            _repository.Add(run);

            bool found = _repository.TryGet(run.Id, out ArenaRunAggregate retrieved);

            Assert.That(found, Is.True);
            Assert.That(retrieved, Is.SameAs(run));
        }

        [Test]
        public void TryGetReturnsFalseForUnknownId()
        {
            bool found = _repository.TryGet(ArenaRunId.FromValue(999UL), out ArenaRunAggregate retrieved);

            Assert.That(found, Is.False);
            Assert.That(retrieved, Is.Null);
        }

        [Test]
        public void AddNullThrows()
        {
            Assert.Throws<ArgumentNullException>(
                () =>
                {
                    _repository.Add(null);
                });
        }

        [Test]
        public void AddDuplicateIdThrows()
        {
            ArenaRunAggregate first = _factory.Create(ArenaRunId.FromValue(1UL), PlayerId.FromValue(1UL));
            ArenaRunAggregate second = _factory.Create(ArenaRunId.FromValue(1UL), PlayerId.FromValue(2UL));

            _repository.Add(first);

            Assert.Throws<InvalidOperationException>(
                () =>
                {
                    _repository.Add(second);
                });
        }

        [Test]
        public void RemoveExistingRunReturnsTrue()
        {
            ArenaRunAggregate run = _factory.Create(ArenaRunId.FromValue(1UL), PlayerId.FromValue(1UL));

            _repository.Add(run);

            bool removed = _repository.Remove(run.Id);

            Assert.That(removed, Is.True);
        }

        [Test]
        public void RemovedRunCannotBeRetrieved()
        {
            ArenaRunAggregate run = _factory.Create(ArenaRunId.FromValue(1UL), PlayerId.FromValue(1UL));

            _repository.Add(run);
            _repository.Remove(run.Id);

            bool found = _repository.TryGet(run.Id, out ArenaRunAggregate retrieved);

            Assert.That(found, Is.False);
            Assert.That(retrieved, Is.Null);
        }

        [Test]
        public void RemoveUnknownIdReturnsFalse()
        {
            bool removed = _repository.Remove(ArenaRunId.FromValue(999UL));

            Assert.That(removed, Is.False);
        }

        [Test]
        public void AggregateMutationsRemainVisibleThroughRepository()
        {
            ArenaRunAggregate run = _factory.Create(ArenaRunId.FromValue(1UL), PlayerId.FromValue(1UL));

            _repository.Add(run);
            run.SwitchWeapon();

            _repository.TryGet(run.Id, out ArenaRunAggregate retrieved);

            Assert.That(retrieved.SelectedWeapon, Is.EqualTo(WeaponKind.Melee));
        }

        [Test]
        public void DifferentArenaRunIdsCanBeStoredIndependently()
        {
            ArenaRunAggregate first = _factory.Create(ArenaRunId.FromValue(1UL), PlayerId.FromValue(1UL));
            ArenaRunAggregate second = _factory.Create(ArenaRunId.FromValue(2UL), PlayerId.FromValue(2UL));

            _repository.Add(first);
            _repository.Add(second);

            Assert.That(_repository.TryGet(first.Id, out ArenaRunAggregate retrievedFirst), Is.True);
            Assert.That(_repository.TryGet(second.Id, out ArenaRunAggregate retrievedSecond), Is.True);
            Assert.That(retrievedFirst, Is.SameAs(first));
            Assert.That(retrievedSecond, Is.SameAs(second));
        }

        [Test]
        public void ConcurrentAddAndTryGetDoesNotCorruptCollection()
        {
            const int Count = 64;

            ArenaRunAggregate[] runs = new ArenaRunAggregate[Count];

            for (int index = 0; index < Count; index++)
            {
                runs[index] = _factory.Create(ArenaRunId.FromValue((ulong)(index + 1)), PlayerId.FromValue((ulong)(index + 1)));
            }

            System.Threading.Tasks.Parallel.For(
                0,
                Count,
                index =>
                {
                    _repository.Add(runs[index]);
                });

            for (int index = 0; index < Count; index++)
            {
                bool found = _repository.TryGet(runs[index].Id, out ArenaRunAggregate retrieved);

                Assert.That(found, Is.True);
                Assert.That(retrieved, Is.SameAs(runs[index]));
            }
        }
    }
}
