using System.Collections.Generic;
using Game.Arena.Domain.Aggregates.ArenaRun;
using Game.Arena.Domain.Concurrency;
using Game.Arena.Domain.Events;
using Game.Arena.Domain.Geometry;
using Game.Arena.Domain.Identity;
using Game.Arena.Domain.Interactions.Spawn;
using Game.Arena.Domain.Tests.Support;
using Game.Arena.Domain.Time;
using NUnit.Framework;

namespace Game.Arena.Domain.Tests.Aggregates._ArenaRun
{
    [TestFixture]
    public sealed class EnemyAttackStartTests
    {
        private static readonly Position3D s_inRange = new(2f, 0f, 0f);
        private static readonly Position3D s_outOfRange = new(5f, 0f, 0f);

        private ArenaRunTestKit _kit;

        [SetUp]
        public void SetUp()
        {
            _kit = new ArenaRunTestKit();
        }

        [Test]
        public void EnemyInRangeStartsAttack()
        {
            ArenaRun run = _kit.StartRunInCompactArena();
            EnemyId enemyId = Spawn(run, 10UL, s_inRange);

            EnemyAttackStartOutcome outcome = run.StartEligibleEnemyAttacks();

            Assert.That(outcome.IsStarted, Is.True);
            Assert.That(outcome.StartedAttacks.Count, Is.EqualTo(1));
            Assert.That(outcome.StartedAttacks[0].EnemyId, Is.EqualTo(enemyId));
            Assert.That(run.PendingEnemyAttackCount, Is.EqualTo(1));
        }

        [Test]
        public void StartingEnemyAttackDoesNotDamagePlayer()
        {
            ArenaRun run = _kit.StartRunInCompactArena();
            Spawn(run, 10UL, s_inRange);

            run.StartEligibleEnemyAttacks();

            Assert.That(run.PlayerHealth.Current, Is.EqualTo(run.PlayerHealth.Maximum));
        }

        [Test]
        public void EnemyOutOfRangeDoesNotStartAttack()
        {
            ArenaRun run = _kit.StartRunInCompactArena();
            Spawn(run, 10UL, s_outOfRange);

            EnemyAttackStartOutcome outcome = run.StartEligibleEnemyAttacks();

            Assert.That(outcome.Status, Is.EqualTo(EnemyAttackStartStatus.NoEligibleEnemies));
            Assert.That(run.PendingEnemyAttackCount, Is.EqualTo(0));
            Assert.That(run.Revision, Is.EqualTo(AggregateRevision.Initial.Next()));
            Assert.That(outcome.Change.HasStateChange, Is.False);
        }

        [Test]
        public void BatchAdvancesRevisionOnceForAllStartedAttacks()
        {
            ArenaRun run = _kit.StartRunInCompactArena();
            Spawn(run, 10UL, s_inRange);
            Spawn(run, 11UL, new Position3D(-2f, 0f, 0f));
            AggregateRevision revisionBeforeBatch = run.Revision;

            EnemyAttackStartOutcome outcome = run.StartEligibleEnemyAttacks();

            Assert.That(outcome.StartedAttacks.Count, Is.EqualTo(2));
            Assert.That(run.Revision.Value, Is.EqualTo(revisionBeforeBatch.Value + 1UL));
            Assert.That(outcome.Change.Revision, Is.EqualTo(run.Revision));
        }

        [Test]
        public void AllBatchEventsShareResultingRevision()
        {
            ArenaRun run = _kit.StartRunInCompactArena();
            Spawn(run, 10UL, s_inRange);
            Spawn(run, 11UL, new Position3D(-2f, 0f, 0f));

            EnemyAttackStartOutcome outcome = run.StartEligibleEnemyAttacks();
            IReadOnlyList<IArenaDomainEvent> events = outcome.Change.DomainEvents;

            Assert.That(events.Count, Is.EqualTo(2));

            for (int index = 0; index < events.Count; index++)
            {
                Assert.That(events[index].AggregateRevision, Is.EqualTo(run.Revision));
            }
        }

        [Test]
        public void AttacksStartInAscendingEnemyIdOrder()
        {
            ArenaRun run = _kit.StartRunInCompactArena();
            Spawn(run, 30UL, s_inRange);
            Spawn(run, 20UL, new Position3D(-2f, 0f, 0f));

            EnemyAttackStartOutcome outcome = run.StartEligibleEnemyAttacks();

            Assert.That(outcome.StartedAttacks[0].EnemyId, Is.EqualTo(EnemyId.FromValue(20UL)));
            Assert.That(outcome.StartedAttacks[1].EnemyId, Is.EqualTo(EnemyId.FromValue(30UL)));
            Assert.That(outcome.StartedAttacks[0].Id.Value, Is.LessThan(outcome.StartedAttacks[1].Id.Value));
        }

        [Test]
        public void StartedAttackCarriesWindupFromDefinition()
        {
            ArenaRun run = _kit.StartRunInCompactArena();
            Spawn(run, 10UL, s_inRange);
            run.AdvanceTime(new GameDuration(1d));

            EnemyAttackStartOutcome outcome = run.StartEligibleEnemyAttacks();
            EnemyAttackStarted started = (EnemyAttackStarted)outcome.Change.DomainEvents[0];

            Assert.That(started.StartedAt, Is.EqualTo(new GameTimePoint(1d)));
            Assert.That(started.ImpactAt, Is.EqualTo(new GameTimePoint(1.2d)));
        }

        [Test]
        public void EnemyWithPendingAttackDoesNotStartAnother()
        {
            ArenaRun run = _kit.StartRunInCompactArena();
            Spawn(run, 10UL, s_inRange);
            run.StartEligibleEnemyAttacks();
            run.AdvanceTime(new GameDuration(5d));

            EnemyAttackStartOutcome outcome = run.StartEligibleEnemyAttacks();

            Assert.That(outcome.Status, Is.EqualTo(EnemyAttackStartStatus.NoEligibleEnemies));
            Assert.That(run.PendingEnemyAttackCount, Is.EqualTo(1));
        }

        [Test]
        public void BatchIsRejectedWhileInteractionIsPending()
        {
            ArenaRun run = _kit.StartRunInCompactArena();
            Spawn(run, 10UL, s_inRange);
            MovementInput input = MovementInput.FromVector(new Displacement3D(1f, 0f, 0f));
            run.RequestPlayerMovement(input, new GameDuration(0.05d));

            EnemyAttackStartOutcome outcome = run.StartEligibleEnemyAttacks();

            Assert.That(outcome.Status, Is.EqualTo(EnemyAttackStartStatus.InteractionPending));
            Assert.That(run.PendingEnemyAttackCount, Is.EqualTo(0));
        }

        [Test]
        public void BatchWithoutEnemiesDoesNotChangeState()
        {
            ArenaRun run = _kit.StartRunInCompactArena();

            EnemyAttackStartOutcome outcome = run.StartEligibleEnemyAttacks();

            Assert.That(outcome.Status, Is.EqualTo(EnemyAttackStartStatus.NoEligibleEnemies));
            Assert.That(outcome.StartedAttacks, Is.Empty);
            Assert.That(run.Revision, Is.EqualTo(AggregateRevision.Initial));
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
