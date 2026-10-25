using System;
using System.Collections.Generic;
using Game.Arena.Application.Dispatch;
using Game.Arena.Application.Notifications;
using Game.Arena.Application.ReadModels;
using Game.Arena.Application.Ticks;
using Game.Arena.Domain.Aggregates.ArenaRun;
using Game.Arena.Domain.Combat;
using Game.Arena.Domain.Concurrency;
using Game.Arena.Domain.Events;
using Game.Arena.Domain.Identity;
using Game.Arena.Domain.Vitality;
using NUnit.Framework;

namespace Game.Arena.Application.Tests.Dispatch
{
    [TestFixture]
    public sealed class ArenaRunResultDispatcherTests
    {
        private static readonly ArenaRunId RunId = ArenaRunId.FromValue(1UL);
        private static readonly AggregateRevision Revision = AggregateRevision.Initial.Next();

        [Test]
        public void SnapshotStagesReceiveTheResultSnapshot()
        {
            RecordingSnapshotHandler published = new();
            RecordingSnapshotHandler synchronized = new();
            ArenaRunResultDispatcher dispatcher = new();

            dispatcher.RegisterSnapshotHandler(
                ArenaRunDispatchStage.SnapshotPublication,
                published);
            dispatcher.RegisterSnapshotHandler(
                ArenaRunDispatchStage.EntityViewSynchronization,
                synchronized);

            ArenaRunTickResult result = CreateResult();

            dispatcher.Dispatch(result);

            Assert.That(published.Snapshots, Has.Count.EqualTo(1));
            Assert.That(synchronized.Snapshots, Has.Count.EqualTo(1));
            Assert.That(published.Snapshots[0], Is.SameAs(result.Snapshot));
            Assert.That(synchronized.Snapshots[0], Is.SameAs(result.Snapshot));
        }

        [Test]
        public void StagesRunInPipelineOrder()
        {
            List<string> order = new();
            ArenaRunResultDispatcher dispatcher = new();

            dispatcher.RegisterSnapshotHandler(
                ArenaRunDispatchStage.SnapshotPublication,
                new RecordingSnapshotHandler(_ => order.Add("snapshot-publication")));
            dispatcher.RegisterSnapshotHandler(
                ArenaRunDispatchStage.EntityViewSynchronization,
                new RecordingSnapshotHandler(_ => order.Add("entity-view")));
            dispatcher.RegisterNotificationHandler(
                ArenaRunDispatchStage.GameplayVisual,
                new RecordingNotificationHandler<ArenaRunEnemyDamagedNotification>(
                    _ => order.Add("gameplay")));
            dispatcher.RegisterNotificationHandler(
                ArenaRunDispatchStage.Hud,
                new RecordingNotificationHandler<ArenaRunEnemyDamagedNotification>(
                    _ => order.Add("hud")));
            dispatcher.RegisterNotificationHandler(
                ArenaRunDispatchStage.AudioAndVisualEffects,
                new RecordingNotificationHandler<ArenaRunEnemyDamagedNotification>(
                    _ => order.Add("audio")));
            dispatcher.RegisterNotificationHandler(
                ArenaRunDispatchStage.AnalyticsAndDiagnostics,
                new RecordingNotificationHandler<ArenaRunEnemyDamagedNotification>(
                    _ => order.Add("analytics")));

            dispatcher.Dispatch(CreateResult(
                CreateDamagedEvent(EnemyId.FromValue(1UL))));

            Assert.That(
                order,
                Is.EqualTo(new[]
                {
                    "snapshot-publication",
                    "entity-view",
                    "gameplay",
                    "hud",
                    "audio",
                    "analytics",
                }));
        }

        [Test]
        public void WithinAStageNotificationsFollowPublicationOrderAndHandlersRegistrationOrder()
        {
            EnemyId firstEnemy = EnemyId.FromValue(1UL);
            EnemyId secondEnemy = EnemyId.FromValue(2UL);
            List<string> order = new();
            ArenaRunResultDispatcher dispatcher = new();

            dispatcher.RegisterNotificationHandler(
                ArenaRunDispatchStage.GameplayVisual,
                new RecordingNotificationHandler<ArenaRunEnemyDamagedNotification>(
                    notification => order.Add("first:" + Name(notification.EnemyId))));
            dispatcher.RegisterNotificationHandler(
                ArenaRunDispatchStage.GameplayVisual,
                new RecordingNotificationHandler<ArenaRunEnemyDamagedNotification>(
                    notification => order.Add("second:" + Name(notification.EnemyId))));

            dispatcher.Dispatch(CreateResult(
                CreateDamagedEvent(firstEnemy),
                CreateDamagedEvent(secondEnemy)));

            Assert.That(
                order,
                Is.EqualTo(new[]
                {
                    "first:" + Name(firstEnemy),
                    "second:" + Name(firstEnemy),
                    "first:" + Name(secondEnemy),
                    "second:" + Name(secondEnemy),
                }));
        }

        [Test]
        public void HandlerRegisteredForBaseInterfaceReceivesEveryNotificationType()
        {
            RecordingNotificationHandler<IArenaRunNotification> handler = new();
            ArenaRunResultDispatcher dispatcher = new();

            dispatcher.RegisterNotificationHandler(
                ArenaRunDispatchStage.AnalyticsAndDiagnostics,
                handler);

            dispatcher.Dispatch(CreateResult(
                CreateDamagedEvent(EnemyId.FromValue(1UL)),
                new ArenaRunVictorious(RunId, Revision)));

            Assert.That(handler.Notifications, Has.Count.EqualTo(2));
            Assert.That(handler.Notifications[0], Is.TypeOf<ArenaRunEnemyDamagedNotification>());
            Assert.That(handler.Notifications[1], Is.TypeOf<ArenaRunVictoryNotification>());
        }

        [Test]
        public void NotificationWithoutMatchingHandlerIsIgnored()
        {
            RecordingNotificationHandler<ArenaRunVictoryNotification> handler = new();
            ArenaRunResultDispatcher dispatcher = new();

            dispatcher.RegisterNotificationHandler(
                ArenaRunDispatchStage.GameplayVisual,
                handler);

            Assert.DoesNotThrow(
                () => dispatcher.Dispatch(CreateResult(
                    CreateDamagedEvent(EnemyId.FromValue(1UL)))));

            Assert.That(handler.Notifications, Is.Empty);
        }

        [Test]
        public void DispatchWithoutHandlersIsANoOp()
        {
            ArenaRunResultDispatcher dispatcher = new();

            Assert.DoesNotThrow(() => dispatcher.Dispatch(CreateResult()));
        }

        [Test]
        public void HandlerFailureIsReportedAndDoesNotStopLaterHandlersOrStages()
        {
            InvalidOperationException failure = new("Handler failed.");
            RecordingDiagnostics diagnostics = new();
            List<string> order = new();
            ArenaRunResultDispatcher dispatcher = new(diagnostics);

            dispatcher.RegisterNotificationHandler(
                ArenaRunDispatchStage.GameplayVisual,
                new RecordingNotificationHandler<ArenaRunEnemyDamagedNotification>(
                    _ =>
                    {
                        order.Add("failing");
                        throw failure;
                    }));
            dispatcher.RegisterNotificationHandler(
                ArenaRunDispatchStage.GameplayVisual,
                new RecordingNotificationHandler<ArenaRunEnemyDamagedNotification>(
                    _ => order.Add("after")));
            dispatcher.RegisterNotificationHandler(
                ArenaRunDispatchStage.Hud,
                new RecordingNotificationHandler<ArenaRunEnemyDamagedNotification>(
                    _ => order.Add("hud")));

            Assert.DoesNotThrow(
                () => dispatcher.Dispatch(CreateResult(
                    CreateDamagedEvent(EnemyId.FromValue(1UL)))));

            Assert.That(order, Is.EqualTo(new[] { "failing", "after", "hud" }));
            Assert.That(diagnostics.Failures, Has.Count.EqualTo(1));

            ArenaRunDispatchFailure reported = diagnostics.Failures[0];

            Assert.That(reported.Stage, Is.EqualTo(ArenaRunDispatchStage.GameplayVisual));
            Assert.That(
                reported.HandlerName,
                Is.EqualTo(
                    typeof(RecordingNotificationHandler<ArenaRunEnemyDamagedNotification>)
                        .Name));
            Assert.That(
                reported.PayloadType,
                Is.EqualTo(typeof(ArenaRunEnemyDamagedNotification)));
            Assert.That(reported.Exception, Is.SameAs(failure));
        }

        [Test]
        public void SnapshotHandlerFailureIsReported()
        {
            InvalidOperationException failure = new("Snapshot handler failed.");
            RecordingDiagnostics diagnostics = new();
            ArenaRunResultDispatcher dispatcher = new(diagnostics);

            dispatcher.RegisterSnapshotHandler(
                ArenaRunDispatchStage.SnapshotPublication,
                new RecordingSnapshotHandler(
                    _ => throw failure));

            Assert.DoesNotThrow(() => dispatcher.Dispatch(CreateResult()));

            Assert.That(diagnostics.Failures, Has.Count.EqualTo(1));
            Assert.That(
                diagnostics.Failures[0].PayloadType,
                Is.EqualTo(typeof(ArenaRunSnapshot)));
        }

        [Test]
        public void DiagnosticsFailureDoesNotEscape()
        {
            RecordingDiagnostics diagnostics = new()
            {
                FailWith = new InvalidOperationException("Diagnostics failed."),
            };
            ArenaRunResultDispatcher dispatcher = new(diagnostics);

            dispatcher.RegisterNotificationHandler(
                ArenaRunDispatchStage.GameplayVisual,
                new RecordingNotificationHandler<ArenaRunEnemyDamagedNotification>(
                    _ => throw new InvalidOperationException("Handler failed.")));

            Assert.DoesNotThrow(
                () => dispatcher.Dispatch(CreateResult(
                    CreateDamagedEvent(EnemyId.FromValue(1UL)))));
        }

        [Test]
        public void RegistrationIsRejectedAfterFirstDispatch()
        {
            ArenaRunResultDispatcher dispatcher = new();

            dispatcher.Dispatch(CreateResult());

            Assert.Throws<InvalidOperationException>(
                () => dispatcher.RegisterSnapshotHandler(
                    ArenaRunDispatchStage.SnapshotPublication,
                    new RecordingSnapshotHandler()));
            Assert.Throws<InvalidOperationException>(
                () => dispatcher.RegisterNotificationHandler(
                    ArenaRunDispatchStage.GameplayVisual,
                    new RecordingNotificationHandler<ArenaRunVictoryNotification>()));
        }

        [Test]
        public void DuplicateRegistrationsAreRejected()
        {
            RecordingSnapshotHandler snapshotHandler = new();
            RecordingNotificationHandler<ArenaRunVictoryNotification> notificationHandler = new();
            ArenaRunResultDispatcher dispatcher = new();

            dispatcher.RegisterSnapshotHandler(
                ArenaRunDispatchStage.SnapshotPublication,
                snapshotHandler);
            dispatcher.RegisterNotificationHandler(
                ArenaRunDispatchStage.GameplayVisual,
                notificationHandler);

            Assert.Throws<ArgumentException>(
                () => dispatcher.RegisterSnapshotHandler(
                    ArenaRunDispatchStage.SnapshotPublication,
                    snapshotHandler));
            Assert.Throws<ArgumentException>(
                () => dispatcher.RegisterNotificationHandler(
                    ArenaRunDispatchStage.GameplayVisual,
                    notificationHandler));

            Assert.DoesNotThrow(
                () => dispatcher.RegisterNotificationHandler(
                    ArenaRunDispatchStage.Hud,
                    notificationHandler));
        }

        [Test]
        public void HandlerKindMustMatchTheStage()
        {
            ArenaRunResultDispatcher dispatcher = new();

            Assert.Throws<ArgumentException>(
                () => dispatcher.RegisterSnapshotHandler(
                    ArenaRunDispatchStage.GameplayVisual,
                    new RecordingSnapshotHandler()));
            Assert.Throws<ArgumentException>(
                () => dispatcher.RegisterNotificationHandler(
                    ArenaRunDispatchStage.SnapshotPublication,
                    new RecordingNotificationHandler<ArenaRunVictoryNotification>()));
        }

        [Test]
        public void NullResultIsRejected()
        {
            ArenaRunResultDispatcher dispatcher = new();

            Assert.Throws<ArgumentNullException>(() => dispatcher.Dispatch(null));
        }

        private static string Name(EnemyId enemyId)
        {
            return enemyId == EnemyId.FromValue(1UL) ? "enemy-1" : "enemy-2";
        }

        private static IArenaDomainEvent CreateDamagedEvent(EnemyId enemyId)
        {
            return new EnemyDamaged(
                RunId,
                Revision,
                enemyId,
                DamageAmount.FromPoints(20),
                Health.Full(30));
        }

        private static ArenaRunTickResult CreateResult(
            params IArenaDomainEvent[] domainEvents)
        {
            return new ArenaRunTickResult(
                RunId,
                Revision,
                CreateSnapshot(),
                domainEvents,
                Array.Empty<ArenaRunTickStage>());
        }

        private static ArenaRunSnapshot CreateSnapshot()
        {
            return new ArenaRunSnapshot(
                RunId,
                Revision,
                default,
                default,
                default,
                new PlayerSnapshot(
                    PlayerId.FromValue(1UL),
                    default,
                    Health.Full(100),
                    default,
                    default,
                    default,
                    false,
                    default),
                Array.Empty<EnemySnapshot>(),
                new WaveSnapshot(
                    WaveNumber.First,
                    WavePhase.RegularCombat,
                    1,
                    BossStatus.NotSpawned),
                WaveNumber.First);
        }

        private sealed class RecordingSnapshotHandler : IArenaRunSnapshotHandler
        {
            private readonly Action<ArenaRunSnapshot> _onHandle;

            public RecordingSnapshotHandler(Action<ArenaRunSnapshot> onHandle = null)
            {
                _onHandle = onHandle;
            }

            public List<ArenaRunSnapshot> Snapshots { get; } = new();

            public void Handle(ArenaRunSnapshot snapshot)
            {
                Snapshots.Add(snapshot);
                _onHandle?.Invoke(snapshot);
            }
        }

        private sealed class RecordingNotificationHandler<TNotification>
            : IArenaRunNotificationHandler<TNotification>
            where TNotification : IArenaRunNotification
        {
            private readonly Action<TNotification> _onHandle;

            public RecordingNotificationHandler(Action<TNotification> onHandle = null)
            {
                _onHandle = onHandle;
            }

            public List<TNotification> Notifications { get; } = new();

            public void Handle(TNotification notification)
            {
                Notifications.Add(notification);
                _onHandle?.Invoke(notification);
            }
        }

        private sealed class RecordingDiagnostics : IArenaRunDispatchDiagnostics
        {
            public List<ArenaRunDispatchFailure> Failures { get; } = new();

            public Exception FailWith { get; set; }

            public void OnHandlerFailed(ArenaRunDispatchFailure failure)
            {
                if (FailWith != null)
                {
                    throw FailWith;
                }

                Failures.Add(failure);
            }
        }
    }
}
