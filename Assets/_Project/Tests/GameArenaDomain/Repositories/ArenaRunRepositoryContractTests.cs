using System;
using Game.Arena.Domain.Identity;
using Game.Arena.Domain.Repositories;
using Game.Arena.Domain.Tests.Support;
using NUnit.Framework;
using ArenaRunAggregate = Game.Arena.Domain.Aggregates.ArenaRun.ArenaRun;

namespace Game.Arena.Domain.Tests.Repositories
{
    [TestFixture]
    public sealed class ArenaRunRepositoryContractTests
    {
        private ArenaRunTestKit _kit;
        private IArenaRunRepository _repository;

        [SetUp]
        public void SetUp()
        {
            _kit = new ArenaRunTestKit();
            _repository = new TestArenaRunRepository();
        }

        [Test]
        public void AddedRunCanBeRetrieved()
        {
            ArenaRunAggregate run = _kit.StartRun();

            _repository.Add(run);
            bool found = _repository.TryGet(run.Id, out ArenaRunAggregate retrieved);

            Assert.That(found, Is.True);
            Assert.That(retrieved, Is.SameAs(run));
        }

        [Test]
        public void TryGetReturnsFalseForUnknownArenaRunId()
        {
            bool found = _repository.TryGet(ArenaRunId.FromValue(999UL), out ArenaRunAggregate retrieved);

            Assert.That(found, Is.False);
            Assert.That(retrieved, Is.Null);
        }

        [Test]
        public void AddingDuplicateArenaRunIdThrows()
        {
            ArenaRunAggregate first = _kit.StartRun();
            _repository.Add(first);

            ArenaRunAggregate second = _kit.StartRunWithSameId(first.Id);

            Assert.Throws<InvalidOperationException>(
                () =>
                {
                    _repository.Add(second);
                });
        }

        [Test]
        public void RemoveReturnsTrueWhenArenaRunExisted()
        {
            ArenaRunAggregate run = _kit.StartRun();
            _repository.Add(run);

            bool removed = _repository.Remove(run.Id);
            bool foundAfterRemove = _repository.TryGet(run.Id, out ArenaRunAggregate retrieved);

            Assert.That(removed, Is.True);
            Assert.That(foundAfterRemove, Is.False);
        }

        [Test]
        public void RemoveReturnsFalseForUnknownArenaRunId()
        {
            bool removed = _repository.Remove(ArenaRunId.FromValue(999UL));

            Assert.That(removed, Is.False);
        }

        [Test]
        public void RetrievedRunReflectsMutationsAppliedThroughAggregateApi()
        {
            ArenaRunAggregate run = _kit.StartRun();
            _repository.Add(run);

            run.SwitchWeapon();

            _repository.TryGet(run.Id, out ArenaRunAggregate retrieved);

            Assert.That(retrieved.SelectedWeapon, Is.EqualTo(run.SelectedWeapon));
        }
    }
}
