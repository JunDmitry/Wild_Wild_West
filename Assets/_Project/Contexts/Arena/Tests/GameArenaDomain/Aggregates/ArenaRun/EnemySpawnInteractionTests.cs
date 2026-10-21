using System;
using Game.Arena.Domain.Aggregates.ArenaRun;
using Game.Arena.Domain.Combat;
using Game.Arena.Domain.Concurrency;
using Game.Arena.Domain.Events;
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
    public sealed class EnemySpawnInteractionTests
    {
        private static readonly Position3D s_outsideEast = new(22f, 0f, 0f);

        private ArenaRunTestKit _kit;

        [SetUp]
        public void SetUp()
        {
            _kit = new ArenaRunTestKit();
        }

        [Test]
        public void SpawnRequestIsRegularWhileRegularEnemiesRemain()
        {
            ArenaRun run = _kit.StartRun();

            EnemySpawnRequestOutcome outcome = run.RequestEnemySpawn();

            Assert.That(outcome.HasRequest, Is.True);
            Assert.That(outcome.Request.EnemyKind, Is.EqualTo(EnemyKind.Regular));
            Assert.That(outcome.Request.Correlation.InteractionId.Value, Is.EqualTo(1UL));
            Assert.That(run.Revision, Is.EqualTo(AggregateRevision.Initial));
        }

        [Test]
        public void SpawnResolutionAddsEnemyAndRaisesEnemySpawned()
        {
            ArenaRun run = _kit.StartRun();
            EnemySpawnRequest request = run.RequestEnemySpawn().Request;
            EnemyId enemyId = EnemyId.FromValue(10UL);

            EnemySpawnResolutionOutcome outcome = run.ApplyEnemySpawn(
                new EnemySpawnResolution(request.Correlation, enemyId, s_outsideEast));

            Assert.That(outcome.IsSpawned, Is.True);
            Assert.That(run.ContainsEnemy(enemyId), Is.True);
            Assert.That(run.ActiveEnemyCount, Is.EqualTo(1));
            Assert.That(run.RegularEnemiesRemainingToSpawn, Is.EqualTo(1));
            Assert.That(run.Revision.Value, Is.EqualTo(1UL));
            Assert.That(outcome.Change.DomainEvents.Count, Is.EqualTo(1));

            EnemySpawned spawned = (EnemySpawned)outcome.Change.DomainEvents[0];

            Assert.That(spawned.EnemyId, Is.EqualTo(enemyId));
            Assert.That(spawned.EnemyKind, Is.EqualTo(EnemyKind.Regular));
            Assert.That(spawned.Position, Is.EqualTo(s_outsideEast));
            Assert.That(spawned.AggregateRevision, Is.EqualTo(run.Revision));
        }

        [Test]
        public void NoSpawnIsDueAfterAllRegularEnemiesAreSpawnedWhileTheyAreAlive()
        {
            ArenaRun run = _kit.StartRun(Position3D.Zero, _kit.Waves(1));
            SpawnRegular(run, 10UL);

            EnemySpawnRequestOutcome outcome = run.RequestEnemySpawn();

            Assert.That(outcome.Status, Is.EqualTo(EnemySpawnRequestStatus.NoSpawnDue));
            Assert.That(run.CurrentWavePhase, Is.EqualTo(WavePhase.RegularCombat));
            Assert.That(run.HasPendingInteraction, Is.False);
        }

        [Test]
        public void WaveWithoutRegularEnemiesStartsInBossCombatAndRequestsBoss()
        {
            ArenaRun run = _kit.StartRun(Position3D.Zero, _kit.Waves(0));

            EnemySpawnRequestOutcome outcome = run.RequestEnemySpawn();

            Assert.That(run.CurrentWavePhase, Is.EqualTo(WavePhase.BossCombat));
            Assert.That(outcome.Request.EnemyKind, Is.EqualTo(EnemyKind.Boss));
            Assert.That(outcome.Request.CollisionRadius, Is.EqualTo(CollisionRadius.FromValue(1f)));
        }

        [Test]
        public void BossSpawnsOnceThenNoSpawnIsDue()
        {
            ArenaRun run = _kit.StartRun(Position3D.Zero, _kit.Waves(0));
            EnemySpawnRequest request = run.RequestEnemySpawn().Request;

            run.ApplyEnemySpawn(new EnemySpawnResolution(request.Correlation, EnemyId.FromValue(10UL), s_outsideEast));
            EnemySpawnRequestOutcome second = run.RequestEnemySpawn();

            Assert.That(run.CurrentBossStatus, Is.EqualTo(BossStatus.Alive));
            Assert.That(second.Status, Is.EqualTo(EnemySpawnRequestStatus.NoSpawnDue));
        }

        [Test]
        public void SpawnResolutionWithNoneEnemyIdIsRejectedWithoutSideEffects()
        {
            ArenaRun run = _kit.StartRun();
            EnemySpawnRequest request = run.RequestEnemySpawn().Request;

            EnemySpawnResolutionOutcome outcome = run.ApplyEnemySpawn(
                new EnemySpawnResolution(request.Correlation, EnemyId.None, s_outsideEast));

            Assert.That(outcome.RejectionReason, Is.EqualTo(EnemySpawnResolutionRejectionReason.EnemyIdIsNone));
            Assert.That(run.ActiveEnemyCount, Is.EqualTo(0));
            Assert.That(run.Revision, Is.EqualTo(AggregateRevision.Initial));
            Assert.That(run.HasPendingInteraction, Is.True);
        }

        [Test]
        public void DuplicateEnemyIdIsRejected()
        {
            ArenaRun run = _kit.StartRun();
            SpawnRegular(run, 10UL);
            EnemySpawnRequest request = run.RequestEnemySpawn().Request;

            EnemySpawnResolutionOutcome outcome = run.ApplyEnemySpawn(
                new EnemySpawnResolution(request.Correlation, EnemyId.FromValue(10UL), s_outsideEast));

            Assert.That(outcome.RejectionReason, Is.EqualTo(EnemySpawnResolutionRejectionReason.DuplicateEnemyId));
            Assert.That(run.ActiveEnemyCount, Is.EqualTo(1));
        }

        [Test]
        public void SpawnPositionInsideArenaIsRejected()
        {
            ArenaRun run = _kit.StartRun();
            EnemySpawnRequest request = run.RequestEnemySpawn().Request;

            EnemySpawnResolutionOutcome outcome = run.ApplyEnemySpawn(
                new EnemySpawnResolution(request.Correlation, EnemyId.FromValue(10UL), new Position3D(5f, 0f, 5f)));

            Assert.That(
                outcome.RejectionReason,
                Is.EqualTo(EnemySpawnResolutionRejectionReason.SpawnPositionOutsidePerimeter));
        }

        [Test]
        public void SpawnPositionBeyondSpawnBandIsRejected()
        {
            ArenaRun run = _kit.StartRun();
            EnemySpawnRequest request = run.RequestEnemySpawn().Request;

            EnemySpawnResolutionOutcome outcome = run.ApplyEnemySpawn(
                new EnemySpawnResolution(request.Correlation, EnemyId.FromValue(10UL), new Position3D(30f, 0f, 0f)));

            Assert.That(
                outcome.RejectionReason,
                Is.EqualTo(EnemySpawnResolutionRejectionReason.SpawnPositionOutsidePerimeter));
        }

        [Test]
        public void SpawnPositionAtWrongHeightIsRejected()
        {
            ArenaRun run = _kit.StartRun();
            EnemySpawnRequest request = run.RequestEnemySpawn().Request;

            EnemySpawnResolutionOutcome outcome = run.ApplyEnemySpawn(
                new EnemySpawnResolution(request.Correlation, EnemyId.FromValue(10UL), new Position3D(22f, 1f, 0f)));

            Assert.That(
                outcome.RejectionReason,
                Is.EqualTo(EnemySpawnResolutionRejectionReason.SpawnPositionOutsidePerimeter));
        }

        [Test]
        public void MovementResolutionIsRejectedForPendingSpawnInteraction()
        {
            ArenaRun run = _kit.StartRun();
            EnemySpawnRequest request = run.RequestEnemySpawn().Request;

            PlayerMovementResolutionOutcome outcome = run.ApplyPlayerMovement(
                new PlayerMovementResolution(request.Correlation, new Position3D(1f, 0f, 0f)));

            Assert.That(outcome.RejectionReason, Is.EqualTo(PlayerMovementResolutionRejectionReason.KindMismatch));
            Assert.That(run.HasPendingInteraction, Is.True);
        }

        [Test]
        public void MovementRequestWhileSpawnPendingThrows()
        {
            ArenaRun run = _kit.StartRun();
            run.RequestEnemySpawn();
            MovementInput input = MovementInput.FromVector(new Displacement3D(1f, 0f, 0f));

            Assert.Throws<InvalidOperationException>(
                () =>
                {
                    run.RequestPlayerMovement(input, new GameDuration(1d));
                });
        }

        [Test]
        public void SpawnRequestWhileMovementPendingThrows()
        {
            ArenaRun run = _kit.StartRun();
            MovementInput input = MovementInput.FromVector(new Displacement3D(1f, 0f, 0f));
            run.RequestPlayerMovement(input, new GameDuration(1d));

            Assert.Throws<InvalidOperationException>(
                () =>
                {
                    run.RequestEnemySpawn();
                });
        }

        [Test]
        public void CancelledSpawnInteractionCanBeRequestedAgain()
        {
            ArenaRun run = _kit.StartRun();
            EnemySpawnRequest request = run.RequestEnemySpawn().Request;

            run.CancelPendingInteraction(request.Correlation, InteractionCancellationReason.ExternalResolutionTimeout);
            EnemySpawnRequestOutcome outcome = run.RequestEnemySpawn();

            Assert.That(outcome.HasRequest, Is.True);
            Assert.That(outcome.Request.Correlation.InteractionId.Value, Is.EqualTo(2UL));
            Assert.That(run.RegularEnemiesRemainingToSpawn, Is.EqualTo(2));
        }

        private void SpawnRegular(ArenaRun run, ulong enemyId)
        {
            EnemySpawnRequest request = run.RequestEnemySpawn().Request;

            run.ApplyEnemySpawn(new EnemySpawnResolution(request.Correlation, EnemyId.FromValue(enemyId), s_outsideEast));
        }
    }
}
