using System;
using System.Collections.Generic;
using System.Reflection;
using Game.Arena.Application.Ports;
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
using CameraController = Game.Arena.Presentation.Camera.ThirdPersonCameraController;

namespace Game.CompositionRoot.PlayMode.Tests
{
    /// <summary>
    /// The in-memory Arena scene rig the composition fixtures share: a real
    /// <see cref="BootstrapLifetimeScope"/> root, a real
    /// <see cref="ArenaSceneLifetimeScope"/> child, a real
    /// <see cref="ArenaSceneBinder"/>, and every presenter the binder requires.
    /// The binder object stays inactive until the fixture activates it, which is
    /// the only window in which its serialized fields can be assigned.
    /// </summary>
    internal sealed class ArenaSceneTestRig : IDisposable
    {
        private readonly List<GameObject> _objects = new();

        private IArenaSceneReloader _reloader;

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

        public static ArenaSceneTestRig Create(
            bool withBootstrap = true,
            string omittedField = null,
            ArenaGameConfiguration configuration = null,
            IArenaSceneReloader reloader = null)
        {
            ArenaSceneTestRig rig = new();
            rig._reloader = reloader;

            if (withBootstrap)
            {
                GameObject bootstrapObject = rig.Track(new GameObject("Bootstrap Scope"));

                // VContainer starts entry points on every Build, so an active
                // bootstrap would load the Arena scene mid-test (ADR-0032).
                // Awake does not run on an inactive object, which is the only
                // window in which the flag and the configuration can be assigned.
                bootstrapObject.SetActive(false);
                rig.Bootstrap = bootstrapObject.AddComponent<BootstrapLifetimeScope>();
                rig.Bootstrap.LoadArenaOnStart = false;
                rig.Bootstrap.Configuration = configuration;
                bootstrapObject.SetActive(true);
            }

            GameObject scopeObject = rig.Track(new GameObject("Arena Scene Scope"));
            rig.Scope = scopeObject.AddComponent<ArenaSceneLifetimeScope>();

            GameObject cameraObject = rig.Track(new GameObject("Scene Camera"));
            rig.Camera = cameraObject.AddComponent<Camera>();
            rig.CameraController = cameraObject.AddComponent<CameraController>();

            GameObject playerObject = rig.Track(new GameObject("Player"));
            rig.PlayerTransform = playerObject.transform;
            playerObject.AddComponent<PlayerView>();
            rig.PlayerPresenter = playerObject.AddComponent<PlayerPresenter>();
            rig.CameraController.Target = rig.PlayerTransform;

            GameObject inputObject = rig.Track(new GameObject("Input Source"));
            rig.InputSource = inputObject.AddComponent<UnityPlayerInputSource>();
            rig.InputSource.ViewTransform = cameraObject.transform;

            GameObject enemyViewsObject = rig.Track(new GameObject("Enemy Views"));
            rig.EnemyViews = enemyViewsObject.AddComponent<EnemyViewRegistry>();

            GameObject announcerObject = rig.Track(new GameObject("Announcer"));
            rig.Announcer = announcerObject.AddComponent<ArenaRunAnnouncer>();

            rig.EnemyPresenter = enemyViewsObject.AddComponent<EnemyPresenter>();
            rig.EnemyPresenter.Registry = rig.EnemyViews;
            rig.EnemyPresenter.Announcer = rig.Announcer;

            rig.CreateHud();
            rig.CreateCombatVisuals(cameraObject.transform);
            rig.CreateWindows();

            GameObject binderObject = rig.Track(new GameObject("Scene Binder"));
            binderObject.SetActive(false);
            rig.BinderObject = binderObject;
            rig.Binder = binderObject.AddComponent<ArenaSceneBinder>();

            // The binder builds the scene scope in Start from this seam; the rig
            // hands it the same objects plus its recording reloader.
            rig.Binder.SceneObjectsOverride = rig.CreateSceneObjects();

            rig.Wire(omittedField);

            return rig;
        }

        public ArenaSceneObjects CreateSceneObjects()
        {
            return new ArenaSceneObjects(
                Camera,
                PlayerTransform,
                InputSource,
                new EnemyViewTransformRegistry(EnemyViews),
                _reloader);
        }

        public void InvokeAwake()
        {
            MethodInfo awake = typeof(ArenaSceneBinder).GetMethod(
                "Awake",
                BindingFlags.Instance | BindingFlags.NonPublic);

            Assert.That(awake, Is.Not.Null, "ArenaSceneBinder has no Awake method.");

            awake.Invoke(Binder, null);
        }

        public void InvokeStart()
        {
            MethodInfo start = typeof(ArenaSceneBinder).GetMethod(
                "Start",
                BindingFlags.Instance | BindingFlags.NonPublic);

            Assert.That(start, Is.Not.Null, "ArenaSceneBinder has no Start method.");

            start.Invoke(Binder, null);
        }

        public void Dispose()
        {
            for (int index = 0; index < _objects.Count; index++)
            {
                UnityEngine.Object.DestroyImmediate(_objects[index]);
            }

            _objects.Clear();
        }

        private void CreateHud()
        {
            GameObject hudObject = Track(new GameObject("Hud View"));
            Hud = hudObject.AddComponent<HudView>();

            GameObject presentersObject = Track(new GameObject("Hud Presenters"));

            Health = presentersObject.AddComponent<HealthIndicatorPresenter>();
            Health.HudView = Hud;
            Health.Announcer = Announcer;

            EquippedWeapon = presentersObject.AddComponent<EquippedWeaponPresenter>();
            EquippedWeapon.HudView = Hud;
            EquippedWeapon.Announcer = Announcer;

            WaveIndicator = presentersObject.AddComponent<WaveIndicatorPresenter>();
            WaveIndicator.HudView = Hud;
            WaveIndicator.Announcer = Announcer;

            BossIndicator = presentersObject.AddComponent<BossIndicatorPresenter>();
            BossIndicator.HudView = Hud;
            BossIndicator.Announcer = Announcer;

            GameObject defeatObject = Track(new GameObject("Defeat Window"));
            DefeatView = defeatObject.AddComponent<DefeatView>();

            DefeatWindow = presentersObject.AddComponent<DefeatWindowPresenter>();
            DefeatWindow.DefeatView = DefeatView;
            DefeatWindow.Announcer = Announcer;
        }

        private void CreateCombatVisuals(Transform viewTransform)
        {
            GameObject trackerObject = Track(new GameObject("View Direction"));
            ViewDirection = trackerObject.AddComponent<ViewDirectionTracker>();
            ViewDirection.ViewTransform = viewTransform;

            GameObject aimObject = Track(new GameObject("Firearm Aim"));
            aimObject.AddComponent<LineRenderer>();
            FirearmAimView = aimObject.AddComponent<FirearmAimMarkerView>();
            FirearmAim = aimObject.AddComponent<FirearmAimPresenter>();
            FirearmAim.MarkerView = FirearmAimView;
            FirearmAim.ViewDirection = ViewDirection;
            FirearmAim.Announcer = Announcer;

            GameObject swingObject = Track(new GameObject("Melee Swing"));
            swingObject.AddComponent<LineRenderer>();
            MeleeSwingView = swingObject.AddComponent<MeleeSwingView>();
            MeleeSwing = swingObject.AddComponent<MeleeSwingPresenter>();
            MeleeSwing.SwingView = MeleeSwingView;
            MeleeSwing.ViewDirection = ViewDirection;

            GameObject cueObject = Track(new GameObject("Attack Cue"));
            AttackCue = cueObject.AddComponent<EnemyAttackCuePresenter>();
            AttackCue.Announcer = Announcer;

            GameObject bossObject = Track(new GameObject("Boss Entry"));
            BossEntryMarker = bossObject.AddComponent<BossEntryMarkerPresenter>();
        }

        private void CreateWindows()
        {
            GameObject victoryObject = Track(new GameObject("Victory Window"));
            VictoryView = victoryObject.AddComponent<VictoryView>();

            GameObject notificationObject = Track(new GameObject("Notifications"));
            NotificationPresenter =
                notificationObject.AddComponent<ArenaRunNotificationPresenter>();
            NotificationPresenter.Announcer = Announcer;
            NotificationPresenter.VictoryView = VictoryView;
        }

        private void Wire(string omittedField)
        {
            WireField("_scope", Scope, omittedField);
            WireField("_camera", Camera, omittedField);
            WireField("_player", PlayerTransform, omittedField);
            WireField("_inputSource", InputSource, omittedField);
            WireField("_enemyViews", EnemyViews, omittedField);
            WireField("_cameraController", CameraController, omittedField);
            WireField("_playerPresenter", PlayerPresenter, omittedField);
            WireField("_enemyPresenter", EnemyPresenter, omittedField);
            WireField("_firearmAim", FirearmAim, omittedField);
            WireField("_meleeSwing", MeleeSwing, omittedField);
            WireField("_attackCue", AttackCue, omittedField);
            WireField("_bossEntryMarker", BossEntryMarker, omittedField);
            WireField("_notificationPresenter", NotificationPresenter, omittedField);
            WireField("_healthIndicator", Health, omittedField);
            WireField("_equippedWeapon", EquippedWeapon, omittedField);
            WireField("_waveIndicator", WaveIndicator, omittedField);
            WireField("_bossIndicator", BossIndicator, omittedField);
            WireField("_defeatWindow", DefeatWindow, omittedField);
        }

        private void WireField(
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

            field.SetValue(Binder, value);
        }

        private GameObject Track(GameObject gameObject)
        {
            _objects.Add(gameObject);

            return gameObject;
        }
    }
}
