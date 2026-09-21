using Game.Arena.Application.Sessions;
using Game.Arena.Domain.Aggregates.ArenaRun;
using Game.Arena.Domain.Concurrency;
using Game.Arena.Domain.Movement;
using Game.Arena.Domain.Repositories;
using NUnit.Framework;

namespace Game.Arena.Application.Tests.Sessions
{
    [TestFixture]
    public sealed class ArenaRunSessionInitialStartTests
    {
        private IArenaRunRepository _repository;
        private RecordingArenaRunIdSource _arenaRunIdSource;
        private RecordingPlayerIdSource _playerIdSource;
        private ArenaRunCreationParameters _creationParameters;
        private ArenaRunSession _session;

        [SetUp]
        public void SetUp()
        {
            _repository = new TestArenaRunRepository();
            _arenaRunIdSource = new RecordingArenaRunIdSource();
            _playerIdSource = new RecordingPlayerIdSource();
            _creationParameters = new ArenaRunCreationParametersFactory().Create();

            ArenaRunSessionDependencies dependencies = new(
                _repository,
                _arenaRunIdSource,
                _playerIdSource,
                new ArenaRunFactory(new MovementPathPolicy()),
                new ReadModels.ArenaRunSnapshotMapper());

            _session = new ArenaRunSession(dependencies, _creationParameters);
        }

        [Test]
        public void InitialRunAllocatesNewArenaRunId()
        {
            InitialRunStartOutcome outcome = _session.StartInitialRun();

            Assert.That(outcome.IsStarted, Is.True);
            Assert.That(_arenaRunIdSource.Allocated.Count, Is.EqualTo(1));
            Assert.That(outcome.ArenaRunId, Is.EqualTo(_arenaRunIdSource.Allocated[0]));
        }

        [Test]
        public void InitialRunAllocatesNewPlayerId()
        {
            InitialRunStartOutcome outcome = _session.StartInitialRun();

            Assert.That(_playerIdSource.Allocated.Count, Is.EqualTo(1));
            Assert.That(outcome.PlayerId, Is.EqualTo(_playerIdSource.Allocated[0]));
        }

        [Test]
        public void InitialRunIsAddedToRepository()
        {
            InitialRunStartOutcome outcome = _session.StartInitialRun();

            bool found = _repository.TryGet(outcome.ArenaRunId, out ArenaRun run);

            Assert.That(found, Is.True);
            Assert.That(run.Id, Is.EqualTo(outcome.ArenaRunId));
            Assert.That(run.PlayerId, Is.EqualTo(outcome.PlayerId));
        }

        [Test]
        public void InitialRunBecomesActive()
        {
            InitialRunStartOutcome outcome = _session.StartInitialRun();

            Assert.That(_session.HasActiveRun, Is.True);
            Assert.That(_session.ActiveArenaRunId, Is.EqualTo(outcome.ArenaRunId));
        }

        [Test]
        public void StartingInitialRunTwiceIsRejected()
        {
            _session.StartInitialRun();

            InitialRunStartOutcome second = _session.StartInitialRun();

            Assert.That(second.IsStarted, Is.False);
            Assert.That(second.Status, Is.EqualTo(InitialRunStartStatus.ActiveRunAlreadyExists));
        }

        [Test]
        public void RejectedInitialStartDoesNotAllocateIdentities()
        {
            _session.StartInitialRun();
            _session.StartInitialRun();

            Assert.That(_arenaRunIdSource.Allocated.Count, Is.EqualTo(1));
            Assert.That(_playerIdSource.Allocated.Count, Is.EqualTo(1));
        }

        [Test]
        public void GetRequiredActiveRunReturnsRepositoryAggregate()
        {
            InitialRunStartOutcome outcome = _session.StartInitialRun();

            _repository.TryGet(outcome.ArenaRunId, out ArenaRun expected);
            ArenaRun actual = _session.GetRequiredActiveRun();

            Assert.That(actual, Is.SameAs(expected));
        }

        [Test]
        public void GetRequiredActiveRunWithoutActiveRunThrows()
        {
            Assert.Throws<System.InvalidOperationException>(
                () =>
                {
                    _session.GetRequiredActiveRun();
                });
        }

        [Test]
        public void InitialRunReturnsSnapshot()
        {
            InitialRunStartOutcome outcome = _session.StartInitialRun();

            Assert.That(outcome.Snapshot, Is.Not.Null);
            Assert.That(outcome.Snapshot.ArenaRunId, Is.EqualTo(outcome.ArenaRunId));
            Assert.That(outcome.Snapshot.Player.PlayerId, Is.EqualTo(outcome.PlayerId));
            Assert.That(outcome.Snapshot.Revision, Is.EqualTo(AggregateRevision.Initial));
            Assert.That(outcome.Snapshot.Status, Is.EqualTo(ArenaRunStatus.Playing));
        }
    }
}
