using System;
using System.Collections.Generic;
using Game.Arena.Domain.Aggregates.ArenaRun;
using Game.Arena.Domain.Combat;
using Game.Arena.Domain.Concurrency;
using Game.Arena.Domain.Configuration;
using Game.Arena.Domain.Geometry;
using Game.Arena.Domain.Identity;
using Game.Arena.Domain.Interactions.Movement;
using Game.Arena.Domain.Tests.Support;
using Game.Arena.Domain.Time;
using NUnit.Framework;

namespace Game.Arena.Domain.Tests.Aggregates._ArenaRun
{
    [TestFixture]
    public sealed class ArenaRunSnapshotTests
    {
        private ArenaRunTestKit _kit;

        [SetUp]
        public void SetUp()
        {
            _kit = new ArenaRunTestKit();
        }

        [Test]
        public void SnapshotContainsAggregateIdentityAndRevision()
        {
            ArenaRun run = _kit.StartRun();

            ArenaRunStateSnapshot snapshot = run.CreateSnapshot();

            Assert.That(snapshot.ArenaRunId, Is.EqualTo(run.Id));
            Assert.That(snapshot.Revision, Is.EqualTo(run.Revision));
            Assert.That(snapshot.Status, Is.EqualTo(ArenaRunStatus.Playing));
            Assert.That(snapshot.CurrentTime, Is.EqualTo(run.CurrentTime));
        }

        [Test]
        public void SnapshotContainsPlayerState()
        {
            ArenaRun run = _kit.StartRun();
            run.SwitchWeapon();

            ArenaRunStateSnapshot snapshot = run.CreateSnapshot();

            Assert.That(snapshot.Player.PlayerId, Is.EqualTo(run.PlayerId));
            Assert.That(snapshot.Player.Position, Is.EqualTo(run.PlayerPosition));
            Assert.That(snapshot.Player.Health, Is.EqualTo(run.PlayerHealth));
            Assert.That(
                snapshot.Player.SelectedWeapon,
                Is.EqualTo(WeaponKind.Melee));
            Assert.That(
                snapshot.Player.RangedReadyAt,
                Is.EqualTo(run.WeaponReadyAt(WeaponKind.Ranged)));
            Assert.That(
                snapshot.Player.MeleeReadyAt,
                Is.EqualTo(run.WeaponReadyAt(WeaponKind.Melee)));
        }

        [Test]
        public void SnapshotContainsWaveState()
        {
            ArenaRun run = _kit.StartRun();

            ArenaRunStateSnapshot snapshot = run.CreateSnapshot();

            Assert.That(snapshot.Wave.Number, Is.EqualTo(run.CurrentWaveNumber));
            Assert.That(snapshot.Wave.Phase, Is.EqualTo(run.CurrentWavePhase));
            Assert.That(
                snapshot.Wave.RegularEnemiesRemainingToSpawn,
                Is.EqualTo(run.RegularEnemiesRemainingToSpawn));
            Assert.That(
                snapshot.Wave.BossStatus,
                Is.EqualTo(run.CurrentBossStatus));
        }

        [Test]
        public void SnapshotContainsEnemiesInAscendingEnemyIdOrder()
        {
            ArenaRun run = _kit.StartRun(Position3D.Zero, new(new[] { new WaveDefinition(WaveNumber.First, 3) }));
            _kit.Spawn(run, 30UL, new Position3D(22f, 0f, 0f));
            _kit.Spawn(run, 10UL, new Position3D(-22f, 0f, 0f));
            _kit.Spawn(run, 20UL, new Position3D(0f, 0f, 22f));

            ArenaRunStateSnapshot snapshot = run.CreateSnapshot();

            Assert.That(snapshot.Enemies.Count, Is.EqualTo(3));
            Assert.That(
                snapshot.Enemies[0].EnemyId,
                Is.EqualTo(EnemyId.FromValue(10UL)));
            Assert.That(
                snapshot.Enemies[1].EnemyId,
                Is.EqualTo(EnemyId.FromValue(20UL)));
            Assert.That(
                snapshot.Enemies[2].EnemyId,
                Is.EqualTo(EnemyId.FromValue(30UL)));
        }

        [Test]
        public void SnapshotDoesNotChangeAfterAggregateMutation()
        {
            ArenaRun run = _kit.StartRun();
            EnemyId enemyId =
                _kit.Spawn(run, 10UL, new Position3D(22f, 0f, 0f));

            ArenaRunStateSnapshot snapshotBeforeMutation = run.CreateSnapshot();
            Position3D enemyPositionBeforeMutation =
                snapshotBeforeMutation.Enemies[0].Position;
            AggregateRevision revisionBeforeMutation =
                snapshotBeforeMutation.Revision;

            EnemyMovementBatchRequest request =
                run.RequestEnemyMovementBatch(new GameDuration(1d)).Request;

            run.ApplyEnemyMovementBatch(
                new EnemyMovementBatchResolution(
                    request.Correlation,
                    new[]
                    {
                        new EnemyMovementBatchResolutionEntry(
                            enemyId,
                            new Position3D(20f, 0f, 0f)),
                    }));

            Assert.That(
                snapshotBeforeMutation.Revision,
                Is.EqualTo(revisionBeforeMutation));
            Assert.That(
                snapshotBeforeMutation.Enemies[0].Position,
                Is.EqualTo(enemyPositionBeforeMutation));
            Assert.That(
                run.Revision,
                Is.Not.EqualTo(snapshotBeforeMutation.Revision));
        }

        [Test]
        public void SnapshotContainsPendingPlayerAttack()
        {
            ArenaRun run = _kit.StartRun();

            run.StartPlayerAttack();

            ArenaRunStateSnapshot snapshot = run.CreateSnapshot();

            Assert.That(snapshot.Player.HasPendingAttack, Is.True);
            Assert.That(snapshot.Player.PendingAttack.Id.IsNone, Is.False);
            Assert.That(
                snapshot.Player.PendingAttack,
                Is.EqualTo(run.CreateSnapshot().Player.PendingAttack));
        }

        [Test]
        public void SnapshotContainsPendingEnemyAttack()
        {
            ArenaRun run = _kit.StartRunInCompactArena();
            _kit.Spawn(run, 10UL, new Position3D(2f, 0f, 0f));

            run.StartEligibleEnemyAttacks();

            ArenaRunStateSnapshot snapshot = run.CreateSnapshot();

            Assert.That(snapshot.Enemies.Count, Is.EqualTo(1));
            Assert.That(snapshot.Enemies[0].HasPendingAttack, Is.True);
            Assert.That(snapshot.Enemies[0].PendingAttack.Id.IsNone, Is.False);
        }

        [Test]
        public void TerminalRunSnapshotPreservesTerminalStatus()
        {
            ArenaRun run = _kit.StartCompactRunWithPlayerHealth(5, 1);
            _kit.Spawn(run, 10UL, new Position3D(2f, 0f, 0f));
            run.StartEligibleEnemyAttacks();
            run.AdvanceTime(new GameDuration(0.2d));
            run.ResolveDueEnemyAttackImpacts();

            ArenaRunStateSnapshot snapshot = run.CreateSnapshot();

            Assert.That(snapshot.Status, Is.EqualTo(ArenaRunStatus.Defeat));
            Assert.That(snapshot.ArenaRunId, Is.EqualTo(run.Id));
            Assert.That(snapshot.Revision, Is.EqualTo(run.Revision));
        }

        [Test]
        public void SnapshotEnemyCollectionCannotBeModified()
        {
            ArenaRun run = _kit.StartRun();
            _kit.Spawn(run, 10UL, new Position3D(22f, 0f, 0f));

            ArenaRunStateSnapshot snapshot = run.CreateSnapshot();

            Assert.Throws<NotSupportedException>(
                () =>
                {
                    ((IList<EnemyStateSnapshot>)snapshot.Enemies).Clear();
                });
        }

        [Test]
        public void SnapshotEnemyCollectionDoesNotShareAggregateCollection()
        {
            ArenaRun run = _kit.StartRun();
            _kit.Spawn(run, 10UL, new Position3D(22f, 0f, 0f));

            ArenaRunStateSnapshot snapshot = run.CreateSnapshot();

            Assert.That(snapshot.Enemies.Count, Is.EqualTo(1));

            _kit.Spawn(run, 11UL, new Position3D(-22f, 0f, 0f));

            Assert.That(snapshot.Enemies.Count, Is.EqualTo(1));
            Assert.That(run.ActiveEnemyCount, Is.EqualTo(2));
        }
    }
}
