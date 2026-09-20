using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Game.Arena.Application.Tests.Ticks;
using Game.Arena.Domain.Aggregates.ArenaRun;
using Game.Arena.Domain.Concurrency;
using Game.Arena.Domain.Geometry;
using Game.Arena.Domain.Identity;
using NUnit.Framework;

namespace Game.Arena.Application.ReadModels
{
    [TestFixture]
    public sealed class ArenaRunSnapshotMapperTests
    {
        private TickTestKit _kit;
        private ArenaRunSnapshotMapper _mapper;

        [SetUp]
        public void SetUp()
        {
            _kit = new TickTestKit();
            _mapper = new ArenaRunSnapshotMapper();
        }

        [Test]
        public void MapCopiesAggregateSnapshotValues()
        {
            ArenaRun run = _kit.StartStandartRun();
            _kit.Spawn(run, 10UL, new Position3D(22f, 0f, 0f));

            ArenaRunSnapshot applicationSnapshot = _mapper.Map(run.CreateSnapshot());

            Assert.That(applicationSnapshot.ArenaRunId, Is.EqualTo(run.Id));
            Assert.That(applicationSnapshot.Revision, Is.EqualTo(run.Revision));
            Assert.That(applicationSnapshot.Status, Is.EqualTo(run.Status));
            Assert.That(applicationSnapshot.CurrentTime, Is.EqualTo(run.CurrentTime));
            Assert.That(applicationSnapshot.Player.PlayerId, Is.EqualTo(run.PlayerId));
            Assert.That(applicationSnapshot.Enemies.Count, Is.EqualTo(1));
            Assert.That(applicationSnapshot.Enemies[0].EnemyId, Is.EqualTo(EnemyId.FromValue(10UL)));
            Assert.That(applicationSnapshot.Wave.Number, Is.EqualTo(run.CurrentWaveNumber));
        }

        [Test]
        public void MapPreservesEnemyOrdering()
        {
            ArenaRun run = _kit.StartStandartRun();
            _kit.Spawn(run, 30UL, new Position3D(22f, 0f, 0f));
            _kit.Spawn(run, 10UL, new Position3D(-22f, 0f, 0f));

            ArenaRunSnapshot applicationSnapshot =
                _mapper.Map(run.CreateSnapshot());

            Assert.That(
                applicationSnapshot.Enemies[0].EnemyId,
                Is.EqualTo(EnemyId.FromValue(10UL)));
            Assert.That(
                applicationSnapshot.Enemies[1].EnemyId,
                Is.EqualTo(EnemyId.FromValue(30UL)));
        }

        [Test]
        public void MapRejectsNullSource()
        {
            Assert.Throws<ArgumentNullException>(
                () =>
                {
                    _mapper.Map(null);
                });
        }

        [Test]
        public void ApplicationEnemyCollectionCannotBeModified()
        {
            ArenaRun run = _kit.StartStandartRun();
            _kit.Spawn(run, 10UL, new Position3D(22f, 0f, 0f));

            ArenaRunSnapshot snapshot =
                _mapper.Map(run.CreateSnapshot());

            Assert.Throws<NotSupportedException>(
                () =>
                {
                    ((IList<EnemySnapshot>)snapshot.Enemies).Clear();
                });
        }

        [Test]
        public void ApplicationSnapshotDoesNotChangeAfterAggregateMutation()
        {
            ArenaRun run = _kit.StartStandartRun();
            _kit.Spawn(run, 10UL, new Position3D(22f, 0f, 0f));

            ArenaRunSnapshot snapshot = _mapper.Map(run.CreateSnapshot());

            int enemyCount = snapshot.Enemies.Count;
            AggregateRevision revision = snapshot.Revision;

            _kit.Spawn(run, 11UL, new Position3D(-22f, 0f, 0f));

            Assert.That(snapshot.Enemies.Count, Is.EqualTo(enemyCount));
            Assert.That(snapshot.Revision, Is.EqualTo(revision));
        }
    }
}
