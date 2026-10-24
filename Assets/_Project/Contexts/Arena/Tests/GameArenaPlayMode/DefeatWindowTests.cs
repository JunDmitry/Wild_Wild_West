using System;
using System.Collections.Generic;
using Game.Arena.Application.Input;
using Game.Arena.Application.Ports;
using Game.Arena.Application.ReadModels;
using Game.Arena.Application.Ticks;
using Game.Arena.Domain.Aggregates.ArenaRun;
using Game.Arena.Domain.Configuration;
using Game.Arena.Domain.Geometry;
using Game.Arena.Domain.Identity;
using Game.Arena.Domain.Time;
using Game.Arena.Infrastructure.Targeting;
using Game.Arena.Presentation.Accessibility;
using Game.Arena.Presentation.Hud;
using Game.Arena.Presentation.Windows;
using NUnit.Framework;
using TMPro;
using UnityEngine;

namespace Game.Arena.PlayMode.Tests
{
    [TestFixture]
    public sealed class DefeatWindowTests
    {
        private const int MaxTicks = 2000;

        private readonly List<GameObject> _objects = new();
        private EmptyEnemyColliderRegistry _registry;
        private ArenaRunAnnouncer _announcer;
        private DefeatView _view;
        private DefeatWindowPresenter _presenter;
        private HudView _hudView;
        private WaveIndicatorPresenter _wavePresenter;
        private TextMeshPro _statementText;
        private TextMeshPro _waveText;
        private FixedInputSource _input;
        private PlayModeTickHarness _harness;

        [SetUp]
        public void SetUp()
        {
            _objects.Clear();
            _registry = new EmptyEnemyColliderRegistry();

            GameObject root = Track(new GameObject("Defeat"));

            _announcer = root.AddComponent<ArenaRunAnnouncer>();
            _presenter = root.AddComponent<DefeatWindowPresenter>();

            GameObject viewObject = Track(new GameObject("DefeatWindow"));
            viewObject.transform.SetParent(root.transform, false);
            _view = viewObject.AddComponent<DefeatView>();

            GameObject statement = Track(new GameObject("Statement"));
            statement.transform.SetParent(viewObject.transform, false);
            _statementText = statement.AddComponent<TextMeshPro>();

            GameObject wave = Track(new GameObject("WaveReached"));
            wave.transform.SetParent(viewObject.transform, false);
            _waveText = wave.AddComponent<TextMeshPro>();

            _view.StatementTextMesh = _statementText;
            _view.WaveTextMesh = _waveText;
            _presenter.DefeatView = _view;
            _presenter.Announcer = _announcer;

            _hudView = root.AddComponent<HudView>();
            _wavePresenter = root.AddComponent<WaveIndicatorPresenter>();

            GameObject hudWave = Track(new GameObject("HudWave"));
            hudWave.transform.SetParent(root.transform, false);
            TextMeshPro hudWaveText = hudWave.AddComponent<TextMeshPro>();

            _hudView.WaveTextMesh = hudWaveText;
            _wavePresenter.HudView = _hudView;
            _wavePresenter.Announcer = _announcer;
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
        public void DefeatingTheRun_ShowsTheDefeatWindowWithStatementAndWave()
        {
            CreateHarness();
            SeedEnemy();

            Assert.That(_view.IsShown, Is.False);

            ArenaRunTickResult defeated = TickUntilStatus(
                ArenaRunStatus.Defeat,
                CreateInput(false));

            Assert.That(defeated.Snapshot.Status, Is.EqualTo(ArenaRunStatus.Defeat));
            Assert.That(_view.IsShown, Is.True);
            Assert.That(_view.ShownRunId, Is.EqualTo(defeated.Snapshot.ArenaRunId));
            Assert.That(_statementText.text, Is.EqualTo(DefeatView.Statement));
            Assert.That(_statementText.text, Is.EqualTo("Defeat"));
            Assert.That(_waveText.text, Is.EqualTo(defeated.Hud.WaveText));
            Assert.That(
                _statementText.transform.GetSiblingIndex(),
                Is.LessThan(_waveText.transform.GetSiblingIndex()));
            Assert.That(_view.GetComponent<Collider>(), Is.Null);
            Assert.That(_view.GetComponent<Collider2D>(), Is.Null);
        }

        [Test]
        public void DefeatWindow_PersistsAcrossDefeatedTicks_AndHidesOnTheRestart()
        {
            CreateHarness();
            SeedEnemy();

            ArenaRunTickResult defeated = TickUntilStatus(
                ArenaRunStatus.Defeat,
                CreateInput(false));

            int announcementsAtDefeat = _announcer.AnnouncementCount;

            for (int index = 0; index < 5; index++)
            {
                ArenaRunTickResult terminal = Tick(CreateInput(false));

                Assert.That(terminal.Snapshot.Status, Is.EqualTo(ArenaRunStatus.Defeat));
                Assert.That(_view.IsShown, Is.True);
                Assert.That(_statementText.text, Is.EqualTo(DefeatView.Statement));
                Assert.That(_waveText.text, Is.EqualTo(defeated.Hud.WaveText));
                Assert.That(_announcer.AnnouncementCount, Is.EqualTo(announcementsAtDefeat));
            }

            ArenaRunTickResult restarted = TickUntilRestart(
                defeated.Snapshot.ArenaRunId,
                CreateInput(false));

            Assert.That(restarted.Snapshot.Status, Is.EqualTo(ArenaRunStatus.Playing));
            Assert.That(_view.IsShown, Is.False);
            Assert.That(_view.ShownRunId, Is.EqualTo(ArenaRunId.None));
        }

        [Test]
        public void WaveLine_MatchesThePublishedHudValues()
        {
            CreateHarness();
            SeedEnemy();

            ArenaRunTickResult defeated = TickUntilStatus(
                ArenaRunStatus.Defeat,
                CreateInput(false));

            Assert.That(defeated.Hud.WaveCurrent, Is.EqualTo(1));
            Assert.That(defeated.Hud.WaveTotal, Is.EqualTo(3));
            Assert.That(_waveText.text, Is.EqualTo(defeated.Hud.WaveText));
            Assert.That(_waveText.text, Is.EqualTo("Wave 1 of 3"));
        }

        [Test]
        public void InputsWhileDefeated_ChangeNothingAndDoNotCarryIntoTheFreshRun()
        {
            CreateHarness();
            SeedEnemy();

            ArenaRunTickResult defeated = TickUntilStatus(
                ArenaRunStatus.Defeat,
                CreateInput(false));

            int announcementsAtDefeat = _announcer.AnnouncementCount;

            AssertGameplayTickChangesNothing(
                defeated,
                new PlayerFrameInput(
                    new MovementAxes(1f, 0f),
                    Direction3D.Forward,
                    false,
                    false));
            AssertGameplayTickChangesNothing(
                defeated,
                new PlayerFrameInput(
                    MovementAxes.Zero,
                    Direction3D.Left,
                    false,
                    false));
            AssertGameplayTickChangesNothing(
                defeated,
                new PlayerFrameInput(
                    MovementAxes.Zero,
                    Direction3D.Forward,
                    false,
                    true));
            AssertGameplayTickChangesNothing(
                defeated,
                new PlayerFrameInput(
                    MovementAxes.Zero,
                    Direction3D.Forward,
                    true,
                    false));

            Assert.That(_announcer.AnnouncementCount, Is.EqualTo(announcementsAtDefeat));
            Assert.That(_view.IsShown, Is.True);

            ArenaRunTickResult restarted = TickUntilRestart(
                defeated.Snapshot.ArenaRunId,
                CreateInput(false));

            Assert.That(restarted.Snapshot.Status, Is.EqualTo(ArenaRunStatus.Playing));
            Assert.That(
                restarted.Snapshot.ArenaRunId,
                Is.Not.EqualTo(defeated.Snapshot.ArenaRunId));

            Position3D freshPosition = restarted.Snapshot.Player.Position;
            ArenaRunTickResult afterRestart = Tick(CreateInput(false));

            Assert.That(
                afterRestart.Snapshot.Player.Position.X,
                Is.EqualTo(freshPosition.X).Within(1e-4f));
            Assert.That(
                afterRestart.Snapshot.Player.Position.Z,
                Is.EqualTo(freshPosition.Z).Within(1e-4f));
        }

        [Test]
        public void DefeatAlert_IsAnnouncedOnce_AndTheFreshRunIsAnnouncedOnce()
        {
            CreateHarness();
            SeedEnemy();

            ArenaRunTickResult defeated = TickUntilStatus(
                ArenaRunStatus.Defeat,
                CreateInput(false));

            int announcementsAtDefeat = _announcer.AnnouncementCount;

            Assert.That(
                _announcer.LastAnnouncement,
                Is.EqualTo(
                    ArenaRunAnnouncer.DefeatAnnouncement + " — " + defeated.Hud.WaveText));
            Assert.That(_announcer.LastAnnouncement, Is.EqualTo("Defeat — Wave 1 of 3"));

            for (int index = 0; index < 5; index++)
            {
                Tick(CreateInput(false));

                Assert.That(_announcer.AnnouncementCount, Is.EqualTo(announcementsAtDefeat));
                Assert.That(_announcer.LastAnnouncement, Is.EqualTo("Defeat — Wave 1 of 3"));
            }

            ArenaRunTickResult restarted = TickUntilRestart(
                defeated.Snapshot.ArenaRunId,
                CreateInput(false));

            Assert.That(_announcer.AnnouncementCount, Is.EqualTo(announcementsAtDefeat + 1));
            Assert.That(
                _announcer.LastAnnouncement,
                Is.EqualTo(
                    restarted.Hud.WaveText + ArenaRunAnnouncer.NewWaveAnnouncementSuffix));

            int announcementsAfterRestart = _announcer.AnnouncementCount;

            Tick(CreateInput(false));

            Assert.That(_announcer.AnnouncementCount, Is.EqualTo(announcementsAfterRestart));
        }

        private void AssertGameplayTickChangesNothing(
            ArenaRunTickResult defeated,
            PlayerFrameInput input)
        {
            ArenaRunTickResult after = Tick(input);

            Assert.That(after.Snapshot.Status, Is.EqualTo(ArenaRunStatus.Defeat));
            Assert.That(after.Snapshot.Revision, Is.EqualTo(defeated.Snapshot.Revision));
            Assert.That(
                after.Snapshot.Player.Position.X,
                Is.EqualTo(defeated.Snapshot.Player.Position.X).Within(1e-4f));
            Assert.That(
                after.Snapshot.Player.Position.Z,
                Is.EqualTo(defeated.Snapshot.Player.Position.Z).Within(1e-4f));
            Assert.That(_view.IsShown, Is.True);
        }

        private void CreateHarness()
        {
            _input = new FixedInputSource(CreateInput(false));
            _harness = new PlayModeTickHarness(
                _registry,
                playerStart: new Position3D(19f, 0f, 0f),
                restartDelay: new GameDuration(1d),
                waveCatalog: CreateThreeWaveCatalog(),
                playerInputSource: _input);
        }

        private void SeedEnemy()
        {
            _harness.SeedEnemy(1UL, new Position3D(20.5f, 0f, 0f));
        }

        private ArenaRunTickResult Tick(PlayerFrameInput input)
        {
            _input.Input = input;

            ArenaRunTickResult result = _harness.TickFrame();

            Present(result);

            return result;
        }

        private void Present(ArenaRunTickResult result)
        {
            _presenter.Present(result.Snapshot, result.Hud);
            _wavePresenter.Present(result.Snapshot, result.Hud);
        }

        private ArenaRunTickResult TickUntilStatus(
            ArenaRunStatus status,
            PlayerFrameInput input)
        {
            for (int index = 0; index < MaxTicks; index++)
            {
                ArenaRunTickResult result = Tick(input);

                if (result.Snapshot.Status == status)
                {
                    return result;
                }
            }

            Assert.Fail("Run did not reach " + status + ".");
            return null;
        }

        private ArenaRunTickResult TickUntilRestart(
            ArenaRunId previousRunId,
            PlayerFrameInput input)
        {
            for (int index = 0; index < MaxTicks; index++)
            {
                ArenaRunTickResult result = Tick(input);

                if (result.Snapshot.Status == ArenaRunStatus.Playing
                    && result.Snapshot.ArenaRunId != previousRunId)
                {
                    return result;
                }
            }

            Assert.Fail("Run did not restart.");
            return null;
        }

        private static PlayerFrameInput CreateInput(bool attackPressed)
        {
            return new PlayerFrameInput(
                MovementAxes.Zero,
                Direction3D.Forward,
                attackPressed,
                false);
        }

        private static WaveCatalog CreateThreeWaveCatalog()
        {
            return new WaveCatalog(
                new[]
                {
                    new WaveDefinition(WaveNumber.First, 1),
                    new WaveDefinition(WaveNumber.FromValue(2), 1),
                    new WaveDefinition(WaveNumber.FromValue(3), 1),
                });
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

        private sealed class EmptyEnemyColliderRegistry : IEnemyColliderRegistry
        {
            public bool TryGetEnemyId(Collider collider, out EnemyId enemyId)
            {
                enemyId = EnemyId.None;
                return false;
            }
        }
    }
}
