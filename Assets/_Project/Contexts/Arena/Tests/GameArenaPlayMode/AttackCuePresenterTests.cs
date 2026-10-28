using System;
using System.Collections.Generic;
using Game.Arena.Application.ReadModels;
using Game.Arena.Domain.Aggregates.ArenaRun;
using Game.Arena.Domain.Combat;
using Game.Arena.Domain.Concurrency;
using Game.Arena.Domain.Geometry;
using Game.Arena.Domain.Identity;
using Game.Arena.Domain.Time;
using Game.Arena.Domain.Vitality;
using Game.Arena.Infrastructure.Targeting;
using Game.Arena.Presentation.Accessibility;
using Game.Arena.Presentation.Combat;
using NUnit.Framework;
using UnityEngine;

namespace Game.Arena.PlayMode.Tests
{
    [TestFixture]
    public sealed class AttackCuePresenterTests
    {
        private const int MeleeDamage = 20;
        private const float HarnessMeleeRange = 25f;
        private const int TicksAfterAttackStart = 4;

        private readonly List<GameObject> _objects = new();
        private TestEnemyColliderRegistry _colliders;
        private PlayModeTickHarness _harness;

        [SetUp]
        public void SetUp()
        {
            _objects.Clear();
            _colliders = new TestEnemyColliderRegistry();
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
        public void AimMarker_ShownForRangedPlayer()
        {
            FirearmAimPresenter presenter = CreateAimPresenter(out _, out _);

            presenter.Present(CreateSnapshot(WeaponKind.Ranged));

            Assert.That(presenter.IsAimVisible, Is.True);
        }

        [Test]
        public void AimMarker_HiddenForMeleePlayer()
        {
            FirearmAimPresenter presenter = CreateAimPresenter(out _, out LineRenderer line);

            presenter.Present(CreateSnapshot(WeaponKind.Melee));

            Assert.That(presenter.IsAimVisible, Is.False);
            Assert.That(line.enabled, Is.False);
        }

        [Test]
        public void AimMarker_HiddenWhenRunIsNotPlaying()
        {
            FirearmAimPresenter presenter = CreateAimPresenter(out _, out LineRenderer line);

            presenter.Present(CreateSnapshot(WeaponKind.Ranged, status: ArenaRunStatus.Defeat));

            Assert.That(presenter.IsAimVisible, Is.False);
            Assert.That(line.enabled, Is.False);
        }

        [Test]
        public void AimMarker_HiddenForDefeatedPlayer()
        {
            FirearmAimPresenter presenter = CreateAimPresenter(out _, out LineRenderer line);

            presenter.Present(CreateSnapshot(WeaponKind.Ranged, playerDefeated: true));

            Assert.That(presenter.IsAimVisible, Is.False);
            Assert.That(line.enabled, Is.False);
        }

        [Test]
        public void AimMarker_FollowsViewDirection()
        {
            FirearmAimPresenter presenter = CreateAimPresenter(out Transform viewTransform, out LineRenderer line);

            presenter.Present(CreateSnapshot(WeaponKind.Ranged));

            Assert.That(line.GetPosition(1).z, Is.GreaterThan(0f));

            viewTransform.rotation = Quaternion.Euler(0f, 90f, 0f);
            presenter.Refresh();

            Assert.That(line.GetPosition(1).x, Is.GreaterThan(0f));
            Assert.That(Mathf.Abs(line.GetPosition(1).z), Is.LessThan(1e-3f));
        }

        [Test]
        public void AimDirection_AnnouncedOncePerEquip()
        {
            FirearmAimPresenter presenter = CreateAimPresenter(out _, out _, out ArenaRunAnnouncer announcer);

            presenter.Present(CreateSnapshot(WeaponKind.Ranged));
            presenter.Present(CreateSnapshot(WeaponKind.Ranged));

            Assert.That(announcer.AnnouncementCount, Is.EqualTo(1));
            Assert.That(announcer.LastAnnouncement, Is.EqualTo(ArenaRunAnnouncer.AimDirectionAnnouncement));

            presenter.Present(CreateSnapshot(WeaponKind.Melee));
            presenter.Present(CreateSnapshot(WeaponKind.Ranged));

            Assert.That(announcer.AnnouncementCount, Is.EqualTo(2));
        }

        [Test]
        public void Swing_HiddenWithoutPendingAttack()
        {
            MeleeSwingPresenter presenter = CreateSwingPresenter();

            presenter.Present(CreateSnapshot(WeaponKind.Melee));

            Assert.That(presenter.IsSwingVisible, Is.False);
        }

        [Test]
        public void Swing_ShownWhileMeleeAttackIsPending()
        {
            MeleeSwingPresenter presenter = CreateSwingPresenter();

            presenter.Present(CreateSnapshot(WeaponKind.Melee, meleePending: true));

            Assert.That(presenter.IsSwingVisible, Is.True);
            Assert.That(presenter.SwingDirection.Dot(Direction3D.Forward), Is.GreaterThan(0.99f));
        }

        [Test]
        public void Swing_HiddenForPendingRangedAttack()
        {
            MeleeSwingPresenter presenter = CreateSwingPresenter();

            presenter.Present(CreateSnapshot(WeaponKind.Ranged, rangedPending: true));

            Assert.That(presenter.IsSwingVisible, Is.False);
        }

        [Test]
        public void Swing_HiddenForDefeatedPlayer()
        {
            MeleeSwingPresenter presenter = CreateSwingPresenter();

            presenter.Present(CreateSnapshot(WeaponKind.Melee, meleePending: true, playerDefeated: true));

            Assert.That(presenter.IsSwingVisible, Is.False);
        }

        [Test]
        public void Swing_HiddenWhenRunIsNotPlaying()
        {
            MeleeSwingPresenter presenter = CreateSwingPresenter();

            presenter.Present(CreateSnapshot(
                WeaponKind.Melee,
                meleePending: true,
                status: ArenaRunStatus.Victory));

            Assert.That(presenter.IsSwingVisible, Is.False);
        }

        [Test]
        public void Swing_FollowsViewDirectionDuringWindup()
        {
            MeleeSwingPresenter presenter = CreateSwingPresenter(out Transform viewTransform, out _);

            presenter.Present(CreateSnapshot(WeaponKind.Melee, meleePending: true));

            Assert.That(presenter.SwingDirection.Dot(Direction3D.Forward), Is.GreaterThan(0.99f));

            viewTransform.rotation = Quaternion.Euler(0f, 90f, 0f);
            presenter.Refresh();

            Assert.That(presenter.SwingDirection.Dot(Direction3D.Right), Is.GreaterThan(0.99f));
        }

        [Test]
        public void Swing_HiddenAfterImpactResolves()
        {
            _harness = new PlayModeTickHarness(_colliders, HarnessMeleeRange);
            SeedEnemy(1UL, new Vector3(0f, 0f, 21f));

            _harness.Tick(false, Direction3D.Forward, true);
            ArenaRunSnapshot windup = _harness.Tick(true, Direction3D.Forward).Snapshot;

            for (int index = 0; index < TicksAfterAttackStart - 1; index++)
            {
                _harness.TickIdle();
            }

            MeleeSwingPresenter presenter = CreateSwingPresenter();
            presenter.Present(windup);
            Assert.That(presenter.IsSwingVisible, Is.True);

            ArenaRunSnapshot afterImpact = _harness.TickIdle().Snapshot;
            presenter.Present(afterImpact);
            Assert.That(presenter.IsSwingVisible, Is.False);
        }

        [Test]
        public void Swing_ArcCoversEveryEnemyDamagedByMeleeImpact()
        {
            _harness = new PlayModeTickHarness(_colliders, HarnessMeleeRange);

            EnemyId ahead = SeedEnemy(1UL, new Vector3(0f, 0f, 21f));
            EnemyId behind = SeedEnemy(2UL, new Vector3(0f, 0f, -21f));
            EnemyId beside = SeedEnemy(3UL, new Vector3(21f, 0f, 0f));

            _harness.Tick(false, Direction3D.Forward, true);
            ArenaRunSnapshot windup = _harness.Tick(true, Direction3D.Forward).Snapshot;

            for (int index = 0; index < TicksAfterAttackStart - 1; index++)
            {
                _harness.TickIdle();
            }

            MeleeSwingPresenter presenter = CreateSwingPresenter();
            presenter.Present(windup);

            PendingPlayerAttack presentedAttack = windup.Player.PendingAttack;

            Assert.That(presenter.SwingView.Range, Is.EqualTo(presentedAttack.Range));
            Assert.That(presenter.SwingView.Arc, Is.EqualTo(presentedAttack.Arc));
            Assert.That(presenter.IsSwingVisible, Is.True);

            EnemyId[] enemies = { ahead, behind, beside };

            for (int index = 0; index < enemies.Length; index++)
            {
                Assert.That(
                    _harness.ReadEnemyHealth(enemies[index]),
                    Is.EqualTo(PlayModeTickHarness.EnemyHealth - MeleeDamage),
                    "Enemy " + enemies[index] + " was not damaged by the impact.");

                Assert.That(
                    presenter.SwingView.Contains(FindEnemy(windup, enemies[index]).Position),
                    Is.True,
                    "The drawn swing does not cover Enemy " + enemies[index] + ".");
            }
        }

        [Test]
        public void Swing_DrawsThePendingAttacksShape()
        {
            const float narrowRange = 6f;
            AttackArc narrowArc = AttackArc.FromDegrees(90f);
            _harness = new PlayModeTickHarness(_colliders, narrowRange, narrowArc);

            _harness.Tick(false, Direction3D.Forward, true);
            ArenaRunSnapshot windup = _harness.Tick(true, Direction3D.Forward).Snapshot;

            MeleeSwingPresenter presenter = CreateSwingPresenter();
            presenter.Present(windup);

            PendingPlayerAttack presentedAttack = windup.Player.PendingAttack;
            Position3D origin = windup.Player.Position;

            Assert.That(presentedAttack.Range.Value, Is.EqualTo(narrowRange));
            Assert.That(presentedAttack.Arc.Degrees, Is.EqualTo(narrowArc.Degrees));
            Assert.That(presenter.SwingView.Range, Is.EqualTo(presentedAttack.Range));
            Assert.That(presenter.SwingView.Arc, Is.EqualTo(presentedAttack.Arc));

            Assert.That(
                presenter.SwingView.Contains(new Position3D(origin.X, origin.Y, origin.Z + 4f)),
                Is.True);

            Assert.That(
                presenter.SwingView.Contains(new Position3D(origin.X, origin.Y, origin.Z + 10f)),
                Is.False,
                "A point beyond the presented range must be outside the swing.");

            Assert.That(
                presenter.SwingView.Contains(new Position3D(origin.X, origin.Y, origin.Z - 4f)),
                Is.False,
                "A point behind the swing direction must be outside the narrow arc.");
        }

        private FirearmAimPresenter CreateAimPresenter(
            out Transform viewTransform,
            out LineRenderer line)
        {
            return CreateAimPresenter(out viewTransform, out line, out _);
        }

        private FirearmAimPresenter CreateAimPresenter(
            out Transform viewTransform,
            out LineRenderer line,
            out ArenaRunAnnouncer announcer)
        {
            GameObject root = Track(new GameObject("AimCues"));

            GameObject markerObject = Track(new GameObject("AimMarker"));
            markerObject.transform.SetParent(root.transform);

            line = markerObject.AddComponent<LineRenderer>();
            FirearmAimMarkerView marker = markerObject.AddComponent<FirearmAimMarkerView>();

            viewTransform = Track(new GameObject("View")).transform;
            viewTransform.SetParent(root.transform);

            announcer = root.AddComponent<ArenaRunAnnouncer>();

            ViewDirectionTracker tracker = viewTransform.gameObject.AddComponent<ViewDirectionTracker>();
            tracker.ViewTransform = viewTransform;

            FirearmAimPresenter presenter = root.AddComponent<FirearmAimPresenter>();
            presenter.MarkerView = marker;
            presenter.ViewDirection = tracker;
            presenter.Announcer = announcer;

            return presenter;
        }

        private MeleeSwingPresenter CreateSwingPresenter()
        {
            return CreateSwingPresenter(out _, out _);
        }

        private MeleeSwingPresenter CreateSwingPresenter(
            out Transform viewTransform,
            out MeleeSwingView swingView)
        {
            GameObject root = Track(new GameObject("SwingCues"));

            GameObject swingObject = Track(new GameObject("Swing"));
            swingObject.transform.SetParent(root.transform);
            swingObject.AddComponent<LineRenderer>();

            swingView = swingObject.AddComponent<MeleeSwingView>();

            viewTransform = Track(new GameObject("View")).transform;
            viewTransform.SetParent(root.transform);

            ViewDirectionTracker tracker = viewTransform.gameObject.AddComponent<ViewDirectionTracker>();
            tracker.ViewTransform = viewTransform;

            MeleeSwingPresenter presenter = root.AddComponent<MeleeSwingPresenter>();
            presenter.SwingView = swingView;
            presenter.ViewDirection = tracker;

            return presenter;
        }

        private EnemyId SeedEnemy(ulong value, Vector3 position)
        {
            EnemyId enemyId = EnemyId.FromValue(value);

            GameObject enemyObject = Track(new GameObject("Enemy" + value));
            enemyObject.transform.position = position;

            BoxCollider collider = enemyObject.AddComponent<BoxCollider>();
            collider.size = new Vector3(1f, 2f, 1f);

            Physics.SyncTransforms();
            _colliders.Register(collider, enemyId);
            _harness.SeedEnemy(value, new Position3D(position.x, position.y, position.z));

            return enemyId;
        }

        private GameObject Track(GameObject gameObject)
        {
            _objects.Add(gameObject);
            return gameObject;
        }

        private static EnemySnapshot FindEnemy(ArenaRunSnapshot snapshot, EnemyId enemyId)
        {
            for (int index = 0; index < snapshot.Enemies.Count; index++)
            {
                if (snapshot.Enemies[index].EnemyId == enemyId)
                {
                    return snapshot.Enemies[index];
                }
            }

            throw new InvalidOperationException("Enemy is not present in the snapshot: " + enemyId);
        }

        private static ArenaRunSnapshot CreateSnapshot(
            WeaponKind selectedWeapon,
            bool rangedPending = false,
            bool meleePending = false,
            ArenaRunStatus status = ArenaRunStatus.Playing,
            bool playerDefeated = false)
        {
            PlayerId playerId = PlayerId.FromValue(1UL);
            PendingPlayerAttack pendingAttack = meleePending
                ? new PendingPlayerAttack(
                    AttackId.None.Next(),
                    playerId,
                    WeaponKind.Melee,
                    Distance.FromValue(2f),
                    AttackArc.Full,
                    new GameTimePoint(0d),
                    new GameTimePoint(0.3d))
                : rangedPending
                    ? new PendingPlayerAttack(
                        AttackId.None.Next(),
                        playerId,
                        WeaponKind.Ranged,
                        Distance.FromValue(50f),
                        AttackArc.Full,
                        new GameTimePoint(0d),
                        new GameTimePoint(0d))
                    : default;

            Health health = playerDefeated
                ? Health.Full(100).ApplyDamage(DamageAmount.FromPoints(100)).RemainingHealth
                : Health.Full(100);

            PlayerSnapshot player = new(
                playerId,
                Position3D.Zero,
                health,
                selectedWeapon,
                new GameTimePoint(0d),
                new GameTimePoint(0d),
                rangedPending || meleePending,
                pendingAttack);

            return new ArenaRunSnapshot(
                ArenaRunId.FromValue(1UL),
                AggregateRevision.Initial,
                status,
                new GameTimePoint(0d),
                new ArenaBounds(-20f, 20f, -20f, 20f, 0f),
                player,
                Array.Empty<EnemySnapshot>(),
                new WaveSnapshot(WaveNumber.First, WavePhase.RegularCombat, 0, BossStatus.NotSpawned),
                WaveNumber.FromValue(3));
        }

        private sealed class TestEnemyColliderRegistry : IEnemyColliderRegistry
        {
            private readonly Dictionary<Collider, EnemyId> _enemyIds = new();

            public void Register(Collider collider, EnemyId enemyId)
            {
                _enemyIds.Add(collider, enemyId);
            }

            public void Register(GameObject gameObject, EnemyId enemyId)
            {
                _enemyIds.Add(gameObject.GetComponent<Collider>(), enemyId);
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
