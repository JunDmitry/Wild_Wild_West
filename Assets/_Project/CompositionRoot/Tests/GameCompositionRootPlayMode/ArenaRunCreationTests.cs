using System.Collections.Generic;
using Game.Arena.Application.Identity;
using Game.Arena.Application.Ports;
using Game.Arena.Application.ReadModels;
using Game.Arena.Application.Sessions;
using Game.Arena.Domain.Aggregates.ArenaRun;
using Game.Arena.Domain.Configuration;
using Game.Arena.Domain.Geometry;
using Game.Arena.Domain.Identity;
using Game.Arena.Domain.Interactions.Spawn;
using Game.Arena.Domain.Movement;
using Game.Arena.Domain.Repositories;
using Game.Arena.Domain.Time;
using Game.Arena.Domain.Vitality;
using NUnit.Framework;

namespace Game.CompositionRoot.PlayMode.Tests
{
    /// <summary>
    /// Proves the initial-run behaviour ADR-0032 defines through the public
    /// session, repository, and entry-point surface: one run on fresh entry,
    /// nothing on a later entry — including after a defeat restart — and exactly
    /// one bootstrap scene load.
    /// </summary>
    [TestFixture]
    public sealed class ArenaRunCreationTests
    {
        private const ulong SeedEnemyId = 1UL;

        [Test]
        public void FreshEntry_CreatesExactlyOneRun()
        {
            ArenaRunSession session = CreateSession(out CountingArenaRunRepository repository);
            ArenaRunStartupDriver driver = new(session);

            driver.Start();

            Assert.That(session.HasActiveRun, Is.True);
            Assert.That(session.ActiveArenaRunId.IsNone, Is.False);
            Assert.That(repository.Count, Is.EqualTo(1));
            Assert.That(repository.TryGet(session.ActiveArenaRunId, out ArenaRun run), Is.True);
            Assert.That(run.PlayerId.IsNone, Is.False);
        }

        [Test]
        public void SecondEntry_WhileARunIsActive_CreatesNothing()
        {
            ArenaRunSession session = CreateSession(out CountingArenaRunRepository repository);
            ArenaRunStartupDriver driver = new(session);

            driver.Start();
            ArenaRunId activeId = session.ActiveArenaRunId;
            Assert.That(repository.TryGet(activeId, out ArenaRun activeRun), Is.True);

            driver.Start();

            Assert.That(repository.Count, Is.EqualTo(1));
            Assert.That(session.ActiveArenaRunId, Is.EqualTo(activeId));
            Assert.That(repository.TryGet(activeId, out ArenaRun sameRun), Is.True);
            Assert.That(sameRun, Is.SameAs(activeRun));
        }

        [Test]
        public void EntryAfterDefeatRestart_WhileTheRestartedRunIsLive_CreatesNothing()
        {
            ArenaRunSession session = CreateSession(
                out CountingArenaRunRepository repository,
                fragilePlayer: true);
            ArenaRunStartupDriver driver = new(session);

            driver.Start();
            ArenaRunId initialId = session.ActiveArenaRunId;

            DefeatActiveRun(session, repository);

            DefeatedRunRestartOutcome restart = session.RestartDefeatedRun();

            Assert.That(restart.IsRestarted, Is.True);
            ArenaRunId restartedId = session.ActiveArenaRunId;
            Assert.That(restartedId, Is.Not.EqualTo(initialId));
            Assert.That(repository.Count, Is.EqualTo(1));

            driver.Start();

            Assert.That(session.ActiveArenaRunId, Is.EqualTo(restartedId));
            Assert.That(repository.Count, Is.EqualTo(1));
            Assert.That(repository.TryGet(restartedId, out ArenaRun restartedRun), Is.True);
            Assert.That(restartedRun.Status, Is.EqualTo(ArenaRunStatus.Playing));
            Assert.That(repository.TryGet(initialId, out _), Is.False);
        }

        [Test]
        public void BootstrapEntryPoint_IssuesExactlyOneReload()
        {
            RecordingArenaSceneReloader reloader = new();
            ArenaBootstrapEntryPoint entryPoint = new(reloader);

            entryPoint.Start();

            Assert.That(reloader.ReloadCount, Is.EqualTo(1));
        }

        private static ArenaRunSession CreateSession(
            out CountingArenaRunRepository repository,
            bool fragilePlayer = false)
        {
            repository = new CountingArenaRunRepository();

            ArenaRunCreationParameters defaults =
                ArenaGameConfiguration.CreateDefault().CreationParameters;

            ArenaRunCreationParameters parameters = fragilePlayer
                ? CreateFragileParameters(defaults)
                : defaults;

            ArenaRunSessionDependencies dependencies = new(
                repository,
                new MonotonicArenaRunIdSource(),
                new MonotonicPlayerIdSource(),
                new ArenaRunFactory(new MovementPathPolicy()),
                new ArenaRunSnapshotMapper());

            return new ArenaRunSession(dependencies, parameters);
        }

        /// <summary>
        /// A one-point player standing next to the spawn band, so a single enemy
        /// attack reaches Defeat without simulating a whole encounter. Only the
        /// configuration values differ; every production type is unchanged.
        /// </summary>
        private static ArenaRunCreationParameters CreateFragileParameters(
            ArenaRunCreationParameters defaults)
        {
            PlayerDefinition fragilePlayer = new(
                new Position3D(19f, 0f, 0f),
                Health.Full(1),
                defaults.PlayerDefinition.MovementSpeed,
                defaults.PlayerDefinition.CollisionRadius);

            return new ArenaRunCreationParameters(
                defaults.ArenaDefinition,
                fragilePlayer,
                defaults.WeaponCatalog,
                defaults.EnemyCatalog,
                defaults.WaveCatalog);
        }

        private static void DefeatActiveRun(
            ArenaRunSession session,
            CountingArenaRunRepository repository)
        {
            Assert.That(repository.TryGet(session.ActiveArenaRunId, out ArenaRun run), Is.True);

            EnemySpawnRequestOutcome spawnRequest = run.RequestEnemySpawn();
            Assert.That(spawnRequest.HasRequest, Is.True);

            EnemySpawnResolutionOutcome spawn = run.ApplyEnemySpawn(
                new EnemySpawnResolution(
                    spawnRequest.Request.Correlation,
                    EnemyId.FromValue(SeedEnemyId),
                    new Position3D(20.5f, 0f, 0f)));

            Assert.That(spawn.IsSpawned, Is.True);

            run.StartEligibleEnemyAttacks();
            run.AdvanceTime(new GameDuration(0.5d));
            run.ResolveDueEnemyAttackImpacts();

            Assert.That(run.Status, Is.EqualTo(ArenaRunStatus.Defeat));
        }

        private sealed class CountingArenaRunRepository : IArenaRunRepository
        {
            private readonly Dictionary<ArenaRunId, ArenaRun> _runs = new();

            public int Count => _runs.Count;

            public void Add(ArenaRun arenaRun)
            {
                _runs.Add(arenaRun.Id, arenaRun);
            }

            public bool TryGet(ArenaRunId id, out ArenaRun arenaRun)
            {
                return _runs.TryGetValue(id, out arenaRun);
            }

            public bool Remove(ArenaRunId id)
            {
                return _runs.Remove(id);
            }
        }

        private sealed class RecordingArenaSceneReloader : IArenaSceneReloader
        {
            public int ReloadCount { get; private set; }

            public void ReloadArenaScene()
            {
                ReloadCount++;
            }
        }
    }
}
