using System.Collections.Generic;
using Game.Arena.Application.Dispatch;
using Game.Arena.Application.Notifications;
using Game.Arena.Application.ReadModels;
using Game.Arena.Application.Ticks;
using Game.Arena.Domain.Geometry;
using Game.Arena.Domain.Identity;
using Game.Arena.Infrastructure.Targeting;
using NUnit.Framework;
using UnityEngine;

namespace Game.Arena.PlayMode.Tests
{
    [TestFixture]
    public sealed class DispatchPipelineOrderTests
    {
        private const int MaxTicks = 10;

        private readonly List<GameObject> _objects = new();
        private readonly List<string> _calls = new();
        private TestEnemyColliderRegistry _registry;
        private RecordingSnapshotHandler _snapshotHandler;
        private RecordingDamagedHandler _notificationHandler;
        private PlayModeTickHarness _harness;

        [SetUp]
        public void SetUp()
        {
            _objects.Clear();
            _calls.Clear();

            _registry = new TestEnemyColliderRegistry();
            _snapshotHandler = new RecordingSnapshotHandler(_calls);
            _notificationHandler = new RecordingDamagedHandler(_calls);

            ArenaRunResultDispatcher dispatcher = new();
            dispatcher.RegisterSnapshotHandler(
                ArenaRunDispatchStage.EntityViewSynchronization,
                _snapshotHandler);
            dispatcher.RegisterNotificationHandler<ArenaRunEnemyDamagedNotification>(
                ArenaRunDispatchStage.GameplayVisual,
                _notificationHandler);

            _harness = new PlayModeTickHarness(_registry, dispatcher: dispatcher);
        }

        [TearDown]
        public void TearDown()
        {
            for (int index = 0; index < _objects.Count; index++)
            {
                Object.DestroyImmediate(_objects[index]);
            }

            _objects.Clear();
        }

        [Test]
        public void OneRealTick_Dispatch_ArrivesSnapshotBeforeNotification()
        {
            EnemyId enemyId = _harness.SeedEnemy(1UL, new Position3D(0f, 0f, 21f));
            _registry.Register(CreateCollider("Enemy", new Vector3(0f, 0f, 21f)), enemyId);

            ArenaRunTickResult damagingTick = null;

            for (int index = 0; index < MaxTicks; index++)
            {
                _calls.Clear();
                _snapshotHandler.Clear();
                _notificationHandler.Clear();

                damagingTick = index == 0
                    ? _harness.Tick(true, Direction3D.Forward)
                    : _harness.TickIdle();

                if (_notificationHandler.NotificationCount > 0)
                {
                    break;
                }
            }

            Assert.That(_notificationHandler.NotificationCount, Is.EqualTo(1));
            Assert.That(damagingTick, Is.Not.Null);

            ArenaRunEnemyDamagedNotification expected = null;

            for (int index = 0; index < damagingTick.Notifications.Count; index++)
            {
                if (damagingTick.Notifications[index] is ArenaRunEnemyDamagedNotification damaged)
                {
                    expected = damaged;
                }
            }

            Assert.That(expected, Is.Not.Null);
            Assert.That(_calls, Is.EqualTo(new[] { "snapshot", "notification" }));
            Assert.That(_snapshotHandler.LastSnapshot, Is.SameAs(damagingTick.Snapshot));
            Assert.That(_notificationHandler.LastNotification, Is.SameAs(expected));
            Assert.That(_notificationHandler.LastNotification.EnemyId, Is.EqualTo(enemyId));
        }

        private GameObject CreateCollider(string name, Vector3 position)
        {
            GameObject gameObject = new(name);
            gameObject.transform.position = position;

            BoxCollider collider = gameObject.AddComponent<BoxCollider>();
            collider.size = new Vector3(1f, 2f, 1f);

            _objects.Add(gameObject);
            Physics.SyncTransforms();

            return gameObject;
        }

        private sealed class RecordingSnapshotHandler : IArenaRunSnapshotHandler
        {
            private readonly List<string> _calls;

            public RecordingSnapshotHandler(List<string> calls)
            {
                _calls = calls;
            }

            public ArenaRunSnapshot LastSnapshot { get; private set; }

            public void Handle(ArenaRunSnapshot snapshot)
            {
                _calls.Add("snapshot");
                LastSnapshot = snapshot;
            }

            public void Clear()
            {
                LastSnapshot = null;
            }
        }

        private sealed class RecordingDamagedHandler : IArenaRunNotificationHandler<ArenaRunEnemyDamagedNotification>
        {
            private readonly List<string> _calls;

            public RecordingDamagedHandler(List<string> calls)
            {
                _calls = calls;
            }

            public ArenaRunEnemyDamagedNotification LastNotification { get; private set; }

            public int NotificationCount { get; private set; }

            public void Handle(ArenaRunEnemyDamagedNotification notification)
            {
                _calls.Add("notification");
                LastNotification = notification;
                NotificationCount++;
            }

            public void Clear()
            {
                LastNotification = null;
                NotificationCount = 0;
            }
        }

        private sealed class TestEnemyColliderRegistry : IEnemyColliderRegistry
        {
            private readonly Dictionary<Collider, EnemyId> _enemyIds = new();

            public void Register(Collider collider, EnemyId enemyId)
            {
                _enemyIds.Add(collider, enemyId);
            }

            public bool TryGetEnemyId(Collider collider, out EnemyId enemyId)
            {
                if (collider == null)
                {
                    enemyId = EnemyId.None;
                    return false;
                }

                return _enemyIds.TryGetValue(collider, out enemyId);
            }
        }
    }
}
