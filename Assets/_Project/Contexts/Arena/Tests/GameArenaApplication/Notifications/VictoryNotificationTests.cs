using System;
using Game.Arena.Application.Input;
using Game.Arena.Application.Notifications;
using Game.Arena.Application.Tests.Sessions;
using Game.Arena.Application.Tests.Ticks;
using Game.Arena.Application.Ticks;
using Game.Arena.Domain.Aggregates.ArenaRun;
using Game.Arena.Domain.Geometry;
using Game.Arena.Domain.Identity;
using NUnit.Framework;

namespace Game.Arena.Application.Tests.Notifications
{
    [TestFixture]
    public sealed class VictoryNotificationTests
    {
        private TickCoordinatorTestKit _kit;
        private RecordingNotificationSink _sink;
        private ArenaRunTickService _service;

        [SetUp]
        public void SetUp()
        {
            _kit = new TickCoordinatorTestKit(
                creationParameters: new ArenaRunCreationParametersFactory()
                    .CreateSingleWaveBossRun());
            _sink = new RecordingNotificationSink();
            _service = CreateService();
            _kit.Session.StartInitialRun();

            _kit.Input.Input = new PlayerFrameInput(
                MovementAxes.Zero,
                Direction3D.Right,
                false,
                false);
        }

        [Test]
        public void VictoryPublishesOneVictoryNotification()
        {
            ArenaRun run = _kit.Session.GetRequiredActiveRun();

            _service.ExecuteTick();

            Assert.That(_sink.Published, Is.Empty);
            Assert.That(_kit.EnemyIds.Allocated.Count, Is.EqualTo(1));

            ArenaRunTickResult result = WinTheRun();

            Assert.That(result.Snapshot.Status, Is.EqualTo(ArenaRunStatus.Victory));

            ArenaRunVictoryNotification[] victoryNotifications = VictoryNotifications();

            Assert.That(victoryNotifications, Has.Length.EqualTo(1));

            ArenaRunVictoryNotification notification = victoryNotifications[0];

            Assert.That(notification.ArenaRunId, Is.EqualTo(run.Id));
            Assert.That(notification.Revision, Is.EqualTo(result.FinalRevision));
        }

        [Test]
        public void VictoryNotificationIsNotRepeatedOnLaterTicks()
        {
            _service.ExecuteTick();
            WinTheRun();

            _service.ExecuteTick();

            Assert.That(VictoryNotifications(), Has.Length.EqualTo(1));
        }

        [Test]
        public void SinkFailureFailsTheTickWithTheBuiltResult()
        {
            _service.ExecuteTick();

            InvalidOperationException failure = new("Sink failed.");
            _sink.FailWith = failure;

            ArenaRunTickFailedException exception = Assert.Throws<ArenaRunTickFailedException>(
                () =>
                {
                    WinTheRun();
                });

            Assert.That(exception.InnerException, Is.SameAs(failure));
            Assert.That(
                exception.PartialResult.Snapshot.Status,
                Is.EqualTo(ArenaRunStatus.Victory));
        }

        private ArenaRunTickResult WinTheRun()
        {
            EnemyId bossId = _kit.EnemyIds.Allocated[0];

            _kit.TargetingResolver.SetHits(bossId);
            _kit.Input.Input = new PlayerFrameInput(
                MovementAxes.Zero,
                Direction3D.Right,
                true,
                false);

            return _service.ExecuteTick();
        }

        private ArenaRunVictoryNotification[] VictoryNotifications()
        {
            int count = 0;

            for (int index = 0; index < _sink.Published.Count; index++)
            {
                if (_sink.Published[index] is ArenaRunVictoryNotification)
                {
                    count++;
                }
            }

            ArenaRunVictoryNotification[] found = new ArenaRunVictoryNotification[count];
            int target = 0;

            for (int index = 0; index < _sink.Published.Count; index++)
            {
                if (_sink.Published[index] is ArenaRunVictoryNotification notification)
                {
                    found[target] = notification;
                    target++;
                }
            }

            return found;
        }

        private ArenaRunTickService CreateService()
        {
            ArenaRunTickServiceDependencies dependencies =
                new ArenaRunTickServiceDependencies(
                    _kit.Session,
                    _kit.Clock,
                    _kit.Input,
                    _kit.MovementResolver,
                    _kit.TargetingResolver,
                    _kit.EnemyMovementResolver,
                    _kit.SpawnResolver,
                    _kit.EnemyIds,
                    _kit.Pacing,
                    _kit.RestartDelayPolicy,
                    _sink);

            return new ArenaRunTickService(dependencies);
        }
    }
}
