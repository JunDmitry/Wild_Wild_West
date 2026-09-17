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

namespace Game.Arena.Domain.Tests.Events
{
    [TestFixture]
    public sealed class WaveProgressionAndVictoryTests
    {
        private static readonly Position3D s_bossSpawn = new(22f, 0f, 0f);
        private static readonly Position3D s_regularSpawn = new(-22f, 0f, 0f);

        private ArenaRunTestKit _kit;

        [SetUp]
        public void SetUp()
        {
            _kit = new ArenaRunTestKit();
        }

        [Test]
        public void DefeatingLastRegularEnemyRaisesWavePhaseChangedToBossCombat()
        {
            ArenaRun run = StartRunWithLowHealthEnemies(_kit.Waves(1));
            EnemyId regularId = Spawn(run, EnemyId.FromValue(10UL), s_regularSpawn);

            PlayerAttackImpactOutcome outcome = FireRanged(run, regularId);

            Assert.That(run.CurrentWavePhase, Is.EqualTo(WavePhase.BossCombat));

            WavePhaseChanged phaseChanged = FindEvent<WavePhaseChanged>(
                outcome.Change.DomainEvents);

            Assert.That(phaseChanged.WaveNumber, Is.EqualTo(WaveNumber.First));
            Assert.That(phaseChanged.PreviousPhase, Is.EqualTo(WavePhase.RegularCombat));
            Assert.That(phaseChanged.NewPhase, Is.EqualTo(WavePhase.BossCombat));
            Assert.That(phaseChanged.AggregateRevision, Is.EqualTo(run.Revision));
        }

        [Test]
        public void DefeatingBossCompletesWave()
        {
            ArenaRun run = StartRunWithLowHealthEnemies(_kit.Waves(0, 0));
            EnemyId bossId = Spawn(run, EnemyId.FromValue(10UL), s_bossSpawn);

            PlayerAttackImpactOutcome outcome = FireRanged(run, bossId);

            WaveCompleted completed = FindEvent<WaveCompleted>(
                outcome.Change.DomainEvents);

            Assert.That(completed.WaveNumber, Is.EqualTo(WaveNumber.First));
            Assert.That(completed.AggregateRevision, Is.EqualTo(run.Revision));
        }

        [Test]
        public void CompletingNonFinalWaveStartsNextWave()
        {
            ArenaRun run = StartRunWithLowHealthEnemies(_kit.Waves(0, 0));
            EnemyId bossId = Spawn(run, EnemyId.FromValue(10UL), s_bossSpawn);

            PlayerAttackImpactOutcome outcome = FireRanged(run, bossId);

            Assert.That(run.Status, Is.EqualTo(ArenaRunStatus.Playing));
            Assert.That(run.CurrentWaveNumber, Is.EqualTo(WaveNumber.First.Next()));
            Assert.That(run.CurrentWavePhase, Is.EqualTo(WavePhase.BossCombat));

            WaveStarted started = FindEvent<WaveStarted>(
                outcome.Change.DomainEvents);

            Assert.That(started.WaveNumber, Is.EqualTo(WaveNumber.First.Next()));
            Assert.That(started.InitialPhase, Is.EqualTo(WavePhase.BossCombat));
            Assert.That(started.AggregateRevision, Is.EqualTo(run.Revision));
        }

        [Test]
        public void DefeatingFinalBossCompletesRun()
        {
            ArenaRun run = StartRunWithLowHealthEnemies(_kit.Waves(0));
            EnemyId bossId = Spawn(run, EnemyId.FromValue(10UL), s_bossSpawn);

            PlayerAttackImpactOutcome outcome = FireRanged(run, bossId);

            Assert.That(run.Status, Is.EqualTo(ArenaRunStatus.Victory));
            Assert.That(run.CurrentWaveNumber, Is.EqualTo(WaveNumber.First));
            Assert.That(run.CurrentWavePhase, Is.EqualTo(WavePhase.Completed));
            Assert.That(run.CurrentBossStatus, Is.EqualTo(BossStatus.Defeated));

            ArenaRunVictorious victorious = FindEvent<ArenaRunVictorious>(
                outcome.Change.DomainEvents);

            Assert.That(victorious.ArenaRunId, Is.EqualTo(run.Id));
            Assert.That(victorious.AggregateRevision, Is.EqualTo(run.Revision));
        }

        [Test]
        public void FinalBossImpactProducesWaveAndVictoryEventsInOneRevision()
        {
            ArenaRun run = StartRunWithLowHealthEnemies(_kit.Waves(0));
            EnemyId bossId = Spawn(run, EnemyId.FromValue(10UL), s_bossSpawn);

            PlayerAttackImpactOutcome outcome = FireRanged(run, bossId);

            WavePhaseChanged phaseChanged = FindEvent<WavePhaseChanged>(
                outcome.Change.DomainEvents);
            WaveCompleted completed = FindEvent<WaveCompleted>(
                outcome.Change.DomainEvents);
            ArenaRunVictorious victorious = FindEvent<ArenaRunVictorious>(
                outcome.Change.DomainEvents);
            PlayerAttackCompleted attackCompleted = LastEvent<PlayerAttackCompleted>(
                outcome.Change.DomainEvents);

            Assert.That(phaseChanged.AggregateRevision, Is.EqualTo(run.Revision));
            Assert.That(completed.AggregateRevision, Is.EqualTo(run.Revision));
            Assert.That(victorious.AggregateRevision, Is.EqualTo(run.Revision));
            Assert.That(attackCompleted.AggregateRevision, Is.EqualTo(run.Revision));
        }

        [Test]
        public void PlayerAttackCompletedRemainsLastEvent()
        {
            ArenaRun run = StartRunWithLowHealthEnemies(_kit.Waves(0));
            EnemyId bossId = Spawn(run, EnemyId.FromValue(10UL), s_bossSpawn);

            PlayerAttackImpactOutcome outcome = FireRanged(run, bossId);

            IArenaDomainEvent last = outcome.Change.DomainEvents[outcome.Change.DomainEvents.Count - 1];

            Assert.That(last, Is.TypeOf<PlayerAttackCompleted>());
        }

        [Test]
        public void VictoriousRunRejectsFurtherCombatProgression()
        {
            ArenaRun run = StartRunWithLowHealthEnemies(_kit.Waves(0));
            EnemyId bossId = Spawn(run, EnemyId.FromValue(10UL), s_bossSpawn);
            FireRanged(run, bossId);

            PlayerAttackStartOutcome attack = run.StartPlayerAttack();
            EnemySpawnRequestOutcome spawn = run.RequestEnemySpawn();
            WeaponSwitchOutcome switchWeapon = run.SwitchWeapon();
            TimeAdvanceOutcome time = run.AdvanceTime(new GameDuration(1d));

            Assert.That(attack.Status, Is.EqualTo(PlayerAttackStartStatus.RunIsNotPlaying));
            Assert.That(spawn.Status, Is.EqualTo(EnemySpawnRequestStatus.RunIsNotPlaying));
            Assert.That(switchWeapon.Status, Is.EqualTo(WeaponSwitchStatus.RunIsNotPlaying));
            Assert.That(time.Status, Is.EqualTo(TimeAdvanceStatus.RunIsNotPlaying));
        }

        private ArenaRun StartRunWithLowHealthEnemies(WaveCatalog waves)
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

            ArenaRunFactory factory = new ArenaRunFactory();

            return factory.Start(
                ArenaRunId.FromValue(1UL),
                PlayerId.FromValue(2UL),
                _kit.Arena,
                _kit.PlayerAt(Position3D.Zero),
                enemies,
                _kit.Weapons,
                waves);
        }

        private EnemyId Spawn(ArenaRun run, EnemyId enemyId, Position3D position)
        {
            EnemySpawnRequest request = run.RequestEnemySpawn().Request;

            run.ApplyEnemySpawn(new EnemySpawnResolution(
                request.Correlation,
                enemyId,
                position));

            return enemyId;
        }

        private PlayerAttackImpactOutcome FireRanged(ArenaRun run, EnemyId enemyId)
        {
            PlayerAttackStartOutcome start = run.StartPlayerAttack();

            if (start.IsStarted == false)
            {
                run.AdvanceTime(new GameDuration(0.5d));
                run.StartPlayerAttack();
            }

            PlayerAttackImpactRequest request = run.RequestPlayerAttackImpact(Direction3D.Right).Request;

            return run.ApplyPlayerAttackImpact(
                new PlayerAttackImpactResolution(
                    request.Correlation,
                    new[] { enemyId }));
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

        private TEvent LastEvent<TEvent>(IReadOnlyList<IArenaDomainEvent> events)
        {
            IArenaDomainEvent last = events[events.Count - 1];

            if (last is TEvent found)
            {
                return found;
            }

            throw new AssertionException("Last event has unexpected type.");
        }
    }
}
