using System.Collections.Generic;
using Game.Arena.Application.ReadModels;
using Game.Arena.Domain.Aggregates.ArenaRun;
using Game.Arena.Domain.Combat;
using Game.Arena.Domain.Concurrency;
using Game.Arena.Domain.Geometry;
using Game.Arena.Domain.Identity;
using Game.Arena.Domain.Time;
using Game.Arena.Domain.Vitality;
using Game.Arena.Presentation.Accessibility;
using Game.Arena.Presentation.Combat;
using NUnit.Framework;
using UnityEngine;

namespace Game.Arena.PlayMode.Tests
{
    [TestFixture]
    public sealed class EnemyAttackCueTests
    {
        private static readonly EnemyId Attacker = EnemyId.FromValue(1UL);

        private readonly List<GameObject> _objects = new();
        private ArenaRunAnnouncer _announcer;
        private EnemyAttackCuePresenter _presenter;

        [SetUp]
        public void SetUp()
        {
            _objects.Clear();

            GameObject root = Track(new GameObject("EnemyAttackCues"));
            _announcer = root.AddComponent<ArenaRunAnnouncer>();

            GameObject templateObject = Track(new GameObject("CueTemplate"));
            templateObject.transform.SetParent(root.transform);
            templateObject.AddComponent<LineRenderer>();
            EnemyAttackCueView template = templateObject.AddComponent<EnemyAttackCueView>();
            templateObject.SetActive(false);

            _presenter = root.AddComponent<EnemyAttackCuePresenter>();
            _presenter.Template = template;
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
        public void Cue_ShownWhileEnemyAttackIsPending()
        {
            _presenter.Present(CreateSnapshot(Enemy(1UL, 2f, 3f, attackPending: true)));

            Assert.That(_presenter.ActiveCount, Is.EqualTo(1));
            Assert.That(_presenter.IsShownFor(Attacker), Is.True);
            Assert.That(_presenter.PositionOf(Attacker).X, Is.EqualTo(2f).Within(1e-4f));
            Assert.That(_presenter.PositionOf(Attacker).Z, Is.EqualTo(3f).Within(1e-4f));
        }

        [Test]
        public void Cue_HiddenWhenAttackResolves()
        {
            _presenter.Present(CreateSnapshot(Enemy(1UL, 2f, 3f, attackPending: true)));
            _presenter.Present(CreateSnapshot(Enemy(1UL, 2f, 3f)));

            Assert.That(_presenter.ActiveCount, Is.EqualTo(0));
            Assert.That(_presenter.IsShownFor(Attacker), Is.False);
        }

        [Test]
        public void Cue_HiddenWhenRunIsTerminal()
        {
            _presenter.Present(CreateSnapshot(Enemy(1UL, 2f, 3f, attackPending: true)));

            _presenter.Present(CreateSnapshot(
                ArenaRunStatus.Victory,
                false,
                Enemy(1UL, 2f, 3f, attackPending: true)));

            Assert.That(_presenter.ActiveCount, Is.EqualTo(0));
        }

        [Test]
        public void Cue_HiddenWhenPlayerIsDefeated()
        {
            _presenter.Present(CreateSnapshot(
                ArenaRunStatus.Playing,
                true,
                Enemy(1UL, 2f, 3f, attackPending: true)));

            Assert.That(_presenter.ActiveCount, Is.EqualTo(0));
        }

        [Test]
        public void Cue_NotShownForEnemyWithoutPendingAttack()
        {
            _presenter.Present(CreateSnapshot(Enemy(1UL, 2f, 3f)));

            Assert.That(_presenter.ActiveCount, Is.EqualTo(0));
            Assert.That(_presenter.IsShownFor(Attacker), Is.False);
        }

        [Test]
        public void Cue_ShownForEveryAttackingEnemy()
        {
            _presenter.Present(CreateSnapshot(
                Enemy(1UL, 2f, 3f, attackPending: true),
                Enemy(2UL, -4f, 5f, attackPending: true)));

            Assert.That(_presenter.ActiveCount, Is.EqualTo(2));
            Assert.That(_presenter.IsShownFor(Attacker), Is.True);
            Assert.That(_presenter.IsShownFor(EnemyId.FromValue(2UL)), Is.True);
        }

        [Test]
        public void Cue_AnnouncedOnceWhenAttackStarts()
        {
            _presenter.Present(CreateSnapshot(Enemy(1UL, 2f, 3f, attackPending: true)));
            _presenter.Present(CreateSnapshot(Enemy(1UL, 2f, 3f, attackPending: true)));

            Assert.That(_announcer.AnnouncementCount, Is.EqualTo(1));
            Assert.That(
                _announcer.LastAnnouncement,
                Is.EqualTo(ArenaRunAnnouncer.EnemyAttackIncomingAnnouncement));
        }

        [Test]
        public void Cue_AnnouncedAgainForANewPendingAttack()
        {
            _presenter.Present(CreateSnapshot(Enemy(1UL, 2f, 3f, attackPending: true)));
            _presenter.Present(CreateSnapshot(Enemy(1UL, 2f, 3f)));
            _presenter.Present(CreateSnapshot(Enemy(1UL, 2f, 3f, attackPending: true)));

            Assert.That(_announcer.AnnouncementCount, Is.EqualTo(2));
        }

        [Test]
        public void Cue_CarriesTheEnemyAttackStatusLabel()
        {
            _presenter.Present(CreateSnapshot(Enemy(1UL, 2f, 3f, attackPending: true)));

            Assert.That(
                _presenter.LabelOf(Attacker),
                Is.EqualTo(EnemyAttackCueView.StatusLabel));
        }

        [Test]
        public void Cue_FollowsTheEnemyPosition()
        {
            _presenter.Present(CreateSnapshot(Enemy(1UL, 2f, 3f, attackPending: true)));
            _presenter.Present(CreateSnapshot(Enemy(1UL, 6f, 7f, attackPending: true)));

            Assert.That(_presenter.PositionOf(Attacker).X, Is.EqualTo(6f).Within(1e-4f));
            Assert.That(_presenter.PositionOf(Attacker).Z, Is.EqualTo(7f).Within(1e-4f));
        }

        [Test]
        public void Cue_IsNotFocusable()
        {
            _presenter.Present(CreateSnapshot(Enemy(1UL, 2f, 3f, attackPending: true)));

            EnemyAttackCueView[] cues = _presenter.GetComponentsInChildren<EnemyAttackCueView>();

            for (int index = 0; index < cues.Length; index++)
            {
                Assert.That(cues[index].GetComponent<Collider>(), Is.Null);
                Assert.That(cues[index].GetComponent<Collider2D>(), Is.Null);
            }
        }

        private static EnemySnapshot Enemy(
            ulong value,
            float x,
            float z,
            bool attackPending = false)
        {
            EnemyId enemyId = EnemyId.FromValue(value);

            PendingEnemyAttack pendingAttack = attackPending
                ? new PendingEnemyAttack(
                    AttackId.None.Next(),
                    enemyId,
                    new GameTimePoint(0d),
                    new GameTimePoint(0.2d))
                : default;

            return new EnemySnapshot(
                enemyId,
                EnemyKind.Regular,
                new Position3D(x, 0f, z),
                Health.Full(PlayModeTickHarness.EnemyHealth),
                CollisionRadius.FromValue(0.5f),
                new GameTimePoint(0d),
                attackPending,
                pendingAttack);
        }

        private static ArenaRunSnapshot CreateSnapshot(params EnemySnapshot[] enemies)
        {
            return CreateSnapshot(ArenaRunStatus.Playing, false, enemies);
        }

        private static ArenaRunSnapshot CreateSnapshot(
            ArenaRunStatus status,
            bool playerDefeated,
            params EnemySnapshot[] enemies)
        {
            Health health = playerDefeated
                ? Health.Full(100).ApplyDamage(DamageAmount.FromPoints(100)).RemainingHealth
                : Health.Full(100);

            PlayerSnapshot player = new(
                PlayerId.FromValue(1UL),
                Position3D.Zero,
                health,
                WeaponKind.Melee,
                new GameTimePoint(0d),
                new GameTimePoint(0d),
                false,
                default);

            return new ArenaRunSnapshot(
                ArenaRunId.FromValue(1UL),
                AggregateRevision.Initial,
                status,
                new GameTimePoint(0d),
                new ArenaBounds(-20f, 20f, -20f, 20f, 0f),
                player,
                enemies,
                new WaveSnapshot(WaveNumber.First, WavePhase.RegularCombat, 0, BossStatus.NotSpawned),
                WaveNumber.FromValue(3));
        }

        private GameObject Track(GameObject gameObject)
        {
            _objects.Add(gameObject);
            return gameObject;
        }
    }
}
