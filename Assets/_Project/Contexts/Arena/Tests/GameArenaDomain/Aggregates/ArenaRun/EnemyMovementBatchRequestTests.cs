using System;
using Game.Arena.Domain.Aggregates.ArenaRun;
using Game.Arena.Domain.Concurrency;
using Game.Arena.Domain.Geometry;
using Game.Arena.Domain.Identity;
using Game.Arena.Domain.Interactions.Movement;
using Game.Arena.Domain.Interactions.Spawn;
using Game.Arena.Domain.Tests.Support;
using Game.Arena.Domain.Time;
using NUnit.Framework;

namespace Game.Arena.Domain.Tests.Aggregates._ArenaRun
{
    [TestFixture]
    public sealed class EnemyMovementBatchRequestTests
    {
        private static readonly Position3D s_inAttackRange = new(2f, 0f, 0f);
        private static readonly Position3D s_outsideAttackRange = new(5f, 0f, 0f);
        private static readonly Position3D s_outsideAttackRangeWest = new(-5f, 0f, 0f);

        private ArenaRunTestKit _kit;

        [SetUp]
        public void SetUp()
        {
            _kit = new ArenaRunTestKit();
        }

        [Test]
        public void EnemyOutsideAttackRangeReceivesMovementIntent()
        {
            ArenaRun run = _kit.StartRunInCompactArena();
            EnemyId enemyId = Spawn(run, 10UL, s_outsideAttackRange);

            EnemyMovementBatchRequestOutcome outcome = run.RequestEnemyMovementBatch(
                new GameDuration(1d));

            Assert.That(outcome.HasRequest, Is.True);
            Assert.That(outcome.Request.Intents.Count, Is.EqualTo(1));
            Assert.That(outcome.Request.Intents[0].EnemyId, Is.EqualTo(enemyId));
            Assert.That(outcome.Request.Intents[0].Intent.From, Is.EqualTo(s_outsideAttackRange));
            Assert.That(outcome.Request.Intents[0].Intent.Direction, Is.EqualTo(Direction3D.Left));
            Assert.That(outcome.Request.Intents[0].Intent.RequestedDistance.Value, Is.EqualTo(2f));
        }

        [Test]
        public void EnemyInsideAttackRangeDoesNotReceiveMovementIntent()
        {
            ArenaRun run = _kit.StartRunInCompactArena();
            Spawn(run, 10UL, s_inAttackRange);

            EnemyMovementBatchRequestOutcome outcome = run.RequestEnemyMovementBatch(
                new GameDuration(1d));

            Assert.That(outcome.Status, Is.EqualTo(EnemyMovementBatchRequestStatus.NoEligibleEnemies));
            Assert.That(outcome.HasRequest, Is.False);
            Assert.That(run.HasPendingInteraction, Is.False);
        }

        [Test]
        public void EnemyWithPendingAttackDoesNotReceiveMovementIntent()
        {
            ArenaRun run = _kit.StartRunInCompactArena();
            Spawn(run, 10UL, s_inAttackRange);
            run.StartEligibleEnemyAttacks();

            EnemyMovementBatchRequestOutcome outcome = run.RequestEnemyMovementBatch(
                new GameDuration(1d));

            Assert.That(outcome.Status, Is.EqualTo(EnemyMovementBatchRequestStatus.NoEligibleEnemies));
            Assert.That(run.PendingEnemyAttackCount, Is.EqualTo(1));
            Assert.That(run.HasPendingInteraction, Is.False);
        }

        [Test]
        public void OnlyEligibleEnemiesAreIncluded()
        {
            ArenaRun run = _kit.StartRunInCompactArena();
            EnemyId farEnemy = Spawn(run, 10UL, s_outsideAttackRange);
            Spawn(run, 11UL, s_inAttackRange);

            EnemyMovementBatchRequestOutcome outcome = run.RequestEnemyMovementBatch(
                new GameDuration(1d));

            Assert.That(outcome.HasRequest, Is.True);
            Assert.That(outcome.Request.Intents.Count, Is.EqualTo(1));
            Assert.That(outcome.Request.Intents[0].EnemyId, Is.EqualTo(farEnemy));
        }

        [Test]
        public void EnemyMovementIntentsAreOrderedByEnemyId()
        {
            ArenaRun run = _kit.StartRunInCompactArena();
            Spawn(run, 30UL, s_outsideAttackRange);
            Spawn(run, 20UL, s_outsideAttackRangeWest);

            EnemyMovementBatchRequestOutcome outcome = run.RequestEnemyMovementBatch(
                new GameDuration(1d));

            Assert.That(outcome.HasRequest, Is.True);
            Assert.That(outcome.Request.Intents.Count, Is.EqualTo(2));
            Assert.That(outcome.Request.Intents[0].EnemyId, Is.EqualTo(EnemyId.FromValue(20UL)));
            Assert.That(outcome.Request.Intents[1].EnemyId, Is.EqualTo(EnemyId.FromValue(30UL)));
        }

        [Test]
        public void EnemyMovementBatchRequestDoesNotAdvanceRevision()
        {
            ArenaRun run = _kit.StartRunInCompactArena();
            Spawn(run, 10UL, s_outsideAttackRange);
            AggregateRevision revisionBeforeRequest = run.Revision;

            EnemyMovementBatchRequestOutcome outcome = run.RequestEnemyMovementBatch(
                new GameDuration(1d));

            Assert.That(outcome.HasRequest, Is.True);
            Assert.That(run.Revision, Is.EqualTo(revisionBeforeRequest));
            Assert.That(outcome.Request.Correlation.AggregateRevision, Is.EqualTo(revisionBeforeRequest));
        }

        [Test]
        public void EnemyMovementBatchRequestOpensOnePendingInteraction()
        {
            ArenaRun run = _kit.StartRunInCompactArena();
            Spawn(run, 10UL, s_outsideAttackRange);

            EnemyMovementBatchRequestOutcome outcome = run.RequestEnemyMovementBatch(
                new GameDuration(1d));

            Assert.That(outcome.HasRequest, Is.True);
            Assert.That(run.HasPendingInteraction, Is.True);
            Assert.That(outcome.Request.Correlation.InteractionId.Value, Is.EqualTo(2UL));
        }

        [Test]
        public void EnemyMovementBatchRequestUsesEnemySpeedAndDuration()
        {
            ArenaRun run = _kit.StartRunInCompactArena();
            Spawn(run, 10UL, s_outsideAttackRange);

            EnemyMovementBatchRequestOutcome outcome = run.RequestEnemyMovementBatch(
                new GameDuration(0.5d));

            Assert.That(outcome.Request.Intents[0].Intent.RequestedDistance.Value, Is.EqualTo(1f));
        }

        [Test]
        public void ZeroDurationDoesNotOpenInteraction()
        {
            ArenaRun run = _kit.StartRunInCompactArena();
            Spawn(run, 10UL, s_outsideAttackRange);

            EnemyMovementBatchRequestOutcome outcome = run.RequestEnemyMovementBatch(
                new GameDuration(0d));

            Assert.That(outcome.Status, Is.EqualTo(EnemyMovementBatchRequestStatus.NoEligibleEnemies));
            Assert.That(run.HasPendingInteraction, Is.False);
        }

        [Test]
        public void EnemyMovementBatchRequestWhileInteractionPendingThrows()
        {
            ArenaRun run = _kit.StartRunInCompactArena();
            Spawn(run, 10UL, s_outsideAttackRange);

            run.RequestEnemyMovementBatch(new GameDuration(1d));

            Assert.Throws<InvalidOperationException>(
                () =>
                {
                    run.RequestEnemyMovementBatch(new GameDuration(1d));
                });
        }

        [Test]
        public void EnemyMovementBatchRequestWhileSpawnInteractionPendingThrows()
        {
            ArenaRun run = _kit.StartRunInCompactArena();

            run.RequestEnemySpawn();

            Assert.Throws<InvalidOperationException>(
                () =>
                {
                    run.RequestEnemyMovementBatch(new GameDuration(1d));
                });
        }

        [Test]
        public void EnemyMovementBatchRequestHasCurrentAggregateCorrelation()
        {
            ArenaRun run = _kit.StartRunInCompactArena();
            Spawn(run, 10UL, s_outsideAttackRange);
            run.AdvanceTime(new GameDuration(1d));
            AggregateRevision expectedRevision = run.Revision;

            EnemyMovementBatchRequestOutcome outcome = run.RequestEnemyMovementBatch(
                new GameDuration(1d));

            Assert.That(outcome.Request.Correlation.ArenaRunId, Is.EqualTo(run.Id));
            Assert.That(outcome.Request.Correlation.AggregateRevision, Is.EqualTo(expectedRevision));
        }

        private EnemyId Spawn(ArenaRun run, ulong value, Position3D position)
        {
            EnemySpawnRequest request = run.RequestEnemySpawn().Request;
            EnemyId enemyId = EnemyId.FromValue(value);

            run.ApplyEnemySpawn(new EnemySpawnResolution(
                request.Correlation,
                enemyId,
                position));

            return enemyId;
        }
    }
}
