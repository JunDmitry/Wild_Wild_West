using System;
using System.Collections.Generic;
using Game.Arena.Application.Dispatch;
using Game.Arena.Application.Input;
using Game.Arena.Application.Notifications;
using Game.Arena.Application.Ports;
using Game.Arena.Application.ReadModels;
using Game.Arena.Application.Ticks;
using Game.Arena.Domain.Aggregates.ArenaRun;
using Game.Arena.Domain.Combat;
using Game.Arena.Domain.Concurrency;
using Game.Arena.Domain.Configuration;
using Game.Arena.Domain.Geometry;
using Game.Arena.Domain.Identity;
using Game.Arena.Domain.Time;
using Game.Arena.Domain.Vitality;
using Game.Arena.Infrastructure.Targeting;
using Game.Arena.Presentation.Accessibility;
using Game.Arena.Presentation.Combat;
using Game.Arena.Presentation.Notifications;
using Game.Arena.Presentation.Windows;
using NUnit.Framework;
using TMPro;
using UnityEngine;

namespace Game.Arena.PlayMode.Tests
{
    [TestFixture]
    public sealed class VictoryWindowTests
    {
        private const int MaxTicks = 2000;

        private readonly List<GameObject> _objects = new();
        private TestEnemyColliderRegistry _registry;
        private HitConfirmationPresenter _hitConfirmation;
        private ArenaRunAnnouncer _announcer;
        private VictoryView _view;
        private ArenaRunNotificationPresenter _presenter;
        private TextMeshPro _titleText;
        private TextMeshPro _messageText;
        private FixedInputSource _input;
        private PlayModeTickHarness _harness;
        private ArenaRunResultDispatcher _dispatcher;

        [SetUp]
        public void SetUp()
        {
            _objects.Clear();
            _registry = new TestEnemyColliderRegistry();

            GameObject root = Track(new GameObject("Victory"));

            _view = root.AddComponent<VictoryView>();
            _hitConfirmation = root.AddComponent<HitConfirmationPresenter>();
            _hitConfirmation.LifetimeSeconds = 10f;
            _announcer = root.AddComponent<ArenaRunAnnouncer>();
            _presenter = root.AddComponent<ArenaRunNotificationPresenter>();

            GameObject title = Track(new GameObject("Title"));
            title.transform.SetParent(root.transform, false);
            _titleText = title.AddComponent<TextMeshPro>();

            GameObject message = Track(new GameObject("Message"));
            message.transform.SetParent(root.transform, false);
            _messageText = message.AddComponent<TextMeshPro>();

            _view.TitleTextMesh = _titleText;
            _view.MessageTextMesh = _messageText;
            _presenter.VictoryView = _view;
            _presenter.HitConfirmation = _hitConfirmation;
            _presenter.Announcer = _announcer;

            _dispatcher = new ArenaRunResultDispatcher();
            _dispatcher.RegisterSnapshotHandler(
                ArenaRunDispatchStage.EntityViewSynchronization,
                _presenter);
            _dispatcher.RegisterNotificationHandler<ArenaRunEnemyDamagedNotification>(
                ArenaRunDispatchStage.GameplayVisual,
                _presenter);
            _dispatcher.RegisterNotificationHandler<ArenaRunEnemyAttackResolvedNotification>(
                ArenaRunDispatchStage.GameplayVisual,
                _presenter);
            _dispatcher.RegisterNotificationHandler<ArenaRunVictoryNotification>(
                ArenaRunDispatchStage.GameplayVisual,
                _presenter);
        }

        [TearDown]
        public void TearDown()
        {
            for (int index = 0; index < _objects.Count; index++)
            {
                UnityEngine.Object.DestroyImmediate(_objects[index]);
            }

            _objects.Clear();
        }

        [Test]
        public void WinningTheRun_ShowsTheVictoryWindowWithNoControl()
        {
            CreateHarness();

            Assert.That(_view.IsShown, Is.False);

            ArenaRunTickResult result = WinTheRun();

            Assert.That(result.Snapshot.Status, Is.EqualTo(ArenaRunStatus.Victory));
            Assert.That(_view.IsShown, Is.True);
            Assert.That(_view.ShownRunId, Is.EqualTo(result.Snapshot.ArenaRunId));
            Assert.That(_titleText.text, Is.EqualTo(VictoryView.Title));
            Assert.That(_titleText.text, Is.EqualTo("Victory"));
            Assert.That(_messageText.text, Is.EqualTo(VictoryView.Message));
            Assert.That(_messageText.text, Is.EqualTo("the run was won"));
            Assert.That(
                _titleText.transform.GetSiblingIndex(),
                Is.LessThan(_messageText.transform.GetSiblingIndex()));

            Assert.That(_view.HoldsFocus, Is.True);
            Assert.That(_view.GetComponent<Collider>(), Is.Null);
            Assert.That(_view.GetComponent<Collider2D>(), Is.Null);

            Assert.That(
                _announcer.LastAnnouncement,
                Is.EqualTo(ArenaRunAnnouncer.VictoryAnnouncement));
            Assert.That(
                _announcer.LastAnnouncement,
                Is.EqualTo("Victory; the run has been won"));
        }

        [Test]
        public void LaterTicksOnTheVictoriousRun_DoNotReShowOrReAnnounce()
        {
            CreateHarness();
            WinTheRun();

            int announcementsAtVictory = _announcer.AnnouncementCount;

            for (int index = 0; index < 5; index++)
            {
                _input.Input = CreateInput(false);

                ArenaRunTickResult terminal = _harness.TickFrame();

                Assert.That(terminal.Snapshot.Status, Is.EqualTo(ArenaRunStatus.Victory));
                Assert.That(_view.IsShown, Is.True);
                Assert.That(_view.HoldsFocus, Is.True);
                Assert.That(_titleText.text, Is.EqualTo(VictoryView.Title));
                Assert.That(_messageText.text, Is.EqualTo(VictoryView.Message));
                Assert.That(_announcer.AnnouncementCount, Is.EqualTo(announcementsAtVictory));
            }
        }

        [Test]
        public void InputsAfterVictory_ChangeNothingAndAreNotAnnounced()
        {
            CreateHarness();
            ArenaRunTickResult victory = WinTheRun();

            int announcementsAtVictory = _announcer.AnnouncementCount;
            Position3D playerPosition = victory.Snapshot.Player.Position;

            AssertGameplayTickChangesNothing(
                victory,
                new PlayerFrameInput(
                    new MovementAxes(1f, 0f),
                    Direction3D.Forward,
                    false,
                    false));
            AssertGameplayTickChangesNothing(
                victory,
                new PlayerFrameInput(
                    MovementAxes.Zero,
                    Direction3D.Left,
                    false,
                    false));
            AssertGameplayTickChangesNothing(
                victory,
                new PlayerFrameInput(
                    MovementAxes.Zero,
                    Direction3D.Forward,
                    false,
                    true));
            AssertGameplayTickChangesNothing(
                victory,
                new PlayerFrameInput(
                    MovementAxes.Zero,
                    Direction3D.Forward,
                    true,
                    false));

            Assert.That(_announcer.AnnouncementCount, Is.EqualTo(announcementsAtVictory));
            Assert.That(_view.IsShown, Is.True);
            Assert.That(victory.Snapshot.Player.Position.X, Is.EqualTo(playerPosition.X).Within(1e-4f));
        }

        [Test]
        public void VictoryNotificationForAnotherRun_DoesNotShowTheWindow()
        {
            ArenaRunSnapshot snapshot = CreateSnapshot(
                ArenaRunId.FromValue(1UL),
                ArenaRunStatus.Playing);

            _presenter.Handle(snapshot);
            _presenter.Handle(new ArenaRunVictoryNotification(
                ArenaRunId.FromValue(2UL),
                AggregateRevision.Initial));

            Assert.That(_view.IsShown, Is.False);
            Assert.That(_view.HoldsFocus, Is.False);
            Assert.That(_announcer.AnnouncementCount, Is.Zero);
        }

        private void AssertGameplayTickChangesNothing(
            ArenaRunTickResult victory,
            PlayerFrameInput input)
        {
            _input.Input = input;

            ArenaRunTickResult after = _harness.TickFrame();

            Assert.That(after.Snapshot.Status, Is.EqualTo(ArenaRunStatus.Victory));
            Assert.That(after.Snapshot.Revision, Is.EqualTo(victory.Snapshot.Revision));
            Assert.That(after.Snapshot.Player.Position.X, Is.EqualTo(victory.Snapshot.Player.Position.X).Within(1e-4f));
            Assert.That(after.Snapshot.Player.Position.Z, Is.EqualTo(victory.Snapshot.Player.Position.Z).Within(1e-4f));
        }

        private void CreateHarness()
        {
            _input = new FixedInputSource(CreateInput(false));
            _harness = new PlayModeTickHarness(
                _registry,
                waveCatalog: CreateBossOnlyCatalog(),
                playerInputSource: _input,
                dispatcher: _dispatcher);
        }

        private ArenaRunTickResult WinTheRun()
        {
            SeedBoss(1UL, new Vector3(0f, 0f, 21f));

            _input.Input = CreateInput(true);

            for (int index = 0; index < MaxTicks; index++)
            {
                ArenaRunTickResult result = _harness.TickFrame();

                if (result.Snapshot.Status == ArenaRunStatus.Victory)
                {
                    return result;
                }
            }

            Assert.Fail("Run did not reach Victory.");
            return null;
        }

        private void SeedBoss(ulong value, Vector3 position)
        {
            EnemyId enemyId = EnemyId.FromValue(value);

            GameObject enemy = Track(new GameObject("Boss" + value));
            enemy.transform.position = position;

            BoxCollider collider = enemy.AddComponent<BoxCollider>();
            collider.size = new Vector3(1f, 2f, 1f);

            Physics.SyncTransforms();

            _registry.Register(collider, enemyId);
            _harness.SeedEnemy(value, new Position3D(position.x, position.y, position.z));
        }

        private static PlayerFrameInput CreateInput(bool attackPressed)
        {
            return new PlayerFrameInput(
                MovementAxes.Zero,
                Direction3D.Forward,
                attackPressed,
                false);
        }

        private static WaveCatalog CreateBossOnlyCatalog()
        {
            return new WaveCatalog(
                new[]
                {
                    new WaveDefinition(WaveNumber.First, 0),
                });
        }

        private static ArenaRunSnapshot CreateSnapshot(
            ArenaRunId arenaRunId,
            ArenaRunStatus status)
        {
            PlayerSnapshot player = new(
                PlayerId.FromValue(1UL),
                Position3D.Zero,
                Health.Full(100),
                WeaponKind.Ranged,
                new GameTimePoint(0d),
                new GameTimePoint(0d),
                false,
                default);

            return new ArenaRunSnapshot(
                arenaRunId,
                AggregateRevision.Initial,
                status,
                new GameTimePoint(0d),
                new ArenaBounds(-20f, 20f, -20f, 20f, 0f),
                player,
                Array.Empty<EnemySnapshot>(),
                new WaveSnapshot(WaveNumber.First, WavePhase.RegularCombat, 0, BossStatus.NotSpawned),
                WaveNumber.First);
        }

        private GameObject Track(GameObject gameObject)
        {
            _objects.Add(gameObject);
            return gameObject;
        }

        private sealed class FixedInputSource : IPlayerInputSource
        {
            public FixedInputSource(PlayerFrameInput input)
            {
                Input = input;
            }

            public PlayerFrameInput Input { get; set; }

            public PlayerFrameInput Read()
            {
                return Input;
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
