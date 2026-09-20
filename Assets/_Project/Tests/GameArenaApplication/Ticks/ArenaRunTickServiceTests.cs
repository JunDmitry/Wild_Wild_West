using System;
using Game.Arena.Application.Input;
using Game.Arena.Application.Ticks;
using Game.Arena.Application.Ticks.Stages;
using Game.Arena.Domain.Aggregates.ArenaRun;
using Game.Arena.Domain.Geometry;
using Game.Arena.Domain.Identity;
using Game.Arena.Domain.Interactions.Movement;
using Game.Arena.Domain.Interactions.Spawn;
using Game.Arena.Domain.Time;
using NUnit.Framework;

namespace Game.Arena.Application.Tests.Ticks
{
    [TestFixture]
    public sealed class ArenaRunTickServiceTests
    {
        private TickCoordinatorTestKit _kit;
        private ArenaRunTickService _service;

        [SetUp]
        public void SetUp()
        {
            _kit = new TickCoordinatorTestKit();
            _service = CreateService(_kit);
            _kit.Session.StartInitialRun();

            _kit.Input.Input = new PlayerFrameInput(
                MovementInput.Zero,
                Direction3D.Right,
                false,
                false);
        }

        [Test]
        public void ServiceReturnsTickResultOnSuccess()
        {
            ArenaRunTickResult result = _service.ExecuteTick();

            Assert.That(
                result.ArenaRunId,
                Is.EqualTo(_kit.Session.ActiveArenaRunId));
        }

        [Test]
        public void ServicePropagatesTickFailureWithPartialResult()
        {
            _kit.TargetingResolver.ReturnNull = true;

            _kit.Input.Input = new PlayerFrameInput(
                MovementInput.Zero,
                Direction3D.Right,
                true,
                false);

            ArenaRunTickFailedException exception =
                Assert.Throws<ArenaRunTickFailedException>(
                    () =>
                    {
                        _service.ExecuteTick();
                    });

            Assert.That(
                exception.PartialResult.ArenaRunId,
                Is.EqualTo(_kit.Session.ActiveArenaRunId));

            Assert.That(
                exception.PartialResult.DomainEvents.Count,
                Is.GreaterThan(0));
        }

        [Test]
        public void ServiceWithoutActiveRunThrowsInvalidOperation()
        {
            TickCoordinatorTestKit freshKit =
                new TickCoordinatorTestKit();

            ArenaRunTickService freshService =
                CreateService(freshKit);

            Assert.Throws<InvalidOperationException>(
                () =>
                {
                    freshService.ExecuteTick();
                });
        }

        [Test]
        public void PacingIsResetWhenActiveRunChanges()
        {
            _service.ExecuteTick();

            Assert.That(_kit.EnemyIds.Allocated.Count, Is.EqualTo(1));

            MakeActiveRunDefeated();
            _kit.Session.RestartDefeatedRun();

            ArenaRunTickResult result = _service.ExecuteTick();

            Assert.That(_kit.EnemyIds.Allocated.Count, Is.EqualTo(2));

            Assert.That(
                result.ExecutedStages,
                Does.Contain(ArenaRunTickStage.EnemySpawn));
        }

        [Test]
        public void PacingIsNotResetWithinSameRun()
        {
            _service.ExecuteTick();
            _service.ExecuteTick();

            Assert.That(
                _kit.EnemyIds.Allocated.Count,
                Is.EqualTo(1));
        }

        [Test]
        public void ServiceDependenciesRejectNullPorts()
        {
            Assert.Throws<ArgumentNullException>(
                () =>
                {
                    _ = new ArenaRunTickServiceDependencies(
                        _kit.Session,
                        _kit.Clock,
                        _kit.Input,
                        null,
                        _kit.TargetingResolver,
                        _kit.EnemyMovementResolver,
                        _kit.SpawnResolver,
                        _kit.EnemyIds,
                        _kit.Pacing);
                });
        }

        private ArenaRunTickService CreateService(
            TickCoordinatorTestKit kit)
        {
            ArenaRunTickServiceDependencies dependencies =
                new ArenaRunTickServiceDependencies(
                    kit.Session,
                    kit.Clock,
                    kit.Input,
                    kit.MovementResolver,
                    kit.TargetingResolver,
                    kit.EnemyMovementResolver,
                    kit.SpawnResolver,
                    kit.EnemyIds,
                    kit.Pacing);

            return new ArenaRunTickService(dependencies);
        }

        private void MakeActiveRunDefeated()
        {
            ArenaRun run = _kit.Session.GetRequiredActiveRun();

            EnemySpawnRequestOutcome requestOutcome = run.RequestEnemySpawn();

            if (requestOutcome.HasRequest)
            {
                EnemySpawnRequest request = requestOutcome.Request;
                EnemyId enemyId = EnemyId.FromValue(900UL);

                run.ApplyEnemySpawn(
                    new EnemySpawnResolution(
                        request.Correlation,
                        enemyId,
                        new Position3D(2f, 0f, 0f)));
            }

            int infinityCycleGuard = 1000;

            while (infinityCycleGuard > 0 && run.PlayerHealth.IsDepleted == false)
            {
                Domain.Interactions.Movement.EnemyMovementBatchRequestOutcome reqOutcome = run.RequestEnemyMovementBatch(new GameDuration(1d));

                if (reqOutcome.HasRequest)
                {
                    Domain.Interactions.Movement.EnemyMovementBatchRequest req = reqOutcome.Request;
                    run.ApplyEnemyMovementBatch(new EnemyMovementBatchResolution(req.Correlation, new EnemyMovementBatchResolutionEntry[] { new(req.Intents[0].EnemyId, req.Intents[0].Intent.RequestedPosition) }));
                }

                run.StartEligibleEnemyAttacks();
                run.AdvanceTime(new GameDuration(0.2d));
                run.ResolveDueEnemyAttackImpacts();
                infinityCycleGuard--;
            }
        }
    }
}
