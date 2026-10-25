using System;
using Game.Arena.Application.Input;
using Game.Arena.Application.Tests.Dispatch;
using Game.Arena.Application.Ticks;
using Game.Arena.Domain.Aggregates.ArenaRun;
using Game.Arena.Domain.Geometry;
using NUnit.Framework;

namespace Game.Arena.Application.Tests.Ticks
{
    [TestFixture]
    public sealed class ArenaRunTickServiceTests
    {
        private TickCoordinatorTestKit _kit;
        private RecordingDispatch _dispatch;
        private ArenaRunTickService _service;

        [SetUp]
        public void SetUp()
        {
            _kit = new TickCoordinatorTestKit();
            _dispatch = new RecordingDispatch();
            _service = CreateService(_kit, _dispatch);
            _kit.Session.StartInitialRun();

            _kit.Input.Input = new PlayerFrameInput(
                MovementAxes.Zero,
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
                MovementAxes.Zero,
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
                CreateService(freshKit, new RecordingDispatch());

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
        public void TickIsDispatchedOnceWithItsResult()
        {
            ArenaRunTickResult result = _service.ExecuteTick();

            Assert.That(_dispatch.Dispatched, Has.Count.EqualTo(1));
            Assert.That(_dispatch.Dispatched[0], Is.SameAs(result));
        }

        [Test]
        public void TickWithoutVictoryProducesNoNotifications()
        {
            ArenaRunTickResult result = _service.ExecuteTick();

            Assert.That(result.Notifications, Is.Empty);
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
                        _kit.Pacing,
                        _kit.RestartDelayPolicy,
                        _kit.SceneReloader,
                        _dispatch);
                });
        }

        private ArenaRunTickService CreateService(
            TickCoordinatorTestKit kit,
            RecordingDispatch dispatch)
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
                    kit.Pacing,
                    kit.RestartDelayPolicy,
                    kit.SceneReloader,
                    dispatch);

            return new ArenaRunTickService(dependencies);
        }

        private void MakeActiveRunDefeated()
        {
            ArenaRun run = _kit.Session.GetRequiredActiveRun();
            run.MakeRunDefeat(reqularsToSpawn: 1);
        }
    }
}
