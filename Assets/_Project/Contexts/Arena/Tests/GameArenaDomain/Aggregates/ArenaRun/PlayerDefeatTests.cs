using System.Collections.Generic;
using Game.Arena.Domain.Aggregates.ArenaRun;
using Game.Arena.Domain.Combat;
using Game.Arena.Domain.Configuration;
using Game.Arena.Domain.Events;
using Game.Arena.Domain.Geometry;
using Game.Arena.Domain.Identity;
using Game.Arena.Domain.Interactions.Attack;
using Game.Arena.Domain.Interactions.Movement;
using Game.Arena.Domain.Interactions.Spawn;
using Game.Arena.Domain.Tests.Support;
using Game.Arena.Domain.Time;
using Game.Arena.Domain.Vitality;
using NUnit.Framework;

namespace Game.Arena.Domain.Tests.Aggregates._ArenaRun
{
    [TestFixture]
    public sealed class PlayerDefeatTests
    {
        private static readonly Position3D s_eastInRange = new(2f, 0f, 0f);
        private static readonly Position3D s_westInRange = new(-2f, 0f, 0f);
        private static readonly Position3D s_farSpawn = new(22f, 0f, 0f);

        private ArenaRunTestKit _kit;

        [SetUp]
        public void SetUp()
        {
            _kit = new ArenaRunTestKit();
        }

        [Test]
        public void LethalEnemyAttackDefeatsPlayer()
        {
            ArenaRun run = StartCompactRunWithPlayerHealth(8);
            Spawn(run, 10UL, s_eastInRange);
            run.StartEligibleEnemyAttacks();
            run.AdvanceTime(new GameDuration(0.2d));

            EnemyAttackImpactOutcome outcome = run.ResolveDueEnemyAttackImpacts();

            Assert.That(run.PlayerHealth.IsDepleted, Is.True);
            Assert.That(FindEvent<PlayerDefeated>(outcome.Change.DomainEvents).PlayerId, Is.EqualTo(run.PlayerId));
        }

        [Test]
        public void LethalEnemyAttackDefeatsArenaRun()
        {
            ArenaRun run = StartCompactRunWithPlayerHealth(8);
            Spawn(run, 10UL, s_eastInRange);
            run.StartEligibleEnemyAttacks();
            run.AdvanceTime(new GameDuration(0.2d));

            run.ResolveDueEnemyAttackImpacts();

            Assert.That(run.Status, Is.EqualTo(ArenaRunStatus.Defeat));
        }

        [Test]
        public void OverkillReportsOnlyAppliedDamage()
        {
            ArenaRun run = StartCompactRunWithPlayerHealth(5);
            Spawn(run, 10UL, s_eastInRange);
            run.StartEligibleEnemyAttacks();
            run.AdvanceTime(new GameDuration(0.2d));

            EnemyAttackImpactOutcome outcome = run.ResolveDueEnemyAttackImpacts();
            PlayerDamaged damaged = FindEvent<PlayerDamaged>(outcome.Change.DomainEvents);

            Assert.That(damaged.AppliedDamage.Points, Is.EqualTo(5));
            Assert.That(damaged.RemainingHealth.Current, Is.EqualTo(0));
            Assert.That(run.PlayerHealth.Current, Is.EqualTo(0));
        }

        [Test]
        public void LethalEnemyAttackStopsRemainingDamage()
        {
            ArenaRun run = StartCompactRunWithPlayerHealth(8);
            Spawn(run, 10UL, s_eastInRange);
            Spawn(run, 20UL, s_westInRange);
            run.StartEligibleEnemyAttacks();
            run.AdvanceTime(new GameDuration(0.2d));

            EnemyAttackImpactOutcome outcome = run.ResolveDueEnemyAttackImpacts();

            Assert.That(outcome.HitCount, Is.EqualTo(1));
            Assert.That(run.PlayerHealth.Current, Is.EqualTo(0));
            Assert.That(CountEvents<PlayerDamaged>(outcome.Change.DomainEvents), Is.EqualTo(1));
        }

        [Test]
        public void LethalEnemyAttackCancelsRemainingEnemyAttacks()
        {
            ArenaRun run = StartCompactRunWithPlayerHealth(8);
            Spawn(run, 10UL, s_eastInRange);
            Spawn(run, 20UL, s_westInRange);
            run.StartEligibleEnemyAttacks();
            run.AdvanceTime(new GameDuration(0.2d));

            EnemyAttackImpactOutcome outcome = run.ResolveDueEnemyAttackImpacts();
            EnemyAttackCancelled cancelled = FindEvent<EnemyAttackCancelled>(outcome.Change.DomainEvents);

            Assert.That(cancelled.EnemyId, Is.EqualTo(EnemyId.FromValue(20UL)));
            Assert.That(cancelled.Cause, Is.EqualTo(AttackCancellationCause.ArenaRunTerminated));
            Assert.That(run.PendingEnemyAttackCount, Is.EqualTo(0));
        }

        [Test]
        public void LethalEnemyAttackCancelsPendingPlayerAttack()
        {
            ArenaRun run = StartCompactRunWithPlayerHealth(8);
            Spawn(run, 10UL, s_eastInRange);
            run.StartEligibleEnemyAttacks();
            run.StartPlayerAttack();
            run.AdvanceTime(new GameDuration(0.2d));

            EnemyAttackImpactOutcome outcome = run.ResolveDueEnemyAttackImpacts();
            PlayerAttackCancelled cancelled = FindEvent<PlayerAttackCancelled>(outcome.Change.DomainEvents);

            Assert.That(cancelled.Cause, Is.EqualTo(AttackCancellationCause.AttackerDefeated));
            Assert.That(run.HasPendingPlayerAttack, Is.False);
        }

        [Test]
        public void AllDefeatEventsShareResultingRevision()
        {
            ArenaRun run = StartCompactRunWithPlayerHealth(8);
            Spawn(run, 10UL, s_eastInRange);
            Spawn(run, 20UL, s_westInRange);
            run.StartEligibleEnemyAttacks();
            run.StartPlayerAttack();
            run.AdvanceTime(new GameDuration(0.2d));

            EnemyAttackImpactOutcome outcome = run.ResolveDueEnemyAttackImpacts();
            IReadOnlyList<IArenaDomainEvent> events = outcome.Change.DomainEvents;

            for (int index = 0; index < events.Count; index++)
            {
                Assert.That(events[index].AggregateRevision, Is.EqualTo(run.Revision));
            }
        }

        [Test]
        public void ArenaRunDefeatedIsLastEvent()
        {
            ArenaRun run = StartCompactRunWithPlayerHealth(8);
            Spawn(run, 10UL, s_eastInRange);
            run.StartEligibleEnemyAttacks();
            run.AdvanceTime(new GameDuration(0.2d));

            EnemyAttackImpactOutcome outcome = run.ResolveDueEnemyAttackImpacts();
            IArenaDomainEvent last = outcome.Change.DomainEvents[outcome.Change.DomainEvents.Count - 1];

            Assert.That(last, Is.TypeOf<ArenaRunDefeated>());
        }

        [Test]
        public void DefeatEventOrderIsDeterministic()
        {
            ArenaRun run = StartCompactRunWithPlayerHealth(8);
            Spawn(run, 10UL, s_eastInRange);
            Spawn(run, 20UL, s_westInRange);
            run.StartEligibleEnemyAttacks();
            run.StartPlayerAttack();
            run.AdvanceTime(new GameDuration(0.2d));

            IReadOnlyList<IArenaDomainEvent> events =
                run.ResolveDueEnemyAttackImpacts().Change.DomainEvents;

            Assert.That(events[0], Is.TypeOf<PlayerDamaged>());
            Assert.That(events[1], Is.TypeOf<PlayerDefeated>());
            Assert.That(events[2], Is.TypeOf<EnemyAttackCompleted>());
            Assert.That(events[3], Is.TypeOf<EnemyAttackCancelled>());
            Assert.That(events[4], Is.TypeOf<PlayerAttackCancelled>());
            Assert.That(events[5], Is.TypeOf<ArenaRunDefeated>());
        }

        [Test]
        public void DefeatedRunRejectsFurtherCombatProgression()
        {
            ArenaRun run = StartCompactRunWithPlayerHealth(8);
            Spawn(run, 10UL, s_eastInRange);
            run.StartEligibleEnemyAttacks();
            run.AdvanceTime(new GameDuration(0.2d));
            run.ResolveDueEnemyAttackImpacts();

            Assert.That(run.StartPlayerAttack().Status, Is.EqualTo(PlayerAttackStartStatus.RunIsNotPlaying));
            Assert.That(run.RequestEnemySpawn().Status, Is.EqualTo(EnemySpawnRequestStatus.RunIsNotPlaying));
            Assert.That(run.SwitchWeapon().Status, Is.EqualTo(WeaponSwitchStatus.RunIsNotPlaying));
            Assert.That(run.AdvanceTime(new GameDuration(1d)).Status, Is.EqualTo(TimeAdvanceStatus.RunIsNotPlaying));
            Assert.That(run.StartEligibleEnemyAttacks().Status, Is.EqualTo(EnemyAttackStartStatus.RunIsNotPlaying));
            Assert.That(run.ResolveDueEnemyAttackImpacts().Status, Is.EqualTo(EnemyAttackImpactStatus.RunIsNotPlaying));
        }

        [Test]
        public void DefeatedEnemyPendingAttackIsCancelledByPlayerImpact()
        {
            ArenaRun run = StartCompactRunWithLowHealthEnemies();
            EnemyId enemyId = Spawn(run, 10UL, s_eastInRange);
            run.StartEligibleEnemyAttacks();

            PlayerAttackImpactOutcome outcome = FireRangedUntilDefeated(run, enemyId);
            EnemyAttackCancelled cancelled = FindEvent<EnemyAttackCancelled>(outcome.Change.DomainEvents);

            Assert.That(cancelled.EnemyId, Is.EqualTo(enemyId));
            Assert.That(cancelled.Cause, Is.EqualTo(AttackCancellationCause.AttackerDefeated));
            Assert.That(run.PendingEnemyAttackCount, Is.EqualTo(0));
            Assert.That(run.ContainsEnemy(enemyId), Is.False);
        }

        private ArenaRun StartCompactRunWithPlayerHealth(int maximumHealth)
        {
            return new ArenaRunFactory(_kit.MovementPath).Start(
                ArenaRunId.FromValue(1UL),
                PlayerId.FromValue(2UL),
                _kit.CompactArena,
                new PlayerDefinition(
                    Position3D.Zero,
                    Health.Full(maximumHealth),
                    MovementSpeed.FromUnitsPerSecond(5f),
                    CollisionRadius.FromValue(0.5f)),
                _kit.Enemies,
                _kit.Weapons,
                _kit.Waves(2, 1, 1));
        }

        private ArenaRun StartCompactRunWithLowHealthEnemies()
        {
            EnemyCatalog enemies = new EnemyCatalog(
                new EnemyDefinition(
                    EnemyKind.Regular,
                    Health.Full(10),
                    MovementSpeed.FromUnitsPerSecond(2f),
                    CollisionRadius.FromValue(0.5f),
                    DamageAmount.FromPoints(8),
                    Distance.FromValue(2f),
                    new GameDuration(1d),
                    new GameDuration(0.2d)),
                new EnemyDefinition(
                    EnemyKind.Boss,
                    Health.Full(10),
                    MovementSpeed.FromUnitsPerSecond(1.75f),
                    CollisionRadius.FromValue(1f),
                    DamageAmount.FromPoints(25),
                    Distance.FromValue(3f),
                    new GameDuration(1.1d),
                    new GameDuration(0.4d)));

            return new ArenaRunFactory(_kit.MovementPath).Start(
                ArenaRunId.FromValue(1UL),
                PlayerId.FromValue(2UL),
                _kit.CompactArena,
                _kit.PlayerAt(Position3D.Zero),
                enemies,
                _kit.Weapons,
                _kit.Waves(2, 1, 1));
        }

        private PlayerAttackImpactOutcome FireRangedUntilDefeated(ArenaRun run, EnemyId enemyId)
        {
            PlayerAttackImpactOutcome outcome = null;

            while (run.ContainsEnemy(enemyId))
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
                outcome = run.ApplyPlayerAttackImpact(new PlayerAttackImpactResolution(request.Correlation, new[] { enemyId }));
            }

            return outcome;
        }

        private EnemyId Spawn(ArenaRun run, ulong value, Position3D position)
        {
            EnemySpawnRequest request = run.RequestEnemySpawn().Request;
            EnemyId enemyId = EnemyId.FromValue(value);

            run.ApplyEnemySpawn(new EnemySpawnResolution(request.Correlation, enemyId, position));

            return enemyId;
        }

        private TEvent FindEvent<TEvent>(IReadOnlyList<IArenaDomainEvent> events)
        {
            for (int index = 0; index < events.Count; index++)
            {
                if (events[index] is TEvent found)
                {
                    return found;
                }
            }

            throw new AssertionException("Event was not found: " + typeof(TEvent).Name);
        }

        private int CountEvents<TEvent>(IReadOnlyList<IArenaDomainEvent> events)
        {
            int count = 0;

            for (int index = 0; index < events.Count; index++)
            {
                if (events[index] is TEvent)
                {
                    count++;
                }
            }

            return count;
        }
    }
}
