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
using Game.Arena.Infrastructure.Input;
using Game.Arena.Presentation.Accessibility;
using Game.Arena.Presentation.Combat;
using Game.Arena.Presentation.Enemies;
using Game.Arena.Presentation.Hud;
using Game.Arena.Presentation.Notifications;
using Game.Arena.Presentation.Player;
using Game.Arena.Presentation.Windows;
using NUnit.Framework;
using UnityEngine;
using UnityEngine.TestTools;
using VContainer;
using VContainer.Unity;
using CameraController = Game.Arena.Presentation.Camera.ThirdPersonCameraController;

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

        private readonly List<GameObject> _objects = new();

        [TearDown]
        public void TearDown()
        {
            for (int index = 0; index < _objects.Count; index++)
            {
                UnityEngine.Object.DestroyImmediate(_objects[index]);
            }

            _objects.Clear();
        }

        [UnityTest]
        public IEnumerator Binder_BuildsTheSceneScope_AndResolvesTheTickService()
        {
            ArenaSceneRig rig = CreateRig();

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
            ArenaSceneRig rig = CreateRig();

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
            ArenaSceneRig rig = CreateRig(omittedField: "_defeatWindow");

            TargetInvocationException thrown = Assert.Throws<TargetInvocationException>(
                () => InvokeAwake(rig.Binder));

            Assert.That(thrown.InnerException, Is.TypeOf<InvalidOperationException>());
            Assert.That(thrown.InnerException.Message, Does.Contain("_defeatWindow"));
        }

        [Test]
        public void SceneScope_WithoutLiveBootstrap_FailsLoudly()
        {
            ArenaSceneRig rig = CreateRig(withBootstrap: false);

            InvokeAwake(rig.Binder);
            rig.Scope.Bind(rig.CreateSceneObjects());

            Assert.Throws<VContainerParentTypeReferenceNotFound>(() => rig.Scope.Build());
        }

        [UnityTest]
        public IEnumerator Binder_RegistersHandlersOnTheAdrStages()
        {
            ArenaSceneRig rig = CreateRig();

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
            ArenaSceneRig rig = CreateRig();

            // Start (and with it BuildScope) only runs on the first frame of an
            // active object. Invoking it directly keeps this fixture off the
            // frame loop, so the recorder is registered before the startup
            // driver's first dispatch closes registration.
            InvokeAwake(rig.Binder);
            InvokeStart(rig.Binder);

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

        private ArenaSceneRig CreateRig(
            bool withBootstrap = true,
            string omittedField = null)
        {
            ArenaSceneRig rig = new();

            if (withBootstrap)
            {
                GameObject bootstrapObject = Track(new GameObject("Bootstrap Scope"));

                // VContainer starts entry points on every Build, so an active
                // bootstrap would load the Arena scene mid-test (ADR-0032).
                // Awake does not run on an inactive object, which is the only
                // window in which the flag can be turned off.
                bootstrapObject.SetActive(false);
                rig.Bootstrap = bootstrapObject.AddComponent<BootstrapLifetimeScope>();
                rig.Bootstrap.LoadArenaOnStart = false;
                bootstrapObject.SetActive(true);
            }

            GameObject scopeObject = Track(new GameObject("Arena Scene Scope"));
            rig.Scope = scopeObject.AddComponent<ArenaSceneLifetimeScope>();

            GameObject cameraObject = Track(new GameObject("Scene Camera"));
            rig.Camera = cameraObject.AddComponent<Camera>();
            rig.CameraController = cameraObject.AddComponent<CameraController>();

            GameObject playerObject = Track(new GameObject("Player"));
            rig.PlayerTransform = playerObject.transform;
            playerObject.AddComponent<PlayerView>();
            rig.PlayerPresenter = playerObject.AddComponent<PlayerPresenter>();
            rig.CameraController.Target = rig.PlayerTransform;

            GameObject inputObject = Track(new GameObject("Input Source"));
            rig.InputSource = inputObject.AddComponent<UnityPlayerInputSource>();
            rig.InputSource.ViewTransform = cameraObject.transform;

            GameObject enemyViewsObject = Track(new GameObject("Enemy Views"));
            rig.EnemyViews = enemyViewsObject.AddComponent<EnemyViewRegistry>();

            GameObject announcerObject = Track(new GameObject("Announcer"));
            rig.Announcer = announcerObject.AddComponent<ArenaRunAnnouncer>();

            rig.EnemyPresenter = enemyViewsObject.AddComponent<EnemyPresenter>();
            rig.EnemyPresenter.Registry = rig.EnemyViews;
            rig.EnemyPresenter.Announcer = rig.Announcer;

            CreateHud(rig);
            CreateCombatVisuals(rig, cameraObject.transform);
            CreateWindows(rig);

            GameObject binderObject = Track(new GameObject("Scene Binder"));
            binderObject.SetActive(false);
            rig.BinderObject = binderObject;
            rig.Binder = binderObject.AddComponent<ArenaSceneBinder>();

            Wire(rig, omittedField);

            return rig;
        }

        private void CreateHud(ArenaSceneRig rig)
        {
            GameObject hudObject = Track(new GameObject("Hud View"));
            rig.Hud = hudObject.AddComponent<HudView>();

            GameObject presentersObject = Track(new GameObject("Hud Presenters"));

            rig.Health = presentersObject.AddComponent<HealthIndicatorPresenter>();
            rig.Health.HudView = rig.Hud;
            rig.Health.Announcer = rig.Announcer;

            rig.EquippedWeapon = presentersObject.AddComponent<EquippedWeaponPresenter>();
            rig.EquippedWeapon.HudView = rig.Hud;
            rig.EquippedWeapon.Announcer = rig.Announcer;

            rig.WaveIndicator = presentersObject.AddComponent<WaveIndicatorPresenter>();
            rig.WaveIndicator.HudView = rig.Hud;
            rig.WaveIndicator.Announcer = rig.Announcer;

            rig.BossIndicator = presentersObject.AddComponent<BossIndicatorPresenter>();
            rig.BossIndicator.HudView = rig.Hud;
            rig.BossIndicator.Announcer = rig.Announcer;

            GameObject defeatObject = Track(new GameObject("Defeat Window"));
            rig.DefeatView = defeatObject.AddComponent<DefeatView>();

            rig.DefeatWindow = presentersObject.AddComponent<DefeatWindowPresenter>();
            rig.DefeatWindow.DefeatView = rig.DefeatView;
            rig.DefeatWindow.Announcer = rig.Announcer;
        }

        private void CreateCombatVisuals(ArenaSceneRig rig, Transform viewTransform)
        {
            GameObject trackerObject = Track(new GameObject("View Direction"));
            rig.ViewDirection = trackerObject.AddComponent<ViewDirectionTracker>();
            rig.ViewDirection.ViewTransform = viewTransform;

            GameObject aimObject = Track(new GameObject("Firearm Aim"));
            aimObject.AddComponent<LineRenderer>();
            rig.FirearmAimView = aimObject.AddComponent<FirearmAimMarkerView>();
            rig.FirearmAim = aimObject.AddComponent<FirearmAimPresenter>();
            rig.FirearmAim.MarkerView = rig.FirearmAimView;
            rig.FirearmAim.ViewDirection = rig.ViewDirection;
            rig.FirearmAim.Announcer = rig.Announcer;

            GameObject swingObject = Track(new GameObject("Melee Swing"));
            swingObject.AddComponent<LineRenderer>();
            rig.MeleeSwingView = swingObject.AddComponent<MeleeSwingView>();
            rig.MeleeSwing = swingObject.AddComponent<MeleeSwingPresenter>();
            rig.MeleeSwing.SwingView = rig.MeleeSwingView;
            rig.MeleeSwing.ViewDirection = rig.ViewDirection;

            GameObject cueObject = Track(new GameObject("Attack Cue"));
            rig.AttackCue = cueObject.AddComponent<EnemyAttackCuePresenter>();
            rig.AttackCue.Announcer = rig.Announcer;

            GameObject bossObject = Track(new GameObject("Boss Entry"));
            rig.BossEntryMarker = bossObject.AddComponent<BossEntryMarkerPresenter>();
        }

        private void CreateWindows(ArenaSceneRig rig)
        {
            GameObject victoryObject = Track(new GameObject("Victory Window"));
            rig.VictoryView = victoryObject.AddComponent<VictoryView>();

            GameObject notificationObject = Track(new GameObject("Notifications"));
            rig.NotificationPresenter =
                notificationObject.AddComponent<ArenaRunNotificationPresenter>();
            rig.NotificationPresenter.Announcer = rig.Announcer;
            rig.NotificationPresenter.VictoryView = rig.VictoryView;
        }

        private void Wire(ArenaSceneRig rig, string omittedField)
        {
            WireField(rig.Binder, "_scope", rig.Scope, omittedField);
            WireField(rig.Binder, "_camera", rig.Camera, omittedField);
            WireField(rig.Binder, "_player", rig.PlayerTransform, omittedField);
            WireField(rig.Binder, "_inputSource", rig.InputSource, omittedField);
            WireField(rig.Binder, "_enemyViews", rig.EnemyViews, omittedField);
            WireField(rig.Binder, "_cameraController", rig.CameraController, omittedField);
            WireField(rig.Binder, "_playerPresenter", rig.PlayerPresenter, omittedField);
            WireField(rig.Binder, "_enemyPresenter", rig.EnemyPresenter, omittedField);
            WireField(rig.Binder, "_firearmAim", rig.FirearmAim, omittedField);
            WireField(rig.Binder, "_meleeSwing", rig.MeleeSwing, omittedField);
            WireField(rig.Binder, "_attackCue", rig.AttackCue, omittedField);
            WireField(rig.Binder, "_bossEntryMarker", rig.BossEntryMarker, omittedField);
            WireField(rig.Binder, "_notificationPresenter", rig.NotificationPresenter, omittedField);
            WireField(rig.Binder, "_healthIndicator", rig.Health, omittedField);
            WireField(rig.Binder, "_equippedWeapon", rig.EquippedWeapon, omittedField);
            WireField(rig.Binder, "_waveIndicator", rig.WaveIndicator, omittedField);
            WireField(rig.Binder, "_bossIndicator", rig.BossIndicator, omittedField);
            WireField(rig.Binder, "_defeatWindow", rig.DefeatWindow, omittedField);
        }

        private static void WireField(
            ArenaSceneBinder binder,
            string fieldName,
            UnityEngine.Object value,
            string omittedField)
        {
            if (string.Equals(fieldName, omittedField, StringComparison.Ordinal))
            {
                return;
            }

            FieldInfo field = typeof(ArenaSceneBinder).GetField(
                fieldName,
                BindingFlags.Instance | BindingFlags.NonPublic);

            Assert.That(
                field,
                Is.Not.Null,
                "ArenaSceneBinder has no serialized field named " + fieldName + ".");

            field.SetValue(binder, value);
        }

        private static void InvokeAwake(ArenaSceneBinder binder)
        {
            MethodInfo awake = typeof(ArenaSceneBinder).GetMethod(
                "Awake",
                BindingFlags.Instance | BindingFlags.NonPublic);

            Assert.That(awake, Is.Not.Null, "ArenaSceneBinder has no Awake method.");

            awake.Invoke(binder, null);
        }

        private static void InvokeStart(ArenaSceneBinder binder)
        {
            MethodInfo start = typeof(ArenaSceneBinder).GetMethod(
                "Start",
                BindingFlags.Instance | BindingFlags.NonPublic);

            Assert.That(start, Is.Not.Null, "ArenaSceneBinder has no Start method.");

            start.Invoke(binder, null);
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

        private GameObject Track(GameObject gameObject)
        {
            _objects.Add(gameObject);

            return gameObject;
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

        private sealed class ArenaSceneRig
        {
            public GameObject BinderObject;
            public BootstrapLifetimeScope Bootstrap;
            public ArenaSceneBinder Binder;
            public ArenaSceneLifetimeScope Scope;
            public Camera Camera;
            public CameraController CameraController;
            public Transform PlayerTransform;
            public PlayerPresenter PlayerPresenter;
            public UnityPlayerInputSource InputSource;
            public EnemyViewRegistry EnemyViews;
            public EnemyPresenter EnemyPresenter;
            public ArenaRunAnnouncer Announcer;
            public HudView Hud;
            public HealthIndicatorPresenter Health;
            public EquippedWeaponPresenter EquippedWeapon;
            public WaveIndicatorPresenter WaveIndicator;
            public BossIndicatorPresenter BossIndicator;
            public DefeatView DefeatView;
            public DefeatWindowPresenter DefeatWindow;
            public ViewDirectionTracker ViewDirection;
            public FirearmAimMarkerView FirearmAimView;
            public FirearmAimPresenter FirearmAim;
            public MeleeSwingView MeleeSwingView;
            public MeleeSwingPresenter MeleeSwing;
            public EnemyAttackCuePresenter AttackCue;
            public BossEntryMarkerPresenter BossEntryMarker;
            public VictoryView VictoryView;
            public ArenaRunNotificationPresenter NotificationPresenter;

            public ArenaSceneObjects CreateSceneObjects()
            {
                return new ArenaSceneObjects(
                    Camera,
                    PlayerTransform,
                    InputSource,
                    new EnemyViewTransformRegistry(EnemyViews));
            }
        }
    }
}
