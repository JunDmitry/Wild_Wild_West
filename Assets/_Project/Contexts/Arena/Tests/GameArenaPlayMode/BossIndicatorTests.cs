using System.Collections.Generic;
using Game.Arena.Application.Hud;
using Game.Arena.Application.ReadModels;
using Game.Arena.Domain.Aggregates.ArenaRun;
using Game.Arena.Domain.Combat;
using Game.Arena.Domain.Concurrency;
using Game.Arena.Domain.Geometry;
using Game.Arena.Domain.Identity;
using Game.Arena.Domain.Time;
using Game.Arena.Domain.Vitality;
using Game.Arena.Presentation.Accessibility;
using Game.Arena.Presentation.Hud;
using NUnit.Framework;
using TMPro;
using UnityEngine;

namespace Game.Arena.PlayMode.Tests
{
    [TestFixture]
    public sealed class BossIndicatorTests
    {
        private readonly List<GameObject> _objects = new();
        private readonly ArenaRunHudSnapshotMapper _mapper = new();
        private ArenaRunAnnouncer _announcer;
        private BossIndicatorPresenter _presenter;
        private HudView _hudView;
        private TextMeshProUGUI _bossLabel;
        private TextMeshProUGUI _bossText;
        private Transform _bossFill;

        [SetUp]
        public void SetUp()
        {
            _objects.Clear();

            GameObject root = Track(new GameObject("Hud"));
            _hudView = root.AddComponent<HudView>();
            _announcer = root.AddComponent<ArenaRunAnnouncer>();
            _presenter = root.AddComponent<BossIndicatorPresenter>();

            GameObject boss = Track(new GameObject("BossText"));
            boss.transform.SetParent(root.transform, false);
            _bossLabel = boss.AddComponent<TextMeshProUGUI>();
            _bossText = new GameObject().AddComponent<TextMeshProUGUI>();
            _bossText.transform.SetParent(boss.transform, true);

            GameObject bossFill = Track(new GameObject("BossFill"));
            bossFill.transform.SetParent(root.transform, false);
            _bossFill = bossFill.transform;
            _bossFill.localScale = new Vector3(1f, 1f, 1f);

            _hudView.BossLabelMesh = _bossLabel;
            _hudView.BossHealthTextMesh = _bossText;
            _hudView.BossFill = _bossFill;
            _presenter.HudView = _hudView;
            _presenter.Announcer = _announcer;
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
        public void Present_BossOutsideBounds_KeepsTheRegionHidden()
        {
            Present(CreateSnapshot(EnemyKind.Boss, 25f, 0f, 300, 300));

            Assert.That(_hudView.IsBossPresent, Is.False);
            Assert.That(_hudView.BossHealthText, Is.Empty);
            Assert.That(_bossLabel.text, Is.Empty);
            Assert.That(_announcer.AnnouncementCount, Is.Zero);
        }

        [Test]
        public void Present_BossCrossesIntoBounds_ShowsTheRegionAndAnnouncesOnce()
        {
            Present(CreateSnapshot(EnemyKind.Boss, 25f, 0f, 300, 300));
            Present(CreateSnapshot(EnemyKind.Boss, 5f, 0f, 300, 300));

            Assert.That(_hudView.IsBossPresent, Is.True);
            Assert.That(_hudView.BossLabel, Is.EqualTo("Boss"));
            Assert.That(_hudView.BossHealthText, Is.EqualTo("300 / 300"));
            Assert.That(_hudView.BossHealthFraction, Is.EqualTo(1f));
            Assert.That(_bossLabel.text, Is.EqualTo("Boss"));
            Assert.That(_bossText.text, Is.EqualTo("300 / 300"));
            Assert.That(_bossFill.localScale.x, Is.EqualTo(1f).Within(1e-4f));
            Assert.That(_announcer.AnnouncementCount, Is.EqualTo(1));
            Assert.That(
                _announcer.LastAnnouncement,
                Is.EqualTo(ArenaRunAnnouncer.BossEntersAnnouncement));

            Present(CreateSnapshot(EnemyKind.Boss, 5f, 0f, 300, 300));

            Assert.That(_announcer.AnnouncementCount, Is.EqualTo(1));
        }

        [Test]
        public void Present_BossDamaged_DecreasesTheRegionWithoutAnnouncing()
        {
            Present(CreateSnapshot(EnemyKind.Boss, 25f, 0f, 300, 300));
            Present(CreateSnapshot(EnemyKind.Boss, 5f, 0f, 300, 300));

            Present(CreateSnapshot(EnemyKind.Boss, 5f, 0f, 180, 300));

            Assert.That(_hudView.BossHealthCurrent, Is.EqualTo(180));
            Assert.That(_hudView.BossHealthText, Is.EqualTo("180 / 300"));
            Assert.That(_hudView.BossHealthFraction, Is.EqualTo(0.6f));
            Assert.That(_bossLabel.text, Is.EqualTo("Boss"));
            Assert.That(_bossText.text, Is.EqualTo("180 / 300"));
            Assert.That(_bossFill.localScale.x, Is.EqualTo(0.6f).Within(1e-4f));
            Assert.That(_announcer.AnnouncementCount, Is.EqualTo(1));
            Assert.That(
                _announcer.LastAnnouncement,
                Is.EqualTo(ArenaRunAnnouncer.BossEntersAnnouncement));
        }

        [Test]
        public void Present_BossDefeated_HidesTheRegionAndAnnouncesOnce()
        {
            Present(CreateSnapshot(EnemyKind.Boss, 25f, 0f, 300, 300));
            Present(CreateSnapshot(EnemyKind.Boss, 5f, 0f, 300, 300));
            Present(CreateSnapshot(EnemyKind.Boss, 5f, 0f, 180, 300));
            Present(CreateSnapshot());

            Assert.That(_hudView.IsBossPresent, Is.False);
            Assert.That(_hudView.BossHealthText, Is.Empty);
            Assert.That(_bossLabel.text, Is.Empty);
            Assert.That(_bossFill.localScale.x, Is.Zero);
            Assert.That(_announcer.AnnouncementCount, Is.EqualTo(2));
            Assert.That(
                _announcer.LastAnnouncement,
                Is.EqualTo(ArenaRunAnnouncer.BossDefeatedAnnouncement));

            Present(CreateSnapshot());

            Assert.That(_announcer.AnnouncementCount, Is.EqualTo(2));
        }

        [Test]
        public void Present_DefeatedRunWithTheBossAlive_KeepsTheLastHealth()
        {
            Present(CreateSnapshot(EnemyKind.Boss, 5f, 0f, 180, 300));
            Present(CreateSnapshot(
                ArenaRunStatus.Defeat,
                Enemy(EnemyKind.Boss, 5f, 0f, 180, 300)));

            Assert.That(_hudView.IsBossPresent, Is.True);
            Assert.That(_hudView.BossHealthCurrent, Is.EqualTo(180));
            Assert.That(_hudView.BossHealthText, Is.EqualTo("180 / 300"));

            Present(CreateSnapshot(
                ArenaRunStatus.Defeat,
                Enemy(EnemyKind.Boss, 5f, 0f, 120, 300)));

            Assert.That(_hudView.BossHealthCurrent, Is.EqualTo(120));
        }

        [Test]
        public void Present_FinalBossVictory_HidesTheRegionWithoutADefeatAnnouncement()
        {
            Present(CreateSnapshot(EnemyKind.Boss, 25f, 0f, 300, 300));
            Present(CreateSnapshot(EnemyKind.Boss, 5f, 0f, 300, 300));
            Present(CreateSnapshot(ArenaRunStatus.Victory));

            Assert.That(_hudView.IsBossPresent, Is.False);
            Assert.That(_announcer.AnnouncementCount, Is.EqualTo(1));
            Assert.That(
                _announcer.LastAnnouncement,
                Is.EqualTo(ArenaRunAnnouncer.BossEntersAnnouncement));
        }

        [Test]
        public void Present_RestartedRun_DoesNotAnnounceADefeat()
        {
            Present(CreateSnapshot(EnemyKind.Boss, 25f, 0f, 300, 300));
            Present(CreateSnapshot(EnemyKind.Boss, 5f, 0f, 300, 300));
            Present(CreateSnapshot(ArenaRunId.FromValue(2UL)));

            Assert.That(_announcer.AnnouncementCount, Is.EqualTo(1));
            Assert.That(
                _announcer.LastAnnouncement,
                Is.EqualTo(ArenaRunAnnouncer.BossEntersAnnouncement));
        }

        [Test]
        public void Present_RegionIsLabelledBossHealthAndTheValueIsLabelledBoss()
        {
            Assert.That(BossIndicatorPresenter.RegionLabel, Is.EqualTo("Boss health"));

            Present(CreateSnapshot(EnemyKind.Boss, 5f, 0f, 300, 300));

            Assert.That(_hudView.BossLabel, Is.EqualTo("Boss"));
        }

        [Test]
        public void Present_NullArguments_Throw()
        {
            ArenaRunSnapshot snapshot = CreateSnapshot(EnemyKind.Boss, 5f, 0f, 300, 300);
            ArenaRunHudSnapshot hud = _mapper.Map(snapshot);

            Assert.Throws<System.ArgumentNullException>(() => _presenter.Present(null, hud));
            Assert.Throws<System.ArgumentNullException>(() => _presenter.Present(snapshot, null));
        }

        private void Present(ArenaRunSnapshot snapshot)
        {
            _presenter.Present(snapshot, _mapper.Map(snapshot));
        }

        private static ArenaRunSnapshot CreateSnapshot(params EnemySnapshot[] enemies)
        {
            return CreateSnapshot(ArenaRunId.FromValue(1UL), ArenaRunStatus.Playing, enemies);
        }

        private static ArenaRunSnapshot CreateSnapshot(ArenaRunId arenaRunId)
        {
            return CreateSnapshot(arenaRunId, ArenaRunStatus.Playing, new EnemySnapshot[0]);
        }

        private static ArenaRunSnapshot CreateSnapshot(ArenaRunStatus status, params EnemySnapshot[] enemies)
        {
            return CreateSnapshot(ArenaRunId.FromValue(1UL), status, enemies);
        }

        private static ArenaRunSnapshot CreateSnapshot(
            EnemyKind kind,
            float x,
            float z,
            int currentHealth,
            int maximumHealth)
        {
            return CreateSnapshot(Enemy(kind, x, z, currentHealth, maximumHealth));
        }

        private static ArenaRunSnapshot CreateSnapshot(
            ArenaRunId arenaRunId,
            ArenaRunStatus status,
            params EnemySnapshot[] enemies)
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
                enemies,
                new WaveSnapshot(WaveNumber.First, WavePhase.BossCombat, 0, BossStatus.Alive),
                WaveNumber.FromValue(3));
        }

        private static EnemySnapshot Enemy(
            EnemyKind kind,
            float x,
            float z,
            int currentHealth,
            int maximumHealth)
        {
            Health health = Health.Full(maximumHealth);
            int damage = maximumHealth - currentHealth;

            if (damage > 0)
            {
                health = health
                    .ApplyDamage(DamageAmount.FromPoints(damage))
                    .RemainingHealth;
            }

            float radius = kind == EnemyKind.Boss ? 1f : 0.5f;

            return new EnemySnapshot(
                EnemyId.FromValue(2UL),
                kind,
                new Position3D(x, 0f, z),
                health,
                CollisionRadius.FromValue(radius),
                new GameTimePoint(0d),
                false,
                default);
        }

        private GameObject Track(GameObject gameObject)
        {
            _objects.Add(gameObject);
            return gameObject;
        }
    }
}
