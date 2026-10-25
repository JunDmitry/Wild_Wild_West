using System;
using Game.Arena.Application.Dispatch;
using Game.Arena.Application.Notifications;
using Game.Arena.Application.Ticks;
using Game.Arena.Infrastructure.Input;
using Game.Arena.Infrastructure.Targeting;
using Game.Arena.Presentation.Combat;
using Game.Arena.Presentation.Enemies;
using Game.Arena.Presentation.Hud;
using Game.Arena.Presentation.Notifications;
using Game.Arena.Presentation.Player;
using Game.Arena.Presentation.Windows;
using UnityEngine;
using VContainer;
using VContainer.Unity;
using CameraController = Game.Arena.Presentation.Camera.ThirdPersonCameraController;

namespace Game.CompositionRoot
{
    /// <summary>
    /// The Arena scene's explicit composition seam (ADR-0031). It supplies the
    /// scene objects, builds the child scope against the persistent bootstrap
    /// scope, registers every presenter with the dispatcher, wires the collider
    /// registry, and only then releases the frame driver. Every reference is a
    /// serialized scene reference: nothing is found through a global search, and
    /// a missing one throws before the first tick.
    /// </summary>
    public sealed class ArenaSceneBinder : MonoBehaviour
    {
        [SerializeField] private ArenaSceneLifetimeScope _scope;
        [SerializeField] private Camera _camera;
        [SerializeField] private Transform _player;
        [SerializeField] private UnityPlayerInputSource _inputSource;
        [SerializeField] private EnemyViewRegistry _enemyViews;

        [SerializeField] private CameraController _cameraController;
        [SerializeField] private PlayerPresenter _playerPresenter;
        [SerializeField] private EnemyPresenter _enemyPresenter;

        [SerializeField] private FirearmAimPresenter _firearmAim;
        [SerializeField] private MeleeSwingPresenter _meleeSwing;
        [SerializeField] private EnemyAttackCuePresenter _attackCue;
        [SerializeField] private BossEntryMarkerPresenter _bossEntryMarker;
        [SerializeField] private ArenaRunNotificationPresenter _notificationPresenter;

        [SerializeField] private HealthIndicatorPresenter _healthIndicator;
        [SerializeField] private EquippedWeaponPresenter _equippedWeapon;
        [SerializeField] private WaveIndicatorPresenter _waveIndicator;
        [SerializeField] private BossIndicatorPresenter _bossIndicator;
        [SerializeField] private DefeatWindowPresenter _defeatWindow;

        private void Awake()
        {
            RequireReferences();

            // The typed parent reference is VContainer's own cross-scene link
            // (ADR-0014): the persistent bootstrap scope is resolved by type,
            // never a service by search.
            _scope.parentReference = ParentReference.Create<BootstrapLifetimeScope>();
        }

        private void Start()
        {
            BuildScope();
        }

        private void BuildScope()
        {
            ArenaSceneObjects sceneObjects = new(
                _camera,
                _player,
                _inputSource,
                new EnemyViewTransformRegistry(_enemyViews));

            _scope.Bind(sceneObjects);
            _scope.Build();

            IObjectResolver container = _scope.Container;
            ArenaRunResultDispatcher dispatcher = container.Resolve<ArenaRunResultDispatcher>();

            RegisterHandlers(dispatcher);

            IEnemyColliderRegistration colliders = container.Resolve<IEnemyColliderRegistration>();
            _enemyViews.ViewCreated += colliders.Register;
            _enemyViews.ViewDestroyed += colliders.Unregister;

            // Resolve the loop's service eagerly: a missing dependency fails here,
            // before the first frame drives it.
            container.Resolve<ArenaRunTickService>();
        }

        private void RegisterHandlers(ArenaRunResultDispatcher dispatcher)
        {
            RegisterSnapshotHandlers(dispatcher);
            RegisterHudHandlers(dispatcher);
            RegisterNotificationHandlers(dispatcher);
        }

        private void RegisterSnapshotHandlers(ArenaRunResultDispatcher dispatcher)
        {
            dispatcher.RegisterSnapshotHandler(
                ArenaRunDispatchStage.EntityViewSynchronization,
                new ArenaSnapshotHandler(_cameraController.Present));
            dispatcher.RegisterSnapshotHandler(
                ArenaRunDispatchStage.EntityViewSynchronization,
                new ArenaSnapshotHandler(_playerPresenter.Present));
            dispatcher.RegisterSnapshotHandler(
                ArenaRunDispatchStage.EntityViewSynchronization,
                new ArenaSnapshotHandler(_enemyPresenter.Present));

            dispatcher.RegisterSnapshotHandler(
                ArenaRunDispatchStage.GameplayVisual,
                new ArenaSnapshotHandler(_firearmAim.Present));
            dispatcher.RegisterSnapshotHandler(
                ArenaRunDispatchStage.GameplayVisual,
                new ArenaSnapshotHandler(_meleeSwing.Present));
            dispatcher.RegisterSnapshotHandler(
                ArenaRunDispatchStage.GameplayVisual,
                new ArenaSnapshotHandler(_attackCue.Present));
            dispatcher.RegisterSnapshotHandler(
                ArenaRunDispatchStage.GameplayVisual,
                _notificationPresenter);
        }

        private void RegisterHudHandlers(ArenaRunResultDispatcher dispatcher)
        {
            dispatcher.RegisterHudHandler(
                ArenaRunDispatchStage.Hud,
                new ArenaHudHandler((_, hud) => _healthIndicator.Present(hud)));
            dispatcher.RegisterHudHandler(
                ArenaRunDispatchStage.Hud,
                new ArenaHudHandler((_, hud) => _equippedWeapon.Present(hud)));
            dispatcher.RegisterHudHandler(
                ArenaRunDispatchStage.Hud,
                new ArenaHudHandler((snapshot, hud) => _waveIndicator.Present(snapshot, hud)));
            dispatcher.RegisterHudHandler(
                ArenaRunDispatchStage.Hud,
                new ArenaHudHandler((snapshot, hud) => _bossIndicator.Present(snapshot, hud)));
            dispatcher.RegisterHudHandler(
                ArenaRunDispatchStage.Hud,
                new ArenaHudHandler((snapshot, hud) => _defeatWindow.Present(snapshot, hud)));

            dispatcher.RegisterHudHandler(
                ArenaRunDispatchStage.GameplayVisual,
                new ArenaHudHandler((snapshot, hud) => _bossEntryMarker.Present(snapshot, hud)));
        }

        private void RegisterNotificationHandlers(ArenaRunResultDispatcher dispatcher)
        {
            dispatcher.RegisterNotificationHandler<ArenaRunEnemyDamagedNotification>(
                ArenaRunDispatchStage.GameplayVisual,
                _notificationPresenter);
            dispatcher.RegisterNotificationHandler<ArenaRunEnemyAttackResolvedNotification>(
                ArenaRunDispatchStage.GameplayVisual,
                _notificationPresenter);
            dispatcher.RegisterNotificationHandler<ArenaRunVictoryNotification>(
                ArenaRunDispatchStage.GameplayVisual,
                _notificationPresenter);
        }

        private void RequireReferences()
        {
            Require(_scope, nameof(_scope));
            Require(_camera, nameof(_camera));
            Require(_player, nameof(_player));
            Require(_inputSource, nameof(_inputSource));
            Require(_enemyViews, nameof(_enemyViews));
            Require(_cameraController, nameof(_cameraController));
            Require(_playerPresenter, nameof(_playerPresenter));
            Require(_enemyPresenter, nameof(_enemyPresenter));
            Require(_firearmAim, nameof(_firearmAim));
            Require(_meleeSwing, nameof(_meleeSwing));
            Require(_attackCue, nameof(_attackCue));
            Require(_bossEntryMarker, nameof(_bossEntryMarker));
            Require(_notificationPresenter, nameof(_notificationPresenter));
            Require(_healthIndicator, nameof(_healthIndicator));
            Require(_equippedWeapon, nameof(_equippedWeapon));
            Require(_waveIndicator, nameof(_waveIndicator));
            Require(_bossIndicator, nameof(_bossIndicator));
            Require(_defeatWindow, nameof(_defeatWindow));
        }

        private static void Require(UnityEngine.Object reference, string fieldName)
        {
            if (reference == null)
            {
                throw new InvalidOperationException(
                    "ArenaSceneBinder is missing its " + fieldName + " scene reference.");
            }
        }
    }
}
