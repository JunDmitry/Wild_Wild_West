using System;
using Game.Arena.Application.Sessions;
using Game.Arena.Domain.Aggregates.ArenaRun;
using Game.Arena.Domain.Combat;
using Game.Arena.Domain.Configuration;
using Game.Arena.Domain.Geometry;
using Game.Arena.Domain.Identity;
using Game.Arena.Domain.Interactions.Attack;
using Game.Arena.Domain.Interactions.Movement;
using Game.Arena.Domain.Interactions.Spawn;
using Game.Arena.Domain.Movement;
using Game.Arena.Domain.Time;
using Game.Arena.Domain.Vitality;
using NUnit.Framework;

namespace Game.Arena.Application.Tests.Sessions
{
    [TestFixture]
    public sealed class ArenaRunSessionRestartTests
    {
        private TestArenaRunRepository _repository;
        private RecordingArenaRunIdSource _arenaRunIdSource;
        private RecordingPlayerIdSource _playerIdSource;
        private ArenaRunCreationParameters _creationParameters;
        private ArenaRunSession _session;

        [SetUp]
        public void SetUp()
        {
            _repository = new TestArenaRunRepository();
            _arenaRunIdSource = new RecordingArenaRunIdSource();
            _playerIdSource = new RecordingPlayerIdSource();

            ArenaDefinition arena = new(
                new ArenaBounds(-1f, 1f, -1f, 1f, 0f),
                Distance.FromValue(5f));

            PlayerDefinition player = new(
                Position3D.Zero,
                Health.Full(8),
                MovementSpeed.FromUnitsPerSecond(5f),
                CollisionRadius.FromValue(0.5f));

            WeaponCatalog weapons = new(
                new WeaponDefinition(
                    WeaponKind.Ranged,
                    DamageAmount.FromPoints(10),
                    Distance.FromValue(50f),
                    new GameDuration(0.4d),
                    new GameDuration(0d)),
                new WeaponDefinition(
                    WeaponKind.Melee,
                    DamageAmount.FromPoints(20),
                    Distance.FromValue(2f),
                    new GameDuration(0.8d),
                    new GameDuration(0.3d)));

            EnemyCatalog enemies = new(
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

            WaveCatalog waves = new(
                new[]
                {
                    new WaveDefinition(WaveNumber.First, 1),
                });

            _creationParameters = new ArenaRunCreationParameters(
                arena,
                player,
                weapons,
                enemies,
                waves);

            ArenaRunSessionDependencies dependencies = new(
                _repository,
                _arenaRunIdSource,
                _playerIdSource,
                new ArenaRunFactory(new MovementPathPolicy()));

            _session = new ArenaRunSession(dependencies, _creationParameters);
        }

        [Test]
        public void RestartWithoutActiveRunIsRejected()
        {
            DefeatedRunRestartOutcome outcome = _session.RestartDefeatedRun();

            Assert.That(outcome.IsRestarted, Is.False);
            Assert.That(outcome.Status, Is.EqualTo(DefeatedRunRestartStatus.NoActiveRun));
        }

        [Test]
        public void RestartOfPlayingRunIsRejected()
        {
            _session.StartInitialRun();

            DefeatedRunRestartOutcome outcome = _session.RestartDefeatedRun();

            Assert.That(outcome.IsRestarted, Is.False);
            Assert.That(outcome.Status, Is.EqualTo(DefeatedRunRestartStatus.ActiveRunIsNotDefeated));
        }

        [Test]
        public void RestartOfVictoriousRunIsRejected()
        {
            _session.StartInitialRun();
            MakeActiveRunVictorious();

            DefeatedRunRestartOutcome outcome = _session.RestartDefeatedRun();

            Assert.That(outcome.IsRestarted, Is.False);
            Assert.That(outcome.Status, Is.EqualTo(DefeatedRunRestartStatus.ActiveRunIsNotDefeated));
        }

        [Test]
        public void RestartOfDefeatedRunAllocatesNewIdentities()
        {
            _session.StartInitialRun();
            MakeActiveRunDefeated();

            DefeatedRunRestartOutcome outcome = _session.RestartDefeatedRun();

            Assert.That(outcome.IsRestarted, Is.True);
            Assert.That(outcome.ArenaRunId, Is.EqualTo(ArenaRunId.FromValue(2UL)));
            Assert.That(outcome.PlayerId, Is.EqualTo(PlayerId.FromValue(2UL)));
            Assert.That(_arenaRunIdSource.Allocated.Count, Is.EqualTo(2));
            Assert.That(_playerIdSource.Allocated.Count, Is.EqualTo(2));
        }

        [Test]
        public void RestartOfDefeatedRunRemovesOldAggregate()
        {
            InitialRunStartOutcome initial = _session.StartInitialRun();
            MakeActiveRunDefeated();

            _session.RestartDefeatedRun();

            bool oldFound = _repository.TryGet(initial.ArenaRunId, out _);

            Assert.That(oldFound, Is.False);
        }

        [Test]
        public void RestartOfDefeatedRunAddsNewAggregate()
        {
            _session.StartInitialRun();
            MakeActiveRunDefeated();

            DefeatedRunRestartOutcome outcome = _session.RestartDefeatedRun();

            bool newFound = _repository.TryGet(outcome.ArenaRunId, out ArenaRun newRun);

            Assert.That(newFound, Is.True);
            Assert.That(newRun.Id, Is.EqualTo(outcome.ArenaRunId));
            Assert.That(newRun.PlayerId, Is.EqualTo(outcome.PlayerId));
            Assert.That(newRun.Status, Is.EqualTo(ArenaRunStatus.Playing));
            Assert.That(newRun.CurrentWaveNumber, Is.EqualTo(WaveNumber.First));
        }

        [Test]
        public void RestartOfDefeatedRunChangesActiveArenaRunId()
        {
            InitialRunStartOutcome initial = _session.StartInitialRun();
            MakeActiveRunDefeated();

            DefeatedRunRestartOutcome outcome = _session.RestartDefeatedRun();

            Assert.That(_session.ActiveArenaRunId, Is.EqualTo(outcome.ArenaRunId));
            Assert.That(_session.ActiveArenaRunId, Is.Not.EqualTo(initial.ArenaRunId));
        }

        [Test]
        public void RestartFailureDuringAddDoesNotChangeActiveArenaRunId()
        {
            InitialRunStartOutcome initial = _session.StartInitialRun();
            MakeActiveRunDefeated();

            _repository.FailOnAdd = true;

            Assert.Throws<InvalidOperationException>(
                () =>
                {
                    _session.RestartDefeatedRun();
                });

            Assert.That(_session.ActiveArenaRunId, Is.EqualTo(initial.ArenaRunId));
            Assert.That(_repository.TryGet(initial.ArenaRunId, out _), Is.True);
        }

        [Test]
        public void FailedOldRunRemovalCompensatesByRemovingNewRun()
        {
            InitialRunStartOutcome initial = _session.StartInitialRun();
            MakeActiveRunDefeated();

            _repository.NextFail = true;

            Assert.Throws<InvalidOperationException>(
                () =>
                {
                    _session.RestartDefeatedRun();
                });

            Assert.That(_session.ActiveArenaRunId, Is.EqualTo(initial.ArenaRunId));

            ArenaRunId expectedNewId = ArenaRunId.FromValue(2UL);
            bool newRunExistsInRepo = _repository.TryGet(expectedNewId, out _);

            Assert.That(newRunExistsInRepo, Is.False);
        }

        [Test]
        public void AllocatedIdentityIsNotReusedAfterFailure()
        {
            _session.StartInitialRun();
            MakeActiveRunDefeated();

            _repository.FailOnAdd = true;

            Assert.Throws<InvalidOperationException>(
                () =>
                {
                    _session.RestartDefeatedRun();
                });

            _repository.FailOnAdd = false;

            DefeatedRunRestartOutcome outcome = _session.RestartDefeatedRun();

            Assert.That(outcome.ArenaRunId, Is.EqualTo(ArenaRunId.FromValue(3UL)));
            Assert.That(outcome.PlayerId, Is.EqualTo(PlayerId.FromValue(3UL)));
        }

        private void MakeActiveRunDefeated()
        {
            ArenaRun run = _session.GetRequiredActiveRun();
            EnemySpawnRequest request = run.RequestEnemySpawn().Request;
            EnemyId enemyId = EnemyId.FromValue(100UL);

            run.ApplyEnemySpawn(
                new EnemySpawnResolution(
                    request.Correlation,
                    enemyId,
                    new Position3D(2f, 0f, 0f)));

            run.StartEligibleEnemyAttacks();
            run.AdvanceTime(new GameDuration(0.2d));
            run.ResolveDueEnemyAttackImpacts();
        }

        private void MakeActiveRunVictorious()
        {
            ArenaRun run = _session.GetRequiredActiveRun();

            EnemySpawnRequest regularRequest = run.RequestEnemySpawn().Request;
            EnemyId regularId = EnemyId.FromValue(200UL);

            run.ApplyEnemySpawn(
                new EnemySpawnResolution(
                    regularRequest.Correlation,
                    regularId,
                    new Position3D(2f, 0f, 0f)));

            run.StartPlayerAttack();
            PlayerAttackImpactRequest rangedToRegular = run.RequestPlayerAttackImpact(Direction3D.Right).Request;

            run.ApplyPlayerAttackImpact(
                new PlayerAttackImpactResolution(
                    rangedToRegular.Correlation,
                    new[] { regularId }));

            EnemySpawnRequest bossRequest = run.RequestEnemySpawn().Request;
            EnemyId bossId = EnemyId.FromValue(201UL);

            run.ApplyEnemySpawn(
                new EnemySpawnResolution(
                    bossRequest.Correlation,
                    bossId,
                    new Position3D(2f, 0f, 0f)));

            run.AdvanceTime(new GameDuration(0.5d));
            run.StartPlayerAttack();
            PlayerAttackImpactRequest rangedToBoss = run.RequestPlayerAttackImpact(Direction3D.Right).Request;

            run.ApplyPlayerAttackImpact(
                new PlayerAttackImpactResolution(
                    rangedToBoss.Correlation,
                    new[] { bossId }));
        }
    }
}
