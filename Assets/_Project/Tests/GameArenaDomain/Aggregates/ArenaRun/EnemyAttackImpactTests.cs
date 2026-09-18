using System.Collections.Generic;
using Game.Arena.Domain.Aggregates.ArenaRun;
using Game.Arena.Domain.Combat;
using Game.Arena.Domain.Concurrency;
using Game.Arena.Domain.Events;
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
    public sealed class EnemyAttackImpactTests
    {
        private static readonly Position3D s_eastInRange = new(2f, 0f, 0f);
        private static readonly Position3D s_westInRange = new(-2f, 0f, 0f);
        private static readonly Position3D s_outOfRange = new(5f, 0f, 0f);

        private ArenaRunTestKit _kit;

        [SetUp]
        public void SetUp()
        {
            _kit = new ArenaRunTestKit();
        }

        [Test]
        public void EnemyAttackImpactBeforeDueTimeDoesNotChangeState()
        {
            ArenaRun run = _kit.StartRunInCompactArenaWithWeakEnemies();
            Spawn(run, 10UL, s_eastInRange);
            run.StartEligibleEnemyAttacks();
            AggregateRevision revisionBeforeResolve = run.Revision;

            EnemyAttackImpactOutcome outcome = run.ResolveDueEnemyAttackImpacts();

            Assert.That(outcome.Status, Is.EqualTo(EnemyAttackImpactStatus.NoAttacksDue));
            Assert.That(run.Revision, Is.EqualTo(revisionBeforeResolve));
            Assert.That(run.PendingEnemyAttackCount, Is.EqualTo(1));
            Assert.That(outcome.Change.HasStateChange, Is.False);
        }

        [Test]
        public void EnemyAttackImpactInRangeDamagesPlayer()
        {
            ArenaRun run = _kit.StartRunInCompactArenaWithWeakEnemies();
            EnemyId enemyId = Spawn(run, 10UL, s_eastInRange);
            run.StartEligibleEnemyAttacks();
            run.AdvanceTime(new GameDuration(0.2d));

            EnemyAttackImpactOutcome outcome = run.ResolveDueEnemyAttackImpacts();

            Assert.That(outcome.IsResolved, Is.True);
            Assert.That(outcome.HitCount, Is.EqualTo(1));
            Assert.That(run.PlayerHealth.Current, Is.EqualTo(95));

            PlayerDamaged damaged = (PlayerDamaged)outcome.Change.DomainEvents[0];

            Assert.That(damaged.SourceEnemyId, Is.EqualTo(enemyId));
            Assert.That(damaged.AppliedDamage.Points, Is.EqualTo(5));
            Assert.That(damaged.RemainingHealth.Current, Is.EqualTo(95));
            Assert.That(damaged.AggregateRevision, Is.EqualTo(run.Revision));
        }

        [Test]
        public void EnemyAttackImpactOutOfRangeMisses()
        {
            ArenaRun run = _kit.StartRunInCompactArenaWithWeakEnemies();
            EnemyId enemyId = Spawn(run, 10UL, new Position3D(3f, 0f, 0f));
            run.StartEligibleEnemyAttacks();
            run.AdvanceTime(new GameDuration(0.2d));
            MovePlayerAway(run);

            EnemyAttackImpactOutcome outcome = run.ResolveDueEnemyAttackImpacts();

            Assert.That(outcome.IsResolved, Is.True);
            Assert.That(outcome.HitCount, Is.EqualTo(0));
            Assert.That(run.PlayerHealth.Current, Is.EqualTo(run.PlayerHealth.Maximum));

            EnemyAttackCompleted completed = (EnemyAttackCompleted)outcome.Change.DomainEvents[0];

            Assert.That(completed.Outcome, Is.EqualTo(AttackOutcome.Miss));
            Assert.That(completed.EnemyId, Is.EqualTo(enemyId));
            Assert.That(outcome.Change.DomainEvents.Count, Is.EqualTo(1));
        }

        [Test]
        public void EnemyAttackImpactCompletesPendingAttack()
        {
            ArenaRun run = _kit.StartRunInCompactArenaWithWeakEnemies();
            Spawn(run, 10UL, s_eastInRange);
            run.StartEligibleEnemyAttacks();
            run.AdvanceTime(new GameDuration(0.2d));

            run.ResolveDueEnemyAttackImpacts();

            Assert.That(run.PendingEnemyAttackCount, Is.EqualTo(0));
            Assert.That(run.DueEnemyAttackCount, Is.EqualTo(0));
        }

        [Test]
        public void EnemyAttackMissDoesNotRestoreCooldown()
        {
            ArenaRun run = _kit.StartRunInCompactArenaWithWeakEnemies();
            Spawn(run, 10UL, new Position3D(3f, 0f, 0f));
            run.StartEligibleEnemyAttacks();
            run.AdvanceTime(new GameDuration(0.2d));
            MovePlayerAway(run);
            run.ResolveDueEnemyAttackImpacts();

            EnemyAttackStartOutcome restart = run.StartEligibleEnemyAttacks();

            Assert.That(restart.Status, Is.EqualTo(EnemyAttackStartStatus.NoEligibleEnemies));
        }

        [Test]
        public void DueEnemyAttacksResolveInEnemyIdOrder()
        {
            ArenaRun run = _kit.StartRunInCompactArenaWithWeakEnemies();
            Spawn(run, 30UL, s_eastInRange);
            Spawn(run, 20UL, s_westInRange);
            run.StartEligibleEnemyAttacks();
            run.AdvanceTime(new GameDuration(0.2d));

            EnemyAttackImpactOutcome outcome = run.ResolveDueEnemyAttackImpacts();
            PlayerDamaged first = (PlayerDamaged)outcome.Change.DomainEvents[0];
            PlayerDamaged second = (PlayerDamaged)outcome.Change.DomainEvents[2];

            Assert.That(first.SourceEnemyId, Is.EqualTo(EnemyId.FromValue(20UL)));
            Assert.That(second.SourceEnemyId, Is.EqualTo(EnemyId.FromValue(30UL)));
        }

        [Test]
        public void MultipleEnemyImpactsAdvanceRevisionOnce()
        {
            ArenaRun run = _kit.StartRunInCompactArenaWithWeakEnemies();
            Spawn(run, 10UL, s_eastInRange);
            Spawn(run, 11UL, s_westInRange);
            run.StartEligibleEnemyAttacks();
            run.AdvanceTime(new GameDuration(0.2d));
            AggregateRevision revisionBeforeResolve = run.Revision;

            EnemyAttackImpactOutcome outcome = run.ResolveDueEnemyAttackImpacts();

            Assert.That(outcome.ResolvedAttackCount, Is.EqualTo(2));
            Assert.That(run.Revision.Value, Is.EqualTo(revisionBeforeResolve.Value + 1UL));
            Assert.That(run.PlayerHealth.Current, Is.EqualTo(90));
        }

        [Test]
        public void MultipleEnemyImpactsShareResultingRevision()
        {
            ArenaRun run = _kit.StartRunInCompactArenaWithWeakEnemies();
            Spawn(run, 10UL, s_eastInRange);
            Spawn(run, 11UL, s_westInRange);
            run.StartEligibleEnemyAttacks();
            run.AdvanceTime(new GameDuration(0.2d));

            EnemyAttackImpactOutcome outcome = run.ResolveDueEnemyAttackImpacts();
            IReadOnlyList<IArenaDomainEvent> events = outcome.Change.DomainEvents;

            Assert.That(events.Count, Is.EqualTo(4));

            for (int index = 0; index < events.Count; index++)
            {
                Assert.That(events[index].AggregateRevision, Is.EqualTo(run.Revision));
            }
        }

        [Test]
        public void PlayerDamagedPrecedesEnemyAttackCompleted()
        {
            ArenaRun run = _kit.StartRunInCompactArenaWithWeakEnemies();
            Spawn(run, 10UL, s_eastInRange);
            run.StartEligibleEnemyAttacks();
            run.AdvanceTime(new GameDuration(0.2d));

            EnemyAttackImpactOutcome outcome = run.ResolveDueEnemyAttackImpacts();

            Assert.That(outcome.Change.DomainEvents[0], Is.TypeOf<PlayerDamaged>());
            Assert.That(outcome.Change.DomainEvents[1], Is.TypeOf<EnemyAttackCompleted>());
        }

        [Test]
        public void EnemyAttackImpactWithoutPendingAttacksDoesNothing()
        {
            ArenaRun run = _kit.StartRunInCompactArenaWithWeakEnemies();
            Spawn(run, 10UL, s_eastInRange);

            EnemyAttackImpactOutcome outcome = run.ResolveDueEnemyAttackImpacts();

            Assert.That(outcome.Status, Is.EqualTo(EnemyAttackImpactStatus.NoAttacksDue));
            Assert.That(run.Revision, Is.EqualTo(AggregateRevision.Initial.Next()));
        }

        [Test]
        public void EnemyAttackImpactIsRejectedWhileInteractionPending()
        {
            ArenaRun run = _kit.StartRunInCompactArenaWithWeakEnemies();
            Spawn(run, 10UL, s_eastInRange);
            Spawn(run, 11UL, s_outOfRange);
            run.StartEligibleEnemyAttacks();
            run.AdvanceTime(new GameDuration(0.2d));
            run.RequestEnemyMovementBatch(new GameDuration(0.1d));

            EnemyAttackImpactOutcome outcome = run.ResolveDueEnemyAttackImpacts();

            Assert.That(outcome.Status, Is.EqualTo(EnemyAttackImpactStatus.InteractionPending));
            Assert.That(run.PendingEnemyAttackCount, Is.EqualTo(1));
        }

        [Test]
        public void LethalEnemyAttackIsNotSupportedYet()
        {
            ArenaRun run = _kit.StartRunInCompactArena();
            Spawn(run, 10UL, s_eastInRange);
            run.StartEligibleEnemyAttacks();
            run.AdvanceTime(new GameDuration(0.2d));

            for (int hit = 0; hit < 12; hit++)
            {
                if (run.PlayerHealth.Current <= 8)
                {
                    break;
                }

                run.ResolveDueEnemyAttackImpacts();
                run.AdvanceTime(new GameDuration(1d));
                run.StartEligibleEnemyAttacks();
                run.AdvanceTime(new GameDuration(0.2d));
            }

            EnemyAttackImpactOutcome outcome = run.ResolveDueEnemyAttackImpacts();

            Assert.That(outcome.Status, Is.EqualTo(EnemyAttackImpactStatus.DefeatNotSupported));
            Assert.That(run.Status, Is.EqualTo(ArenaRunStatus.Playing));
            Assert.That(run.PlayerHealth.IsDepleted, Is.False);
            Assert.That(run.PendingEnemyAttackCount, Is.EqualTo(1));
        }

        private void MovePlayerAway(ArenaRun run)
        {
            MovementInput input = MovementInput.FromVector(new Displacement3D(-1f, 0f, 0f));
            PlayerMovementRequest request = run.RequestPlayerMovement(input, new GameDuration(0.2d)).Request;

            run.ApplyPlayerMovement(new PlayerMovementResolution(request.Correlation, request.Intent.RequestedPosition));
        }

        private EnemyId Spawn(ArenaRun run, ulong value, Position3D position)
        {
            EnemySpawnRequest request = run.RequestEnemySpawn().Request;
            EnemyId enemyId = EnemyId.FromValue(value);

            run.ApplyEnemySpawn(new EnemySpawnResolution(request.Correlation, enemyId, position));

            return enemyId;
        }
    }
}
