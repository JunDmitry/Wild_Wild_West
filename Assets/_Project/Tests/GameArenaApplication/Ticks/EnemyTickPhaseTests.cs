using System;
using Game.Arena.Application.Spawning;
using Game.Arena.Application.Tests.Sessions;
using Game.Arena.Application.Ticks;
using Game.Arena.Application.Ticks.Stages;
using Game.Arena.Domain.Aggregates.ArenaRun;
using Game.Arena.Domain.Concurrency;
using Game.Arena.Domain.Events;
using Game.Arena.Domain.Geometry;
using Game.Arena.Domain.Identity;
using Game.Arena.Domain.Time;
using NUnit.Framework;

namespace Game.Arena.Application.Tests.Ticks
{
    [TestFixture]
    public sealed class EnemyTickPhaseTests
    {
        private static readonly GameDuration s_delta = new(0.1d);

        private TickTestKit _kit;
        private ScriptedEnemyMovementResolver _movement;
        private ScriptedSpawnPlacementResolver _spawnPlacement;
        private RecordingEnemyIdSource _enemyIdSource;
        private FixedIntervalSpawnPacingPolicy _pacing;
        private EnemyTickPhase _phase;
        private ArenaRunTickRecorder _recorder;

        [SetUp]
        public void SetUp()
        {
            _kit = new TickTestKit();
            _movement = new ScriptedEnemyMovementResolver();
            _spawnPlacement = new ScriptedSpawnPlacementResolver
            {
                Position = new(3f, 0f, 0f)
            };

            _enemyIdSource = new RecordingEnemyIdSource();
            _pacing = new FixedIntervalSpawnPacingPolicy(new GameDuration(1d));
            _recorder = new ArenaRunTickRecorder();

            _phase = new EnemyTickPhase(
                _movement,
                _spawnPlacement,
                _enemyIdSource,
                _pacing,
                new PendingInteractionTracker());
        }

        [Test]
        public void SpawnAllocatesIdentityOnlyAfterDomainRequest()
        {
            ArenaRun run = _kit.StartCompactRun(100, 1);

            _phase.Execute(run, s_delta, _recorder);

            Assert.That(_spawnPlacement.CallCount, Is.EqualTo(1));
            Assert.That(_enemyIdSource.Allocated.Count, Is.EqualTo(1));
            Assert.That(run.ActiveEnemyCount, Is.EqualTo(1));
        }

        [Test]
        public void SpawnIsSkippedWhenPacingPolicyBlocks()
        {
            ArenaRun run = _kit.StartCompactRun(100, 2);

            _phase.Execute(run, s_delta, _recorder);
            run.AdvanceTime(new GameDuration(0.2d));
            _phase.Execute(run, s_delta, _recorder);

            Assert.That(run.ActiveEnemyCount, Is.EqualTo(1));
            Assert.That(_enemyIdSource.Allocated.Count, Is.EqualTo(1));
        }

        [Test]
        public void SpawnResumesAfterIntervalElapsed()
        {
            ArenaRun run = _kit.StartCompactRun(100, 2);

            _phase.Execute(run, s_delta, _recorder);
            run.AdvanceTime(new GameDuration(1.5d));
            _phase.Execute(run, s_delta, _recorder);

            Assert.That(run.ActiveEnemyCount, Is.EqualTo(2));
        }

        [Test]
        public void SpawnedEnemyDoesNotActInTheSameTick()
        {
            ArenaRun run = _kit.StartCompactRun(100, 1);

            _phase.Execute(run, s_delta, _recorder);
            ArenaRunTickResult result = _recorder.Build(run.Id, run.Revision);

            Assert.That(run.PendingEnemyAttackCount, Is.EqualTo(0));
            Assert.That(run.PlayerHealth.Current, Is.EqualTo(run.PlayerHealth.Maximum));
            Assert.That(result.DomainEvents[result.DomainEvents.Count - 1], Is.TypeOf<EnemySpawned>());
        }

        [Test]
        public void EnemyMovementPrecedesEnemyAttackStart()
        {
            ArenaRun run = _kit.StartCompactRun(100, 1);
            _kit.Spawn(run, 10UL, new Position3D(5f, 0f, 0f));
            run.AdvanceTime(new GameDuration(2d));

            _phase.Execute(run, s_delta, _recorder);
            ArenaRunTickResult result = _recorder.Build(run.Id, run.Revision);

            Assert.That(_movement.CallCount, Is.EqualTo(1));
            Assert.That(result.ExecutedStages[0], Is.EqualTo(ArenaRunTickStage.EnemyMovement));
            Assert.That(result.ExecutedStages[1], Is.EqualTo(ArenaRunTickStage.EnemyAttackStart));
            Assert.That(result.ExecutedStages[2], Is.EqualTo(ArenaRunTickStage.EnemyAttackImpact));
        }

        [Test]
        public void EnemyReachingRangeStartsAttackInSameTick()
        {
            ArenaRun run = _kit.StartCompactRun(100, 1);
            _kit.Spawn(run, 10UL, new Position3D(3.5f, 0f, 0f));
            run.AdvanceTime(new GameDuration(2d));

            _phase.Execute(run, new GameDuration(1d), _recorder);

            Assert.That(run.PendingEnemyAttackCount, Is.EqualTo(1));
        }

        [Test]
        public void EnemyInRangeDoesNotReceiveMovementIntent()
        {
            ArenaRun run = _kit.StartCompactRun(100, 1);
            _kit.Spawn(run, 10UL, new Position3D(2f, 0f, 0f));

            _phase.Execute(run, s_delta, _recorder);

            Assert.That(_movement.CallCount, Is.EqualTo(0));
            Assert.That(run.PendingEnemyAttackCount, Is.EqualTo(1));
        }

        [Test]
        public void DueEnemyAttackDamagesPlayer()
        {
            ArenaRun run = _kit.StartCompactRun(100, 1);
            _kit.Spawn(run, 10UL, new Position3D(2f, 0f, 0f));

            _phase.Execute(run, s_delta, _recorder);
            run.AdvanceTime(new GameDuration(0.3d));
            _phase.Execute(run, s_delta, _recorder);

            Assert.That(run.PlayerHealth.Current, Is.LessThan(run.PlayerHealth.Maximum));
        }

        [Test]
        public void SpawnIsSkippedWhenPlayerIsDefeatedDuringImpacts()
        {
            ArenaRun run = _kit.StartCompactRun(5, 2);
            _kit.Spawn(run, 10UL, new Position3D(2f, 0f, 0f));
            run.StartEligibleEnemyAttacks();
            run.AdvanceTime(new GameDuration(0.3d));
            int enemyCountBeforePhase = run.ActiveEnemyCount;

            _phase.Execute(run, s_delta, _recorder);

            Assert.That(run.Status, Is.EqualTo(ArenaRunStatus.Defeat));
            Assert.That(run.ActiveEnemyCount, Is.EqualTo(enemyCountBeforePhase));
            Assert.That(_spawnPlacement.CallCount, Is.EqualTo(0));
        }

        [Test]
        public void TerminalRunSkipsEnemyStages()
        {
            ArenaRun run = _kit.StartCompactRun(5, 1);
            _kit.DefeatPlayer(run);
            AggregateRevision revisionAfterDefeat = run.Revision;

            StageExecutionStatus status = _phase.Execute(run, s_delta, _recorder);
            ArenaRunTickResult result = _recorder.Build(run.Id, run.Revision);

            Assert.That(status, Is.EqualTo(StageExecutionStatus.Completed));
            Assert.That(result.ExecutedStages, Is.Empty);
            Assert.That(run.Revision, Is.EqualTo(revisionAfterDefeat));
            Assert.That(_movement.CallCount, Is.EqualTo(0));
        }

        [Test]
        public void IncompleteMovementBatchLeavesInteractionPendingAndStopsPhase()
        {
            ArenaRun run = _kit.StartCompactRun(100, 1);
            _kit.Spawn(run, 10UL, new Position3D(5f, 0f, 0f));
            _movement.Mode = ScriptedEnemyMovementResolver.ResolveMode.OmitFirstEntry;

            StageExecutionStatus status = _phase.Execute(run, s_delta, _recorder);

            Assert.That(status, Is.EqualTo(StageExecutionStatus.InteractionLeftPending));
            Assert.That(run.HasPendingInteraction, Is.True);
            Assert.That(run.PendingEnemyAttackCount, Is.EqualTo(0));
            Assert.That(_spawnPlacement.CallCount, Is.EqualTo(0));
        }

        [Test]
        public void NullMovementResultThrows()
        {
            ArenaRun run = _kit.StartCompactRun(100, 1);
            _kit.Spawn(run, 10UL, new Position3D(5f, 0f, 0f));
            _movement.Mode = ScriptedEnemyMovementResolver.ResolveMode.ReturnNull;

            Assert.Throws<InvalidOperationException>(
                () =>
                {
                    _phase.Execute(run, s_delta, _recorder);
                });

            Assert.That(run.HasPendingInteraction, Is.True);
        }

        [Test]
        public void BlockedEnemyMovementKeepsPositionsAndClosesInteraction()
        {
            ArenaRun run = _kit.StartCompactRun(100, 1);
            EnemyId enemyId = _kit.Spawn(run, 10UL, new Position3D(5f, 0f, 0f));
            _movement.Mode = ScriptedEnemyMovementResolver.ResolveMode.KeepInPlace;

            _phase.Execute(run, s_delta, _recorder);

            run.TryGetEnemyPosition(enemyId, out Position3D position);

            Assert.That(position, Is.EqualTo(new Position3D(5f, 0f, 0f)));
            Assert.That(run.HasPendingInteraction, Is.False);
        }

        [Test]
        public void RejectedEnemyMovementStopsRemainingEnemyStages()
        {
            ArenaRun run = _kit.StartCompactRun(100, 1);
            _kit.Spawn(run, 10UL, new Position3D(5f, 0f, 0f));

            _movement.Mode = ScriptedEnemyMovementResolver.ResolveMode.OmitFirstEntry;

            StageExecutionStatus status = _phase.Execute(run, s_delta, _recorder);
            ArenaRunTickResult result = _recorder.Build(run.Id, run.Revision);

            Assert.That(status, Is.EqualTo(StageExecutionStatus.InteractionLeftPending));
            Assert.That(run.HasPendingInteraction, Is.True);
            Assert.That(result.ExecutedStages, Does.Not.Contain(ArenaRunTickStage.EnemyAttackStart.ToString()));
            Assert.That(result.ExecutedStages, Does.Not.Contain(ArenaRunTickStage.EnemyAttackImpact.ToString()));
            Assert.That(result.ExecutedStages, Does.Not.Contain(ArenaRunTickStage.EnemySpawn.ToString()));
        }
    }
}
