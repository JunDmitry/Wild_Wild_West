using System;
using System.Collections;
using System.Collections.Generic;
using System.Reflection;
using Game.Arena.Application.Dispatch;
using Game.Arena.Application.Hud;
using Game.Arena.Application.ReadModels;
using Game.Arena.Application.Sessions;
using Game.Arena.Application.Ticks;
using Game.Arena.Domain.Aggregates.ArenaRun;
using Game.Arena.Domain.Combat;
using Game.Arena.Domain.Concurrency;
using Game.Arena.Domain.Events;
using Game.Arena.Domain.Geometry;
using Game.Arena.Domain.Identity;
using Game.Arena.Domain.Repositories;
using Game.Arena.Domain.Time;
using Game.Arena.Domain.Vitality;
using NUnit.Framework;
using UnityEngine.TestTools;
using VContainer;
using VContainer.Unity;

namespace Game.CompositionRoot.PlayMode.Tests
{
    /// <summary>
    /// Proves the Arena scene binder (ADR-0031): it builds the scene scope over
    /// its serialized references, resolves the tick service and the dispatcher,
    /// fails loudly when a reference or the bootstrap scope is missing, and
    /// registers every presenter on its ADR-0027 stage.
    /// </summary>
    [TestFixture]
    public sealed class ArenaSceneBinderTests
    {
        private static readonly ArenaRunId RunId = ArenaRunId.FromValue(7UL);

        private readonly List<ArenaSceneTestRig> _rigs = new();

        [TearDown]
        public void TearDown()
        {
            for (int index = 0; index < _rigs.Count; index++)
            {
                _rigs[index].Dispose();
            }

            _rigs.Clear();
        }

        [UnityTest]
        public IEnumerator Binder_BuildsTheSceneScope_AndResolvesTheTickService()
        {
            ArenaSceneTestRig rig = CreateRig();

            rig.BinderObject.SetActive(true);
            yield return null;

            Assert.That(rig.Scope.Container, Is.Not.Null);
            Assert.That(
                rig.Scope.Container.Resolve<ArenaRunTickService>(),
                Is.Not.Null);
            Assert.That(
                rig.Scope.Container.Resolve<ArenaRunResultDispatcher>(),
                Is.Not.Null);
        }

        [UnityTest]
        public IEnumerator EnteringTheArena_CreatesTheRunAndTheTickAdvancesIt()
        {
            ArenaSceneTestRig rig = CreateRig();

            rig.BinderObject.SetActive(true);

            // Entry points start at the next Startup and ticks run in Update, so
            // one frame later the scene scope owns a live run and the tick has
            // already operated on it (ADR-0032).
            yield return null;
            yield return null;
            yield return null;

            ArenaRunSession session =
                rig.Scope.Container.Resolve<ArenaRunSession>();

            Assert.That(
                session.HasActiveRun,
                Is.True,
                "The scene-scope startup driver creates the run when the scope builds.");

            IArenaRunRepository repository =
                rig.Scope.Container.Resolve<IArenaRunRepository>();

            Assert.That(
                repository.TryGet(session.ActiveArenaRunId, out ArenaRun run),
                Is.True);

            Assert.That(
                run.Revision,
                Is.GreaterThan(AggregateRevision.Initial),
                "The tick advanced the run the startup driver created.");
        }

        [Test]
        public void Binder_MissingReference_ThrowsNamingTheField()
        {
            ArenaSceneTestRig rig = CreateRig(omittedField: "_defeatWindow");

            TargetInvocationException thrown = Assert.Throws<TargetInvocationException>(
                () => rig.InvokeAwake());

            Assert.That(thrown.InnerException, Is.TypeOf<InvalidOperationException>());
            Assert.That(thrown.InnerException.Message, Does.Contain("_defeatWindow"));
        }

        [Test]
        public void SceneScope_WithoutLiveBootstrap_FailsLoudly()
        {
            ArenaSceneTestRig rig = CreateRig(withBootstrap: false);

            rig.InvokeAwake();
            rig.Scope.Bind(rig.CreateSceneObjects());

            Assert.Throws<VContainerParentTypeReferenceNotFound>(() => rig.Scope.Build());
        }

        [UnityTest]
        public IEnumerator Binder_RegistersHandlersOnTheAdrStages()
        {
            ArenaSceneTestRig rig = CreateRig();

            rig.BinderObject.SetActive(true);
            yield return null;

            ArenaRunResultDispatcher dispatcher =
                rig.Scope.Container.Resolve<ArenaRunResultDispatcher>();

            IList<object> entityHandlers = SnapshotHandlerTargets(
                dispatcher,
                ArenaRunDispatchStage.EntityViewSynchronization);

            Assert.That(entityHandlers.Count, Is.EqualTo(3));
            Assert.AreSame(rig.CameraController, entityHandlers[0]);
            Assert.AreSame(rig.PlayerPresenter, entityHandlers[1]);
            Assert.AreSame(rig.EnemyPresenter, entityHandlers[2]);

            IList<object> gameplayHandlers = SnapshotHandlerTargets(
                dispatcher,
                ArenaRunDispatchStage.GameplayVisual);

            Assert.That(gameplayHandlers.Count, Is.EqualTo(4));
            Assert.AreSame(rig.FirearmAim, gameplayHandlers[0]);
            Assert.AreSame(rig.MeleeSwing, gameplayHandlers[1]);
            Assert.AreSame(rig.AttackCue, gameplayHandlers[2]);
            Assert.AreSame(rig.NotificationPresenter, gameplayHandlers[3]);

            Assert.That(
                HandlerCount(dispatcher, "_snapshotHandlers", ArenaRunDispatchStage.Hud),
                Is.EqualTo(0));
            Assert.That(
                HandlerCount(dispatcher, "_hudHandlers", ArenaRunDispatchStage.Hud),
                Is.EqualTo(5));
            Assert.That(
                HandlerCount(dispatcher, "_hudHandlers", ArenaRunDispatchStage.GameplayVisual),
                Is.EqualTo(1));
            Assert.That(
                HandlerCount(
                    dispatcher,
                    "_notificationHandlers",
                    ArenaRunDispatchStage.GameplayVisual),
                Is.EqualTo(3));
        }

        [Test]
        public void Dispatch_DrivesEntityHudAndNotificationHandlers()
        {
            ArenaSceneTestRig rig = CreateRig();

            // Start (and with it BuildScope) only runs on the first frame of an
            // active object. Invoking it directly keeps this fixture off the
            // frame loop, so the recorder is registered before the startup
            // driver's first dispatch closes registration.
            rig.InvokeAwake();
            rig.InvokeStart();

            ArenaRunResultDispatcher dispatcher =
                rig.Scope.Container.Resolve<ArenaRunResultDispatcher>();

            RecordingHudHandler recorder = new();
            dispatcher.RegisterHudHandler(ArenaRunDispatchStage.Hud, recorder);

            ArenaRunSnapshot snapshot = CreateSnapshot(
                new Position3D(3f, 0f, 4f),
                ArenaRunStatus.Victory);

            ArenaRunTickResult result = new(
                RunId,
                AggregateRevision.Initial,
                snapshot,
                new IArenaDomainEvent[] { new ArenaRunVictorious(RunId, AggregateRevision.Initial) },
                Array.Empty<ArenaRunTickStage>());

            dispatcher.Dispatch(result);

            Assert.That(rig.PlayerTransform.position.x, Is.EqualTo(3f).Within(1e-4f));
            Assert.That(rig.PlayerTransform.position.z, Is.EqualTo(4f).Within(1e-4f));
            Assert.That(rig.EnemyViews.Count, Is.EqualTo(1));

            Assert.That(rig.Hud.HealthText, Is.EqualTo(result.Hud.HealthText));
            Assert.That(rig.Hud.EquippedWeaponText, Is.EqualTo(result.Hud.EquippedWeaponText));
            Assert.That(rig.Hud.WaveText, Is.EqualTo(result.Hud.WaveText));
            Assert.AreSame(result.Snapshot, recorder.Snapshot);
            Assert.AreSame(result.Hud, recorder.Hud);

            Assert.That(rig.VictoryView.IsShown, Is.True);

            float yawBeforeLook = rig.CameraController.Yaw;
            rig.CameraController.ApplyLook(5f, 0f);

            Assert.That(
                rig.CameraController.Yaw,
                Is.EqualTo(yawBeforeLook),
                "A terminal snapshot stops the camera from following look input.");
        }

        private ArenaSceneTestRig CreateRig(
            bool withBootstrap = true,
            string omittedField = null)
        {
            ArenaSceneTestRig rig = ArenaSceneTestRig.Create(withBootstrap, omittedField);

            _rigs.Add(rig);

            return rig;
        }

        private static IList<object> SnapshotHandlerTargets(
            ArenaRunResultDispatcher dispatcher,
            ArenaRunDispatchStage stage)
        {
            List<object> targets = new();

            foreach (object registration in Registrations(dispatcher, "_snapshotHandlers", stage))
            {
                object handler = Handler(registration);

                targets.Add(handler is ArenaSnapshotHandler
                    ? DelegateTarget(handler)
                    : handler);
            }

            return targets;
        }

        private static object DelegateTarget(object handler)
        {
            FieldInfo present = handler.GetType().GetField(
                "_present",
                BindingFlags.Instance | BindingFlags.NonPublic);

            Assert.That(present, Is.Not.Null);

            Delegate presentDelegate = (Delegate)present.GetValue(handler);

            return presentDelegate.Target;
        }

        private static object Handler(object registration)
        {
            PropertyInfo property = registration.GetType().GetProperty(
                "Handler",
                BindingFlags.Instance | BindingFlags.Public);

            Assert.That(property, Is.Not.Null);

            return property.GetValue(registration);
        }

        private static int HandlerCount(
            ArenaRunResultDispatcher dispatcher,
            string fieldName,
            ArenaRunDispatchStage stage)
        {
            object registrations = RegistrationsMap(dispatcher, fieldName)[stage];

            return registrations == null ? 0 : ((ICollection)registrations).Count;
        }

        private static IEnumerable Registrations(
            ArenaRunResultDispatcher dispatcher,
            string fieldName,
            ArenaRunDispatchStage stage)
        {
            object registrations = RegistrationsMap(dispatcher, fieldName)[stage];

            return registrations as IEnumerable ?? Array.Empty<object>();
        }

        private static IDictionary RegistrationsMap(
            ArenaRunResultDispatcher dispatcher,
            string fieldName)
        {
            FieldInfo field = typeof(ArenaRunResultDispatcher).GetField(
                fieldName,
                BindingFlags.Instance | BindingFlags.NonPublic);

            Assert.That(field, Is.Not.Null, "No dispatcher field named " + fieldName + ".");

            return (IDictionary)field.GetValue(dispatcher);
        }

        private static ArenaRunSnapshot CreateSnapshot(
            Position3D playerPosition,
            ArenaRunStatus status)
        {
            PlayerSnapshot player = new(
                PlayerId.FromValue(1UL),
                playerPosition,
                Health.Full(100),
                WeaponKind.Melee,
                new GameTimePoint(0d),
                new GameTimePoint(0d),
                false,
                default);

            EnemySnapshot enemy = new(
                EnemyId.FromValue(1UL),
                EnemyKind.Regular,
                new Position3D(1f, 0f, 1f),
                Health.Full(50),
                CollisionRadius.FromValue(0.5f),
                new GameTimePoint(0d),
                false,
                default);

            return new ArenaRunSnapshot(
                RunId,
                AggregateRevision.Initial,
                status,
                new GameTimePoint(0d),
                new ArenaBounds(-20f, 20f, -20f, 20f, 0f),
                player,
                new[] { enemy },
                new WaveSnapshot(
                    WaveNumber.First,
                    WavePhase.RegularCombat,
                    0,
                    BossStatus.NotSpawned),
                WaveNumber.FromValue(3));
        }

        private sealed class RecordingHudHandler : IArenaRunHudHandler
        {
            public ArenaRunSnapshot Snapshot { get; private set; }

            public ArenaRunHudSnapshot Hud { get; private set; }

            public void Handle(ArenaRunSnapshot snapshot, ArenaRunHudSnapshot hud)
            {
                Snapshot = snapshot;
                Hud = hud;
            }
        }
    }
}
