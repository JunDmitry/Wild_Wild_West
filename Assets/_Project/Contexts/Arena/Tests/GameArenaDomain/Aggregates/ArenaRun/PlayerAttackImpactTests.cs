using System;
using System.Collections.Generic;
using Game.Arena.Domain.Aggregates.ArenaRun;
using Game.Arena.Domain.Combat;
using Game.Arena.Domain.Concurrency;
using Game.Arena.Domain.Events;
using Game.Arena.Domain.Geometry;
using Game.Arena.Domain.Identity;
using Game.Arena.Domain.Interactions.Attack;
using Game.Arena.Domain.Interactions.Spawn;
using Game.Arena.Domain.Tests.Support;
using Game.Arena.Domain.Time;
using NUnit.Framework;

namespace Game.Arena.Domain.Tests.Aggregates._ArenaRun
{
    [TestFixture]
    public sealed class PlayerAttackImpactTests
    {
        private static readonly Position3D s_farSpawn = new (22f, 0f, 0f);
        private static readonly Position3D s_closeSpawn = new (2.5f, 0f, 0f);

        private ArenaRunTestKit _kit;

        [SetUp]
        public void SetUp()
        {
            _kit = new ArenaRunTestKit();
        }

        [Test]
        public void ImpactRequestIsNotDueBeforeImpactTime()
        {
            ArenaRun run = _kit.StartRun();
            run.SwitchWeapon();
            run.StartPlayerAttack();

            PlayerAttackImpactRequestOutcome outcome = run.RequestPlayerAttackImpact(Direction3D.Right);

            Assert.That(outcome.Status, Is.EqualTo(PlayerAttackImpactRequestStatus.ImpactNotDue));
            Assert.That(run.HasPendingInteraction, Is.False);
        }

        [Test]
        public void ImpactRequestWithoutPendingAttackIsRejected()
        {
            ArenaRun run = _kit.StartRun();

            PlayerAttackImpactRequestOutcome outcome = run.RequestPlayerAttackImpact(Direction3D.Right);

            Assert.That(outcome.Status, Is.EqualTo(PlayerAttackImpactRequestStatus.NoPendingAttack));
        }

        [Test]
        public void ImpactRequestDoesNotAdvanceRevision()
        {
            ArenaRun run = _kit.StartRun();
            run.StartPlayerAttack();
            AggregateRevision revisionBeforeRequest = run.Revision;

            PlayerAttackImpactRequestOutcome outcome = run.RequestPlayerAttackImpact(Direction3D.Right);

            Assert.That(outcome.HasRequest, Is.True);
            Assert.That(run.Revision, Is.EqualTo(revisionBeforeRequest));
            Assert.That(outcome.Request.Range, Is.EqualTo(Distance.FromValue(50f)));
        }

        [Test]
        public void RangedImpactDamagesResolvedEnemy()
        {
            ArenaRun run = _kit.StartRun();
            EnemyId enemyId = SpawnRegular(run, 10UL, s_farSpawn);
            run.StartPlayerAttack();
            PlayerAttackImpactRequest request = run.RequestPlayerAttackImpact(Direction3D.Right).Request;

            PlayerAttackImpactOutcome outcome = run.ApplyPlayerAttackImpact(
                new PlayerAttackImpactResolution(request.Correlation, new[] { enemyId }));

            Assert.That(outcome.Status, Is.EqualTo(PlayerAttackImpactStatus.Hit));
            Assert.That(run.ActiveEnemyCount, Is.EqualTo(1));
            Assert.That(run.HasPendingPlayerAttack, Is.False);
            Assert.That(run.HasPendingInteraction, Is.False);

            EnemyDamaged damaged = (EnemyDamaged)outcome.Change.DomainEvents[0];

            Assert.That(damaged.EnemyId, Is.EqualTo(enemyId));
            Assert.That(damaged.Damage.Points, Is.EqualTo(10));
            Assert.That(damaged.RemainingHealth.Current, Is.EqualTo(40));
            Assert.That(damaged.AggregateRevision, Is.EqualTo(run.Revision));
        }

        [Test]
        public void ImpactCompletesAttackAndRaisesCompletionEvent()
        {
            ArenaRun run = _kit.StartRun();
            EnemyId enemyId = SpawnRegular(run, 10UL, s_farSpawn);
            run.StartPlayerAttack();
            PlayerAttackImpactRequest request = run.RequestPlayerAttackImpact(Direction3D.Right).Request;

            PlayerAttackImpactOutcome outcome = run.ApplyPlayerAttackImpact(
                new PlayerAttackImpactResolution(request.Correlation, new[] { enemyId }));

            IReadOnlyList<IArenaDomainEvent> events = outcome.Change.DomainEvents;
            PlayerAttackCompleted completed = (PlayerAttackCompleted)events[events.Count - 1];

            Assert.That(completed.Outcome, Is.EqualTo(AttackOutcome.Hit));
            Assert.That(completed.AttackId, Is.EqualTo(request.AttackId));
            Assert.That(completed.WeaponKind, Is.EqualTo(WeaponKind.Ranged));
        }

        [Test]
        public void MissCompletesAttackWithoutDamage()
        {
            ArenaRun run = _kit.StartRun();
            SpawnRegular(run, 10UL, s_farSpawn);
            run.StartPlayerAttack();
            PlayerAttackImpactRequest request = run.RequestPlayerAttackImpact(Direction3D.Right).Request;
            AggregateRevision revisionBeforeImpact = run.Revision;

            PlayerAttackImpactOutcome outcome = run.ApplyPlayerAttackImpact(
                PlayerAttackImpactResolution.Miss(request.Correlation));

            PlayerAttackCompleted completed = (PlayerAttackCompleted)outcome.Change.DomainEvents[0];

            Assert.That(outcome.Status, Is.EqualTo(PlayerAttackImpactStatus.Missed));
            Assert.That(completed.Outcome, Is.EqualTo(AttackOutcome.Miss));
            Assert.That(outcome.Change.DomainEvents.Count, Is.EqualTo(1));
            Assert.That(run.HasPendingPlayerAttack, Is.False);
            Assert.That(run.Revision.Value, Is.EqualTo(revisionBeforeImpact.Value + 1UL));
            Assert.That(outcome.Change.HasStateChange, Is.True);
        }

        [Test]
        public void LethalDamageRemovesEnemyAndRaisesEnemyDefeated()
        {
            ArenaRun run = _kit.StartRun();
            EnemyId enemyId = SpawnRegular(run, 10UL, s_farSpawn);

            for (int shot = 0; shot < 4; shot++)
            {
                FireRanged(run, enemyId);
            }

            PlayerAttackImpactOutcome outcome = FireRanged(run, enemyId);

            Assert.That(run.ContainsEnemy(enemyId), Is.False);
            Assert.That(run.ActiveEnemyCount, Is.EqualTo(0));

            EnemyDefeated defeated = (EnemyDefeated)outcome.Change.DomainEvents[1];

            Assert.That(defeated.EnemyId, Is.EqualTo(enemyId));
            Assert.That(defeated.EnemyKind, Is.EqualTo(EnemyKind.Regular));
        }

        [Test]
        public void DefeatingLastRegularEnemyEntersBossCombat()
        {
            ArenaRun run = _kit.StartRun(Position3D.Zero, _kit.Waves(1));
            EnemyId enemyId = SpawnRegular(run, 10UL, s_farSpawn);

            for (int shot = 0; shot < 5; shot++)
            {
                FireRanged(run, enemyId);
            }

            Assert.That(run.CurrentWavePhase, Is.EqualTo(WavePhase.BossCombat));
            Assert.That(run.CurrentBossStatus, Is.EqualTo(BossStatus.NotSpawned));
        }

        [Test]
        public void UnknownTargetIsRejectedWithoutSideEffects()
        {
            ArenaRun run = _kit.StartRun();
            SpawnRegular(run, 10UL, s_farSpawn);
            run.StartPlayerAttack();
            PlayerAttackImpactRequest request = run.RequestPlayerAttackImpact(Direction3D.Right).Request;
            AggregateRevision revisionBeforeImpact = run.Revision;

            PlayerAttackImpactOutcome outcome = run.ApplyPlayerAttackImpact(
                new PlayerAttackImpactResolution(request.Correlation, new[] { EnemyId.FromValue(777UL) }));

            Assert.That(outcome.RejectionReason, Is.EqualTo(PlayerAttackImpactRejectionReason.UnknownTarget));
            Assert.That(run.Revision, Is.EqualTo(revisionBeforeImpact));
            Assert.That(run.HasPendingPlayerAttack, Is.True);
            Assert.That(run.HasPendingInteraction, Is.True);
        }

        [Test]
        public void TargetBeyondWeaponRangeIsRejected()
        {
            ArenaRun run = _kit.StartRunInSmallArena();
            EnemyId enemyId = SpawnRegular(run, 10UL, new Position3D(6f, 0f, 0f));
            run.SwitchWeapon();
            run.StartPlayerAttack();
            run.AdvanceTime(new GameDuration(0.3d));
            PlayerAttackImpactRequest request = run.RequestPlayerAttackImpact(Direction3D.Right).Request;

            PlayerAttackImpactOutcome outcome = run.ApplyPlayerAttackImpact(
                new PlayerAttackImpactResolution(request.Correlation, new[] { enemyId }));

            Assert.That(outcome.RejectionReason, Is.EqualTo(PlayerAttackImpactRejectionReason.TargetOutOfRange));
        }

        [Test]
        public void RangedResolutionWithMultipleTargetsIsRejected()
        {
            ArenaRun run = _kit.StartRun();
            EnemyId first = SpawnRegular(run, 10UL, s_farSpawn);
            EnemyId second = SpawnRegular(run, 11UL, new Position3D(-22f, 0f, 0f));
            run.StartPlayerAttack();
            PlayerAttackImpactRequest request = run.RequestPlayerAttackImpact(Direction3D.Right).Request;

            PlayerAttackImpactOutcome outcome = run.ApplyPlayerAttackImpact(
                new PlayerAttackImpactResolution(request.Correlation, new[] { first, second }));

            Assert.That(outcome.RejectionReason, Is.EqualTo(PlayerAttackImpactRejectionReason.TooManyTargets));
        }

        [Test]
        public void MeleeAttackDamagesEachEnemyOnce()
        {
            ArenaRun run = _kit.StartRunInSmallArena();
            EnemyId enemyId = SpawnRegular(run, 10UL, s_closeSpawn);
            run.SwitchWeapon();
            run.StartPlayerAttack();
            run.AdvanceTime(new GameDuration(0.3d));
            PlayerAttackImpactRequest request = run.RequestPlayerAttackImpact(Direction3D.Right).Request;

            PlayerAttackImpactOutcome outcome = run.ApplyPlayerAttackImpact(
                new PlayerAttackImpactResolution(request.Correlation, new[] { enemyId, enemyId }));

            EnemyDamaged damaged = (EnemyDamaged)outcome.Change.DomainEvents[0];

            Assert.That(outcome.Status, Is.EqualTo(PlayerAttackImpactStatus.Hit));
            Assert.That(damaged.RemainingHealth.Current, Is.EqualTo(30));
            Assert.That(outcome.Change.DomainEvents.Count, Is.EqualTo(2));
        }

        [Test]
        public void RejectedImpactAllowsRetryWithValidResolution()
        {
            ArenaRun run = _kit.StartRun();
            EnemyId enemyId = SpawnRegular(run, 10UL, s_farSpawn);
            run.StartPlayerAttack();
            PlayerAttackImpactRequest request = run.RequestPlayerAttackImpact(Direction3D.Right).Request;

            run.ApplyPlayerAttackImpact(
                new PlayerAttackImpactResolution(request.Correlation, new[] { EnemyId.FromValue(777UL) }));

            PlayerAttackImpactOutcome outcome = run.ApplyPlayerAttackImpact(
                new PlayerAttackImpactResolution(request.Correlation, new[] { enemyId }));

            Assert.That(outcome.Status, Is.EqualTo(PlayerAttackImpactStatus.Hit));
        }

        [Test]
        public void DuplicateImpactResolutionIsRejected()
        {
            ArenaRun run = _kit.StartRun();
            EnemyId enemyId = SpawnRegular(run, 10UL, s_farSpawn);
            run.StartPlayerAttack();
            PlayerAttackImpactRequest request = run.RequestPlayerAttackImpact(Direction3D.Right).Request;
            PlayerAttackImpactResolution resolution = new PlayerAttackImpactResolution(
                request.Correlation,
                new[] { enemyId });

            run.ApplyPlayerAttackImpact(resolution);
            AggregateRevision revisionAfterImpact = run.Revision;

            PlayerAttackImpactOutcome duplicate = run.ApplyPlayerAttackImpact(resolution);

            Assert.That(duplicate.RejectionReason, Is.EqualTo(PlayerAttackImpactRejectionReason.InteractionClosed));
            Assert.That(run.Revision, Is.EqualTo(revisionAfterImpact));
        }

        [Test]
        public void SecondAttackRequiresCooldownAfterCompletion()
        {
            ArenaRun run = _kit.StartRun();
            EnemyId enemyId = SpawnRegular(run, 10UL, s_farSpawn);
            FireRanged(run, enemyId);

            PlayerAttackStartOutcome tooEarly = run.StartPlayerAttack();
            run.AdvanceTime(new GameDuration(0.5d));
            PlayerAttackStartOutcome afterCooldown = run.StartPlayerAttack();

            Assert.That(tooEarly.Status, Is.EqualTo(PlayerAttackStartStatus.WeaponNotReady));
            Assert.That(afterCooldown.IsStarted, Is.True);
        }

        [Test]
        public void ImpactRequestWhileInteractionPendingThrows()
        {
            ArenaRun run = _kit.StartRun();
            run.StartPlayerAttack();
            run.RequestPlayerAttackImpact(Direction3D.Right);

            Assert.Throws<InvalidOperationException>(
                () =>
                {
                    run.RequestPlayerAttackImpact(Direction3D.Right);
                });
        }

        private PlayerAttackImpactOutcome FireRanged(ArenaRun run, EnemyId enemyId)
        {
            if (run.HasPendingPlayerAttack == false)
            {
                PlayerAttackStartOutcome start = run.StartPlayerAttack();

                if (start.IsStarted == false)
                {
                    run.AdvanceTime(new GameDuration(0.5d));
                    run.StartPlayerAttack();
                }
            }

            PlayerAttackImpactRequest request = run.RequestPlayerAttackImpact(Direction3D.Right).Request;

            return run.ApplyPlayerAttackImpact(
                new PlayerAttackImpactResolution(request.Correlation, new[] { enemyId }));
        }

        private EnemyId SpawnRegular(ArenaRun run, ulong value, Position3D position)
        {
            EnemySpawnRequest request = run.RequestEnemySpawn().Request;
            EnemyId enemyId = EnemyId.FromValue(value);

            run.ApplyEnemySpawn(new EnemySpawnResolution(request.Correlation, enemyId, position));

            return enemyId;
        }
    }
}
