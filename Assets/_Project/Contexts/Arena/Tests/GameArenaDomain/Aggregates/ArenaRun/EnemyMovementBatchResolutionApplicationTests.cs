using Game.Arena.Domain.Aggregates.ArenaRun;
using Game.Arena.Domain.Concurrency;
using Game.Arena.Domain.Geometry;
using Game.Arena.Domain.Identity;
using Game.Arena.Domain.Interactions;
using Game.Arena.Domain.Interactions.Movement;
using Game.Arena.Domain.Interactions.Spawn;
using Game.Arena.Domain.Tests.Support;
using Game.Arena.Domain.Time;
using NUnit.Framework;

namespace Game.Arena.Domain.Tests.Aggregates._ArenaRun
{
    [TestFixture]
    public sealed class EnemyMovementBatchResolutionApplicationTests
    {
        private static readonly Position3D s_eastFar = new(5f, 0f, 0f);
        private static readonly Position3D s_westFar = new(-5f, 0f, 0f);

        private ArenaRunTestKit _kit;

        [SetUp]
        public void SetUp()
        {
            _kit = new ArenaRunTestKit();
        }

        [Test]
        public void EnemyMovementBatchResolutionMovesAllEnemiesAtomically()
        {
            ArenaRun run = _kit.StartRunInCompactArena();
            EnemyId east = Spawn(run, 10UL, s_eastFar);
            EnemyId west = Spawn(run, 11UL, s_westFar);
            EnemyMovementBatchRequest request = run.RequestEnemyMovementBatch(new GameDuration(1d)).Request;

            EnemyMovementBatchResolutionOutcome outcome = run.ApplyEnemyMovementBatch(
                new EnemyMovementBatchResolution(
                    request.Correlation,
                    new[]
                    {
                        Entry(east, new Position3D(3f, 0f, 0f)),
                        Entry(west, new Position3D(-3f, 0f, 0f)),
                    }));

            Assert.That(outcome.Status, Is.EqualTo(EnemyMovementBatchResolutionStatus.Applied));
            Assert.That(run.TryGetEnemyPosition(east, out Position3D eastPosition), Is.True);
            Assert.That(run.TryGetEnemyPosition(west, out Position3D westPosition), Is.True);
            Assert.That(eastPosition, Is.EqualTo(new Position3D(3f, 0f, 0f)));
            Assert.That(westPosition, Is.EqualTo(new Position3D(-3f, 0f, 0f)));
            Assert.That(run.HasPendingInteraction, Is.False);
        }

        [Test]
        public void EnemyMovementBatchWithPartialMovementAdvancesRevisionOnce()
        {
            ArenaRun run = _kit.StartRunInCompactArena();
            EnemyId east = Spawn(run, 10UL, s_eastFar);
            EnemyId west = Spawn(run, 11UL, s_westFar);
            EnemyMovementBatchRequest request = run.RequestEnemyMovementBatch(new GameDuration(1d)).Request;
            AggregateRevision revisionBeforeApply = run.Revision;

            EnemyMovementBatchResolutionOutcome outcome = run.ApplyEnemyMovementBatch(
                new EnemyMovementBatchResolution(
                    request.Correlation,
                    new[]
                    {
                        Entry(east, s_eastFar),
                        Entry(west, new Position3D(-3f, 0f, 0f)),
                    }));

            Assert.That(outcome.Status, Is.EqualTo(EnemyMovementBatchResolutionStatus.Applied));
            Assert.That(run.Revision.Value, Is.EqualTo(revisionBeforeApply.Value + 1UL));
            Assert.That(outcome.Change.Revision, Is.EqualTo(run.Revision));
            Assert.That(outcome.Change.HasStateChange, Is.True);
        }

        [Test]
        public void EnemyMovementBatchWithNoActualMovementDoesNotAdvanceRevision()
        {
            ArenaRun run = _kit.StartRunInCompactArena();
            EnemyId east = Spawn(run, 10UL, s_eastFar);
            EnemyMovementBatchRequest request = run.RequestEnemyMovementBatch(new GameDuration(1d)).Request;
            AggregateRevision revisionBeforeApply = run.Revision;

            EnemyMovementBatchResolutionOutcome outcome = run.ApplyEnemyMovementBatch(
                new EnemyMovementBatchResolution(
                    request.Correlation,
                    new[]
                    {
                        Entry(east, s_eastFar),
                    }));

            Assert.That(outcome.Status, Is.EqualTo(EnemyMovementBatchResolutionStatus.AcceptedWithoutStateChange));
            Assert.That(run.Revision, Is.EqualTo(revisionBeforeApply));
            Assert.That(outcome.Change.HasStateChange, Is.False);
            Assert.That(run.HasPendingInteraction, Is.False);
        }

        [Test]
        public void EnemyMovementBatchWithMissingEnemyIdIsRejected()
        {
            ArenaRun run = _kit.StartRunInCompactArena();
            Spawn(run, 10UL, s_eastFar);
            Spawn(run, 11UL, s_westFar);
            EnemyMovementBatchRequest request = run.RequestEnemyMovementBatch(new GameDuration(1d)).Request;

            EnemyMovementBatchResolutionOutcome outcome = run.ApplyEnemyMovementBatch(
                new EnemyMovementBatchResolution(
                    request.Correlation,
                    new[]
                    {
                        Entry(EnemyId.FromValue(10UL), new Position3D(3f, 0f, 0f)),
                    }));

            Assert.That(outcome.RejectionReason, Is.EqualTo(EnemyMovementBatchRejectionReason.MissingEnemyId));
        }

        [Test]
        public void EnemyMovementBatchWithUnexpectedEnemyIdIsRejected()
        {
            ArenaRun run = _kit.StartRunInCompactArena();
            Spawn(run, 10UL, s_eastFar);
            EnemyMovementBatchRequest request = run.RequestEnemyMovementBatch(new GameDuration(1d)).Request;

            EnemyMovementBatchResolutionOutcome outcome = run.ApplyEnemyMovementBatch(
                new EnemyMovementBatchResolution(
                    request.Correlation,
                    new[]
                    {
                        Entry(EnemyId.FromValue(10UL), new Position3D(3f, 0f, 0f)),
                        Entry(EnemyId.FromValue(777UL), new Position3D(0f, 0f, 0f)),
                    }));

            Assert.That(outcome.RejectionReason, Is.EqualTo(EnemyMovementBatchRejectionReason.UnexpectedEnemyId));
        }

        [Test]
        public void EnemyMovementBatchWithDuplicateEnemyIdIsRejected()
        {
            ArenaRun run = _kit.StartRunInCompactArena();
            EnemyId enemyId = Spawn(run, 10UL, s_eastFar);
            EnemyMovementBatchRequest request = run.RequestEnemyMovementBatch(new GameDuration(1d)).Request;

            EnemyMovementBatchResolutionOutcome outcome = run.ApplyEnemyMovementBatch(
                new EnemyMovementBatchResolution(
                    request.Correlation,
                    new[]
                    {
                        Entry(enemyId, new Position3D(4f, 0f, 0f)),
                        Entry(enemyId, new Position3D(3f, 0f, 0f)),
                    }));

            Assert.That(outcome.RejectionReason, Is.EqualTo(EnemyMovementBatchRejectionReason.DuplicateEnemyId));
        }

        [Test]
        public void EnemyMovementBatchWithNoneEnemyIdIsRejected()
        {
            ArenaRun run = _kit.StartRunInCompactArena();
            Spawn(run, 10UL, s_eastFar);
            EnemyMovementBatchRequest request = run.RequestEnemyMovementBatch(new GameDuration(1d)).Request;

            EnemyMovementBatchResolutionOutcome outcome = run.ApplyEnemyMovementBatch(
                new EnemyMovementBatchResolution(
                    request.Correlation,
                    new[]
                    {
                        Entry(EnemyId.None, new Position3D(3f, 0f, 0f)),
                    }));

            Assert.That(outcome.RejectionReason, Is.EqualTo(EnemyMovementBatchRejectionReason.EnemyIdIsNone));
        }

        [Test]
        public void EnemyMovementBatchWithEnemyBeyondRequestedDistanceIsRejected()
        {
            ArenaRun run = _kit.StartRunInCompactArena();
            EnemyId enemyId = Spawn(run, 10UL, s_eastFar);
            EnemyMovementBatchRequest request = run.RequestEnemyMovementBatch(new GameDuration(1d)).Request;

            EnemyMovementBatchResolutionOutcome outcome = run.ApplyEnemyMovementBatch(
                new EnemyMovementBatchResolution(
                    request.Correlation,
                    new[]
                    {
                        Entry(enemyId, new Position3D(2f, 0f, 0f)),
                    }));

            Assert.That(outcome.RejectionReason, Is.EqualTo(EnemyMovementBatchRejectionReason.AcceptedPositionBeyondRequestedDistance));
        }

        [Test]
        public void EnemyMovementBatchWithEnemyBehindRequestedDirectionIsRejected()
        {
            ArenaRun run = _kit.StartRunInCompactArena();
            EnemyId enemyId = Spawn(run, 10UL, s_eastFar);
            EnemyMovementBatchRequest request = run.RequestEnemyMovementBatch(new GameDuration(1d)).Request;

            EnemyMovementBatchResolutionOutcome outcome = run.ApplyEnemyMovementBatch(
                new EnemyMovementBatchResolution(
                    request.Correlation,
                    new[]
                    {
                        Entry(enemyId, new Position3D(6f, 0f, 0f)),
                    }));

            Assert.That(outcome.RejectionReason, Is.EqualTo(EnemyMovementBatchRejectionReason.AcceptedPositionBehindRequest));
        }

        [Test]
        public void EnemyMovementBatchWithEnemyOffPathIsRejected()
        {
            ArenaRun run = _kit.StartRunInCompactArena();
            EnemyId enemyId = Spawn(run, 10UL, s_eastFar);
            EnemyMovementBatchRequest request = run.RequestEnemyMovementBatch(new GameDuration(1d)).Request;

            EnemyMovementBatchResolutionOutcome outcome = run.ApplyEnemyMovementBatch(
                new EnemyMovementBatchResolution(
                    request.Correlation,
                    new[]
                    {
                        Entry(enemyId, new Position3D(4f, 0f, 1f)),
                    }));

            Assert.That(outcome.RejectionReason, Is.EqualTo(EnemyMovementBatchRejectionReason.AcceptedPositionOffMovementPath));
        }

        [Test]
        public void EnemyMovementBatchWithEnemyOffGroundPlaneIsRejected()
        {
            ArenaRun run = _kit.StartRunInCompactArena();
            EnemyId enemyId = Spawn(run, 10UL, s_eastFar);
            EnemyMovementBatchRequest request = run.RequestEnemyMovementBatch(new GameDuration(1d)).Request;

            EnemyMovementBatchResolutionOutcome outcome = run.ApplyEnemyMovementBatch(
                new EnemyMovementBatchResolution(
                    request.Correlation,
                    new[]
                    {
                        Entry(enemyId, new Position3D(4f, 1f, 0f)),
                    }));

            Assert.That(outcome.RejectionReason, Is.EqualTo(EnemyMovementBatchRejectionReason.AcceptedPositionOffGroundPlane));
        }

        [Test]
        public void RejectedEnemyMovementBatchDoesNotMoveAnyEnemy()
        {
            ArenaRun run = _kit.StartRunInCompactArena();
            EnemyId east = Spawn(run, 10UL, s_eastFar);
            EnemyId west = Spawn(run, 11UL, s_westFar);
            EnemyMovementBatchRequest request = run.RequestEnemyMovementBatch(new GameDuration(1d)).Request;

            EnemyMovementBatchResolutionOutcome outcome = run.ApplyEnemyMovementBatch(
                new EnemyMovementBatchResolution(
                    request.Correlation,
                    new[]
                    {
                        Entry(east, new Position3D(3f, 0f, 0f)),
                        Entry(west, new Position3D(-10f, 0f, 0f)),
                    }));

            run.TryGetEnemyPosition(east, out Position3D eastPosition);
            run.TryGetEnemyPosition(west, out Position3D westPosition);

            Assert.That(outcome.Status, Is.EqualTo(EnemyMovementBatchResolutionStatus.Rejected));
            Assert.That(eastPosition, Is.EqualTo(s_eastFar));
            Assert.That(westPosition, Is.EqualTo(s_westFar));
        }

        [Test]
        public void RejectedEnemyMovementBatchKeepsPendingInteractionOpen()
        {
            ArenaRun run = _kit.StartRunInCompactArena();
            EnemyId enemyId = Spawn(run, 10UL, s_eastFar);
            EnemyMovementBatchRequest request = run.RequestEnemyMovementBatch(new GameDuration(1d)).Request;

            run.ApplyEnemyMovementBatch(
                new EnemyMovementBatchResolution(
                    request.Correlation,
                    new[]
                    {
                        Entry(enemyId, new Position3D(2f, 0f, 0f)),
                    }));

            Assert.That(run.HasPendingInteraction, Is.True);
        }

        [Test]
        public void ValidEnemyMovementBatchCanBeAppliedAfterRejectedResolution()
        {
            ArenaRun run = _kit.StartRunInCompactArena();
            EnemyId enemyId = Spawn(run, 10UL, s_eastFar);
            EnemyMovementBatchRequest request = run.RequestEnemyMovementBatch(new GameDuration(1d)).Request;

            run.ApplyEnemyMovementBatch(
                new EnemyMovementBatchResolution(
                    request.Correlation,
                    new[]
                    {
                        Entry(enemyId, new Position3D(2f, 0f, 0f)),
                    }));

            EnemyMovementBatchResolutionOutcome outcome = run.ApplyEnemyMovementBatch(
                new EnemyMovementBatchResolution(
                    request.Correlation,
                    new[]
                    {
                        Entry(enemyId, new Position3D(3f, 0f, 0f)),
                    }));

            Assert.That(outcome.Status, Is.EqualTo(EnemyMovementBatchResolutionStatus.Applied));
            Assert.That(run.TryGetEnemyPosition(enemyId, out Position3D position), Is.True);
            Assert.That(position, Is.EqualTo(new Position3D(3f, 0f, 0f)));
        }

        [Test]
        public void EnemyMovementBatchDoesNotProduceDomainEvents()
        {
            ArenaRun run = _kit.StartRunInCompactArena();
            EnemyId enemyId = Spawn(run, 10UL, s_eastFar);
            EnemyMovementBatchRequest request = run.RequestEnemyMovementBatch(new GameDuration(1d)).Request;

            EnemyMovementBatchResolutionOutcome outcome = run.ApplyEnemyMovementBatch(
                new EnemyMovementBatchResolution(
                    request.Correlation,
                    new[]
                    {
                        Entry(enemyId, new Position3D(3f, 0f, 0f)),
                    }));

            Assert.That(outcome.Change.DomainEvents, Is.Empty);
        }

        [Test]
        public void DuplicateEnemyMovementBatchResolutionIsRejected()
        {
            ArenaRun run = _kit.StartRunInCompactArena();
            EnemyId enemyId = Spawn(run, 10UL, s_eastFar);
            EnemyMovementBatchRequest request = run.RequestEnemyMovementBatch(new GameDuration(1d)).Request;

            EnemyMovementBatchResolution resolution = new(
                request.Correlation,
                new[]
                {
                    Entry(enemyId, new Position3D(3f, 0f, 0f)),
                });

            run.ApplyEnemyMovementBatch(resolution);
            AggregateRevision revisionAfterApply = run.Revision;

            EnemyMovementBatchResolutionOutcome duplicate = run.ApplyEnemyMovementBatch(resolution);

            Assert.That(duplicate.RejectionReason, Is.EqualTo(EnemyMovementBatchRejectionReason.InteractionClosed));
            Assert.That(run.Revision, Is.EqualTo(revisionAfterApply));
        }

        [Test]
        public void EnemyMovementBatchResolutionForAnotherArenaRunIsRejected()
        {
            ArenaRun run = _kit.StartRunInCompactArena();
            EnemyId enemyId = Spawn(run, 10UL, s_eastFar);
            EnemyMovementBatchRequest request = run.RequestEnemyMovementBatch(new GameDuration(1d)).Request;

            InteractionCorrelation foreign = new(
                ArenaRunId.FromValue(999UL),
                request.Correlation.InteractionId,
                request.Correlation.AggregateRevision);

            EnemyMovementBatchResolutionOutcome outcome = run.ApplyEnemyMovementBatch(
                new EnemyMovementBatchResolution(
                    foreign,
                    new[]
                    {
                        Entry(enemyId, new Position3D(3f, 0f, 0f)),
                    }));

            Assert.That(outcome.RejectionReason, Is.EqualTo(EnemyMovementBatchRejectionReason.ForeignArenaRun));
            Assert.That(run.HasPendingInteraction, Is.True);
        }

        [Test]
        public void TryGetEnemyPositionReturnsCurrentActiveEnemyPosition()
        {
            ArenaRun run = _kit.StartRunInCompactArena();
            EnemyId enemyId = Spawn(run, 10UL, s_eastFar);
            EnemyMovementBatchRequest request = run.RequestEnemyMovementBatch(new GameDuration(1d)).Request;

            run.ApplyEnemyMovementBatch(
                new EnemyMovementBatchResolution(
                    request.Correlation,
                    new[]
                    {
                        Entry(enemyId, new Position3D(3f, 0f, 0f)),
                    }));

            bool found = run.TryGetEnemyPosition(enemyId, out Position3D position);

            Assert.That(found, Is.True);
            Assert.That(position, Is.EqualTo(new Position3D(3f, 0f, 0f)));
        }

        [Test]
        public void TryGetEnemyPositionReturnsFalseForUnknownEnemy()
        {
            ArenaRun run = _kit.StartRunInCompactArena();

            bool found = run.TryGetEnemyPosition(EnemyId.FromValue(777UL), out Position3D position);

            Assert.That(found, Is.False);
        }

        private EnemyMovementBatchResolutionEntry Entry(
            EnemyId enemyId,
            Position3D position)
        {
            return new EnemyMovementBatchResolutionEntry(enemyId, position);
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
