using System;
using System.Collections.Generic;
using Game.Arena.Application.Input;
using Game.Arena.Application.Notifications;
using Game.Arena.Application.Tests.Dispatch;
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
        private RecordingDispatch _dispatch;
        private ArenaRunTickService _service;

        [SetUp]
        public void SetUp()
        {
            _kit = new TickCoordinatorTestKit(
                creationParameters: new ArenaRunCreationParametersFactory()
                    .CreateSingleWaveBossRun());
            _dispatch = new RecordingDispatch();
            _service = CreateService();
            _kit.Session.StartInitialRun();

            _kit.Input.Input = new PlayerFrameInput(
                MovementAxes.Zero,
                Direction3D.Right,
                false,
                false);
        }

        [Test]
        public void VictoryProducesOneVictoryNotification()
        {
            ArenaRun run = _kit.Session.GetRequiredActiveRun();

            ArenaRunTickResult first = _service.ExecuteTick();

            Assert.That(first.Notifications, Is.Empty);
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
        public void DispatchFailureFailsTheTickWithTheBuiltResult()
        {
            _service.ExecuteTick();

            InvalidOperationException failure = new("Dispatch failed.");
            _dispatch.FailWith = failure;

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

            for (int resultIndex = 0; resultIndex < _dispatch.Dispatched.Count; resultIndex++)
            {
                count += CountVictory(_dispatch.Dispatched[resultIndex]);
            }

            ArenaRunVictoryNotification[] found = new ArenaRunVictoryNotification[count];
            int target = 0;

            for (int resultIndex = 0; resultIndex < _dispatch.Dispatched.Count; resultIndex++)
            {
                IReadOnlyList<IArenaRunNotification> notifications =
                    _dispatch.Dispatched[resultIndex].Notifications;

                for (int index = 0; index < notifications.Count; index++)
                {
                    if (notifications[index] is ArenaRunVictoryNotification notification)
                    {
                        found[target] = notification;
                        target++;
                    }
                }
            }

            return found;
        }

        private static int CountVictory(ArenaRunTickResult result)
        {
            int count = 0;

            for (int index = 0; index < result.Notifications.Count; index++)
            {
                if (result.Notifications[index] is ArenaRunVictoryNotification)
                {
                    count++;
                }
            }

            return count;
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
                    _dispatch);

            return new ArenaRunTickService(dependencies);
        }
    }
}
