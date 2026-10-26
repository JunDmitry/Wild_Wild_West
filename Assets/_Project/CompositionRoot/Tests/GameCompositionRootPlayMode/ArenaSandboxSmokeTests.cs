using System;
using System.Collections;
using System.Collections.Generic;
using Game.Arena.Application.Ports;
using Game.Arena.Application.ReadModels;
using Game.Arena.Application.Sessions;
using Game.Arena.Domain.Aggregates.ArenaRun;
using Game.Arena.Domain.Combat;
using Game.Arena.Domain.Configuration;
using Game.Arena.Domain.Geometry;
using Game.Arena.Domain.Identity;
using Game.Arena.Domain.Interactions.Movement;
using Game.Arena.Domain.Interactions.Spawn;
using Game.Arena.Domain.Repositories;
using Game.Arena.Domain.Time;
using Game.Arena.Domain.Vitality;
using Game.Arena.Infrastructure.Input;
using Game.Arena.Infrastructure.Targeting;
using Game.Arena.Presentation.Accessibility;
using NUnit.Framework;
using UnityEngine;
using UnityEngine.TestTools;
using VContainer;

namespace Game.CompositionRoot.PlayMode.Tests
{
    /// <summary>
    /// The end-to-end Arena sandbox smoke (T-8.6): every scenario drives the real
    /// bootstrap-to-arena composition graph — the real scene binder, presenters,
    /// HUD, and tick driver — over real frames, with input supplied through the
    /// public <see cref="UnityPlayerInputSource.Device"/> seam.
    ///
    /// Scenarios 5-7 of the request (wave progression, defeat plus reload, and
    /// victory) drive the same graph through the configuration seam and the
    /// binder's scene-objects override: the rig hands the binder a recording
    /// <c>IArenaSceneReloader</c>, so defeat and victory are observed without a
    /// real scene load.
    /// </summary>
    [TestFixture]
    public sealed class ArenaSandboxSmokeTests
    {
        private const string AttackButton = "Fire1";
        private const string VerticalAxis = "Vertical";
        private const string HorizontalAxis = "Horizontal";

        private static readonly EnemyId FrontEnemyId = EnemyId.FromValue(5001UL);
        private static readonly EnemyId SideEnemyId = EnemyId.FromValue(5002UL);
        private static readonly EnemyId MeleeFirstId = EnemyId.FromValue(5011UL);
        private static readonly EnemyId MeleeSecondId = EnemyId.FromValue(5012UL);
        private static readonly EnemyId MeleeThirdId = EnemyId.FromValue(5013UL);

        private static readonly EnemyId[] MeleeEnemyIds =
        {
            MeleeFirstId,
            MeleeSecondId,
            MeleeThirdId,
        };

        private const int EnemyHealth = 50;
        private const int RangedDamage = 10;
        private const int MeleeDamage = 20;

        private readonly List<ArenaSceneTestRig> _rigs = new();
        private readonly List<GameObject> _colliders = new();

        private FakeInputDevice _device;

        [TearDown]
        public void TearDown()
        {
            for (int index = 0; index < _rigs.Count; index++)
            {
                _rigs[index].Dispose();
            }

            _rigs.Clear();

            for (int index = 0; index < _colliders.Count; index++)
            {
                UnityEngine.Object.DestroyImmediate(_colliders[index]);
            }

            _colliders.Clear();
        }

        [UnityTest]
        public IEnumerator EnteringTheArena_CreatesExactlyOneFreshRun()
        {
            ArenaSceneTestRig rig = CreateRig();

            rig.BinderObject.SetActive(true);
            yield return null;

            ArenaRunSession session = rig.Scope.Container.Resolve<ArenaRunSession>();

            yield return WaitUntil(() => session.HasActiveRun, 2f);

            Assert.That(
                session.HasActiveRun,
                Is.True,
                "Entering the arena starts exactly one run.");

            IArenaRunRepository repository =
                rig.Scope.Container.Resolve<IArenaRunRepository>();

            Assert.That(
                repository.TryGet(session.ActiveArenaRunId, out ArenaRun run),
                Is.True);

            Assert.That(
                session.ActiveArenaRunId,
                Is.EqualTo(ArenaRunId.FromValue(1UL)),
                "The process id sequence allocated the first, fresh run identity.");

            Assert.That(run.PlayerId.IsNone, Is.False, "The run owns a fresh player identity.");
            Assert.That(run.Status, Is.EqualTo(ArenaRunStatus.Playing));

            Assert.That(
                repository.TryGet(ArenaRunId.FromValue(2UL), out _),
                Is.False,
                "No second run exists.");

            ArenaRunId activeId = session.ActiveArenaRunId;
            yield return null;
            yield return null;

            Assert.That(
                session.ActiveArenaRunId,
                Is.EqualTo(activeId),
                "Later frames keep the same single active run.");
        }

        [UnityTest]
        public IEnumerator SteeringIntoTheArenaBound_KeepsThePublishedPositionInside()
        {
            ArenaSceneTestRig rig = CreateRig();

            rig.BinderObject.SetActive(true);
            yield return null;

            ArenaRunSession session = rig.Scope.Container.Resolve<ArenaRunSession>();

            yield return WaitUntil(() => session.HasActiveRun, 2f);

            IArenaRunRepository repository =
                rig.Scope.Container.Resolve<IArenaRunRepository>();

            Assert.That(
                repository.TryGet(session.ActiveArenaRunId, out ArenaRun run),
                Is.True);

            _device.Axes[VerticalAxis] = 1f;

            yield return WaitUntil(
                () => run.PlayerPosition.Z >= run.ArenaBounds.MaximumZ - 0.1f,
                12f);

            for (int index = 0; index < 5; index++)
            {
                yield return null;
            }

            _device.Axes.Remove(VerticalAxis);

            Assert.That(
                run.PlayerPosition.Z,
                Is.GreaterThan(run.ArenaBounds.MaximumZ - 1f),
                "Steering forward moved the published position to the bound.");

            Assert.That(
                run.PlayerPosition.Z,
                Is.LessThanOrEqualTo(run.ArenaBounds.MaximumZ + 1e-3f),
                "The published position stays inside the arena bound.");

            Assert.That(
                run.PlayerPosition.X,
                Is.InRange(run.ArenaBounds.MinimumX - 1e-3f, run.ArenaBounds.MaximumX + 1e-3f));
        }

        [UnityTest]
        public IEnumerator RangedAttack_HitsATargetInFront_AndMissesIntoEmptySpace()
        {
            ArenaSceneTestRig rig = CreateRig();

            rig.BinderObject.SetActive(true);
            yield return null;

            ArenaRunSession session = rig.Scope.Container.Resolve<ArenaRunSession>();

            yield return WaitUntil(() => session.HasActiveRun, 2f);

            IArenaRunRepository repository =
                rig.Scope.Container.Resolve<IArenaRunRepository>();

            Assert.That(
                repository.TryGet(session.ActiveArenaRunId, out ArenaRun run),
                Is.True);

            IEnemyColliderRegistration colliders =
                rig.Scope.Container.Resolve<IEnemyColliderRegistration>();

            // The Arena spawns enemies from its perimeter band: a position is
            // only a valid spawn when it lies outside the bounds and inside the
            // five-unit band around them.
            SeedEnemy(run, FrontEnemyId, new Position3D(0f, 0f, 21f));
            RegisterCollider(colliders, FrontEnemyId, new Vector3(0f, 0f, 21f));

            SeedEnemy(run, SideEnemyId, new Position3D(21f, 0f, 0f));
            RegisterCollider(colliders, SideEnemyId, new Vector3(21f, 0f, 0f));

            Assert.That(run.SelectedWeapon, Is.EqualTo(WeaponKind.Ranged));
            Assert.That(HealthOf(run, FrontEnemyId), Is.EqualTo(EnemyHealth));
            Assert.That(HealthOf(run, SideEnemyId), Is.EqualTo(EnemyHealth));

            yield return Strike();

            yield return WaitUntil(
                () => HealthOf(run, FrontEnemyId) < EnemyHealth,
                2f);

            Assert.That(
                HealthOf(run, FrontEnemyId),
                Is.EqualTo(EnemyHealth - RangedDamage),
                "A ranged attack damages the enemy in front of the player.");

            yield return WaitUntil(() => run.IsSelectedWeaponReady, 2f);

            yield return Strike();
            yield return WaitForSeconds(1f);

            Assert.That(
                HealthOf(run, SideEnemyId),
                Is.EqualTo(EnemyHealth),
                "An attack with no target in range records a miss and damages nothing.");
        }

        [UnityTest]
        public IEnumerator OneMeleeSwing_DamagesEveryEnemyInsideTheArc()
        {
            ArenaSceneTestRig rig = CreateRig();

            rig.BinderObject.SetActive(true);
            yield return null;

            ArenaRunSession session = rig.Scope.Container.Resolve<ArenaRunSession>();

            yield return WaitUntil(() => session.HasActiveRun, 2f);

            IArenaRunRepository repository =
                rig.Scope.Container.Resolve<IArenaRunRepository>();

            Assert.That(
                repository.TryGet(session.ActiveArenaRunId, out ArenaRun run),
                Is.True);

            IEnemyColliderRegistration colliders =
                rig.Scope.Container.Resolve<IEnemyColliderRegistration>();

            Dictionary<EnemyId, GameObject> tracked = new();

            SeedEnemy(run, MeleeFirstId, new Position3D(0f, 0f, 21f));
            tracked[MeleeFirstId] = RegisterCollider(
                colliders,
                MeleeFirstId,
                new Vector3(0f, 0f, 21f));

            SeedEnemy(run, MeleeSecondId, new Position3D(1.5f, 0f, 21f));
            tracked[MeleeSecondId] = RegisterCollider(
                colliders,
                MeleeSecondId,
                new Vector3(1.5f, 0f, 21f));

            SeedEnemy(run, MeleeThirdId, new Position3D(-1.5f, 0f, 21f));
            tracked[MeleeThirdId] = RegisterCollider(
                colliders,
                MeleeThirdId,
                new Vector3(-1.5f, 0f, 21f));

            // Enemies spawn outside the arena and walk in. Closing the distance
            // with the player is part of the scenario; the target colliders
            // follow the published positions so the melee sweep sees them.
            _device.Axes[VerticalAxis] = 1f;

            yield return WaitUntil(() => run.PlayerPosition.Z >= 16f, 8f);

            _device.Axes.Remove(VerticalAxis);

            float deadline = Time.realtimeSinceStartup + 8f;

            while (AreWithinMeleeReach(run) == false
                && Time.realtimeSinceStartup < deadline)
            {
                SyncColliders(run, tracked);

                yield return null;
            }

            SyncColliders(run, tracked);

            Assert.That(
                AreWithinMeleeReach(run),
                Is.True,
                "All three seeded enemies closed to melee range.");

            yield return SwitchToMelee(run);

            Assert.That(run.SelectedWeapon, Is.EqualTo(WeaponKind.Melee));

            yield return Strike();

            yield return WaitUntil(
                () => HealthOf(run, MeleeFirstId) < EnemyHealth
                    && HealthOf(run, MeleeSecondId) < EnemyHealth
                    && HealthOf(run, MeleeThirdId) < EnemyHealth,
                3f);

            int damagedHealth = EnemyHealth - MeleeDamage;

            Assert.That(HealthOf(run, MeleeFirstId), Is.EqualTo(damagedHealth));
            Assert.That(HealthOf(run, MeleeSecondId), Is.EqualTo(damagedHealth));
            Assert.That(HealthOf(run, MeleeThirdId), Is.EqualTo(damagedHealth));
        }

        [UnityTest]
        public IEnumerator ClearingWaveOne_AdvancesToWaveTwoAndPresentsItsWaveText()
        {
            ArenaSceneTestRig rig = CreateRig(CreateTwoWaveConfiguration());

            // The camera controller pins the yaw of the view the attack aim is
            // read from; this scenario points the view at the boss instead.
            rig.CameraController.enabled = false;

            rig.BinderObject.SetActive(true);
            yield return null;

            ArenaRunSession session = rig.Scope.Container.Resolve<ArenaRunSession>();

            yield return WaitUntil(() => session.HasActiveRun, 2f);

            IArenaRunRepository repository =
                rig.Scope.Container.Resolve<IArenaRunRepository>();

            Assert.That(
                repository.TryGet(session.ActiveArenaRunId, out ArenaRun run),
                Is.True);

            IEnemyColliderRegistration colliders =
                rig.Scope.Container.Resolve<IEnemyColliderRegistration>();

            Dictionary<EnemyId, GameObject> tracked = new();
            EnemyId bossId = EnemyId.None;
            float deadline = Time.realtimeSinceStartup + 45f;

            while (run.CurrentWaveNumber.Value == 1
                && Time.realtimeSinceStartup < deadline)
            {
                if (bossId.IsNone && TryFindBoss(run, out EnemyId spawnedBoss))
                {
                    bossId = spawnedBoss;
                }

                if (bossId.IsNone == false
                    && tracked.ContainsKey(bossId) == false
                    && run.TryGetEnemyPosition(bossId, out Position3D spawn))
                {
                    tracked[bossId] = RegisterCollider(
                        colliders,
                        bossId,
                        new Vector3(spawn.X, spawn.Y, spawn.Z));
                }

                if (bossId.IsNone == false
                    && tracked.TryGetValue(bossId, out GameObject bossCollider))
                {
                    SyncColliders(run, tracked);

                    // The boss spawns outside the arena, so no production view
                    // exists for it yet. Registering the synced collider on
                    // every frame keeps the aim ray resolving the boss after it
                    // walks in and the production view takes the mapping over.
                    colliders.Register(bossId, bossCollider.GetComponent<Collider>());

                    AimAt(rig, run, bossId);
                }

                if (run.IsSelectedWeaponReady)
                {
                    yield return Strike();
                    continue;
                }

                yield return null;
            }

            Assert.That(
                run.CurrentWaveNumber.Value,
                Is.EqualTo(2),
                "Clearing wave one advanced the run to the second wave.");

            yield return WaitUntil(() => rig.Hud.WaveText == "Wave 2 of 2", 2f);

            Assert.That(run.Status, Is.EqualTo(ArenaRunStatus.Playing));
            Assert.That(
                rig.Hud.WaveText,
                Is.EqualTo("Wave 2 of 2"),
                "The HUD presents the Application-formatted second-wave text.");
        }

        [UnityTest]
        public IEnumerator DefeatingTheRun_ShowsTheDefeatWindowAndReloadsExactlyOnce()
        {
            RecordingArenaSceneReloader reloader = new();
            ArenaSceneTestRig rig = CreateRig(CreateDefeatConfiguration(), reloader);

            rig.BinderObject.SetActive(true);
            yield return null;

            ArenaRunSession session = rig.Scope.Container.Resolve<ArenaRunSession>();

            yield return WaitUntil(() => session.HasActiveRun, 2f);

            IArenaRunRepository repository =
                rig.Scope.Container.Resolve<IArenaRunRepository>();

            Assert.That(
                repository.TryGet(session.ActiveArenaRunId, out ArenaRun run),
                Is.True);

            // No attack input: the seeded wave closes and defeats the player.
            float deadline = Time.realtimeSinceStartup + 30f;

            while (run.Status != ArenaRunStatus.Defeat
                && Time.realtimeSinceStartup < deadline)
            {
                yield return null;
            }

            Assert.That(
                run.Status,
                Is.EqualTo(ArenaRunStatus.Defeat),
                "The regular wave defeated the player through the real tick stages.");

            float defeatedAt = Time.realtimeSinceStartup;
            ArenaRunId defeatedRunId = session.ActiveArenaRunId;

            Assert.That(
                rig.DefeatView.IsShown,
                Is.True,
                "The defeat window is shown over the published defeat state.");
            Assert.That(rig.DefeatView.ShownRunId, Is.EqualTo(defeatedRunId));

            Assert.That(
                reloader.ReloadCount,
                Is.EqualTo(0),
                "The defeated tick itself requests no reload.");

            yield return WaitUntil(() => reloader.ReloadCount > 0, 5f);

            Assert.That(
                reloader.ReloadCount,
                Is.EqualTo(1),
                "Defeat requests exactly one scene reload.");
            Assert.That(
                Time.realtimeSinceStartup - defeatedAt,
                Is.GreaterThanOrEqualTo(0.4f),
                "The reload waits for the configured restart delay.");

            Assert.That(
                session.ActiveArenaRunId,
                Is.Not.EqualTo(defeatedRunId),
                "A fresh run replaces the defeated one on the reload edge.");

            yield return WaitForSeconds(0.5f);

            Assert.That(
                reloader.ReloadCount,
                Is.EqualTo(1),
                "Later frames do not request another reload.");
        }

        [UnityTest]
        public IEnumerator ClearingTheLastWave_ShowsVictoryOnceWithoutReloading()
        {
            RecordingArenaSceneReloader reloader = new();
            ArenaSceneTestRig rig = CreateRig(CreateBossOnlyVictoryConfiguration(), reloader);

            int victoryAnnouncements = 0;

            rig.Announcer.Announced += announcement =>
            {
                if (announcement == ArenaRunAnnouncer.VictoryAnnouncement)
                {
                    victoryAnnouncements++;
                }
            };

            // The attack aim is read from the camera; this scenario points the
            // view at the boss instead of letting the controller follow the player.
            rig.CameraController.enabled = false;

            rig.BinderObject.SetActive(true);
            yield return null;

            ArenaRunSession session = rig.Scope.Container.Resolve<ArenaRunSession>();

            yield return WaitUntil(() => session.HasActiveRun, 2f);

            IArenaRunRepository repository =
                rig.Scope.Container.Resolve<IArenaRunRepository>();

            Assert.That(
                repository.TryGet(session.ActiveArenaRunId, out ArenaRun run),
                Is.True);

            IEnemyColliderRegistration colliders =
                rig.Scope.Container.Resolve<IEnemyColliderRegistration>();

            Dictionary<EnemyId, GameObject> tracked = new();
            EnemyId bossId = EnemyId.None;
            float deadline = Time.realtimeSinceStartup + 45f;

            while (run.Status == ArenaRunStatus.Playing
                && Time.realtimeSinceStartup < deadline)
            {
                if (bossId.IsNone && TryFindBoss(run, out EnemyId spawnedBoss))
                {
                    bossId = spawnedBoss;
                }

                if (bossId.IsNone == false
                    && tracked.ContainsKey(bossId) == false
                    && run.TryGetEnemyPosition(bossId, out Position3D spawn))
                {
                    tracked[bossId] = RegisterCollider(
                        colliders,
                        bossId,
                        new Vector3(spawn.X, spawn.Y, spawn.Z));
                }

                if (bossId.IsNone == false
                    && tracked.TryGetValue(bossId, out GameObject bossCollider))
                {
                    SyncColliders(run, tracked);

                    // The boss spawns outside the arena, so no production view
                    // exists for it yet. Registering the synced collider on every
                    // frame keeps the aim ray resolving the boss after it walks
                    // in and the production view takes the mapping over.
                    colliders.Register(bossId, bossCollider.GetComponent<Collider>());

                    AimAt(rig, run, bossId);
                }

                if (run.IsSelectedWeaponReady)
                {
                    yield return Strike();
                    continue;
                }

                yield return null;
            }

            Assert.That(
                run.Status,
                Is.EqualTo(ArenaRunStatus.Victory),
                "Clearing the last wave won the run.");

            Assert.That(
                rig.VictoryView.IsShown,
                Is.True,
                "The victory window is shown over the published victory.");
            Assert.That(
                rig.VictoryView.ShownRunId,
                Is.EqualTo(session.ActiveArenaRunId));
            Assert.That(
                victoryAnnouncements,
                Is.EqualTo(1),
                "Victory is presented exactly once.");

            Assert.That(
                reloader.ReloadCount,
                Is.EqualTo(0),
                "The victory tick requests no scene reload.");

            ArenaRunId shownRunId = rig.VictoryView.ShownRunId;

            yield return WaitForSeconds(0.5f);

            Assert.That(run.Status, Is.EqualTo(ArenaRunStatus.Victory));
            Assert.That(rig.VictoryView.IsShown, Is.True);
            Assert.That(rig.VictoryView.ShownRunId, Is.EqualTo(shownRunId));
            Assert.That(
                victoryAnnouncements,
                Is.EqualTo(1),
                "Terminal frames do not present victory again.");
            Assert.That(
                reloader.ReloadCount,
                Is.EqualTo(0),
                "Terminal frames request no scene reload.");
        }

        [UnityTest]
        public IEnumerator ThreeWaveStart_ReachesVictory_WithWaveTextInOrder()
        {
            RecordingArenaSceneReloader reloader = new();
            ArenaSceneTestRig rig = CreateRig(CreateThreeWaveVictoryConfiguration(), reloader);

            int victoryAnnouncements = 0;

            rig.Announcer.Announced += announcement =>
            {
                if (announcement == ArenaRunAnnouncer.VictoryAnnouncement)
                {
                    victoryAnnouncements++;
                }
            };

            // The attack aim is read from the camera; this scenario points the
            // view at each boss instead of letting the controller follow the player.
            rig.CameraController.enabled = false;

            rig.BinderObject.SetActive(true);
            yield return null;

            ArenaRunSession session = rig.Scope.Container.Resolve<ArenaRunSession>();

            yield return WaitUntil(() => session.HasActiveRun, 2f);

            IArenaRunRepository repository =
                rig.Scope.Container.Resolve<IArenaRunRepository>();

            Assert.That(
                repository.TryGet(session.ActiveArenaRunId, out ArenaRun run),
                Is.True);

            IEnemyColliderRegistration colliders =
                rig.Scope.Container.Resolve<IEnemyColliderRegistration>();

            List<string> observedWaveText = new();
            Dictionary<EnemyId, GameObject> tracked = new();
            EnemyId bossId = EnemyId.None;
            int lastWave = run.CurrentWaveNumber.Value;

            yield return WaitUntil(() => rig.Hud.WaveText == "Wave 1 of 3", 3f);

            Assert.That(
                rig.Hud.WaveText,
                Is.EqualTo("Wave 1 of 3"),
                "The HUD presents the first wave of the three-wave catalogue.");
            observedWaveText.Add(rig.Hud.WaveText);

            float deadline = Time.realtimeSinceStartup + 90f;

            while (run.Status == ArenaRunStatus.Playing
                && Time.realtimeSinceStartup < deadline)
            {
                int currentWave = run.CurrentWaveNumber.Value;

                if (currentWave != lastWave)
                {
                    // The previous boss is gone; its fixture collider would keep
                    // capturing the aim ray if it stayed at the death position.
                    DestroyTrackedColliders(tracked);
                    bossId = EnemyId.None;
                    lastWave = currentWave;

                    string expectedWaveText = "Wave " + currentWave + " of 3";

                    yield return WaitUntil(() => rig.Hud.WaveText == expectedWaveText, 3f);

                    Assert.That(
                        rig.Hud.WaveText,
                        Is.EqualTo(expectedWaveText),
                        "The HUD presents wave " + currentWave + " in order.");
                    observedWaveText.Add(rig.Hud.WaveText);
                }

                if (bossId.IsNone && TryFindBoss(run, out EnemyId spawnedBoss))
                {
                    bossId = spawnedBoss;
                }

                if (bossId.IsNone == false
                    && tracked.ContainsKey(bossId) == false
                    && run.TryGetEnemyPosition(bossId, out Position3D spawn))
                {
                    tracked[bossId] = RegisterCollider(
                        colliders,
                        bossId,
                        new Vector3(spawn.X, spawn.Y, spawn.Z));
                }

                if (bossId.IsNone == false
                    && tracked.TryGetValue(bossId, out GameObject bossCollider))
                {
                    SyncColliders(run, tracked);

                    // The boss spawns outside the arena, so no production view
                    // exists for it yet. Registering the synced collider on every
                    // frame keeps the aim ray resolving the boss after it walks in.
                    colliders.Register(bossId, bossCollider.GetComponent<Collider>());

                    AimAt(rig, run, bossId);
                }

                if (run.IsSelectedWeaponReady)
                {
                    yield return Strike();
                    continue;
                }

                yield return null;
            }

            Assert.That(
                run.Status,
                Is.EqualTo(ArenaRunStatus.Victory),
                "Clearing the third boss won the run.");

            Assert.That(
                observedWaveText,
                Is.EqualTo(new[] { "Wave 1 of 3", "Wave 2 of 3", "Wave 3 of 3" }),
                "The HUD presented every wave text in order.");

            Assert.That(
                rig.VictoryView.IsShown,
                Is.True,
                "The victory window is shown over the published victory.");
            Assert.That(
                rig.VictoryView.ShownRunId,
                Is.EqualTo(session.ActiveArenaRunId));
            Assert.That(
                victoryAnnouncements,
                Is.EqualTo(1),
                "Victory is presented exactly once.");

            Assert.That(
                reloader.ReloadCount,
                Is.EqualTo(0),
                "The victory tick requests no scene reload.");

            ArenaRunId shownRunId = rig.VictoryView.ShownRunId;

            yield return WaitForSeconds(0.5f);

            Assert.That(run.Status, Is.EqualTo(ArenaRunStatus.Victory));
            Assert.That(rig.VictoryView.IsShown, Is.True);
            Assert.That(rig.VictoryView.ShownRunId, Is.EqualTo(shownRunId));
            Assert.That(
                victoryAnnouncements,
                Is.EqualTo(1),
                "Terminal frames do not present victory again.");
            Assert.That(
                reloader.ReloadCount,
                Is.EqualTo(0),
                "Terminal frames request no scene reload.");
        }

        [UnityTest]
        public IEnumerator EnemyWindup_PlayerEscapes_ResolvesAsMiss()
        {
            RecordingArenaSceneReloader reloader = new();
            ArenaSceneTestRig rig = CreateRig(CreateWindupEnemyConfiguration(), reloader);

            // Movement is view-relative; the controller would otherwise rewrite
            // the view basis every frame.
            rig.CameraController.enabled = false;

            rig.BinderObject.SetActive(true);
            yield return null;

            ArenaRunSession session = rig.Scope.Container.Resolve<ArenaRunSession>();

            yield return WaitUntil(() => session.HasActiveRun, 2f);

            IArenaRunRepository repository =
                rig.Scope.Container.Resolve<IArenaRunRepository>();

            Assert.That(
                repository.TryGet(session.ActiveArenaRunId, out ArenaRun run),
                Is.True);

            float deadline = Time.realtimeSinceStartup + 25f;
            EnemyId attackerId = EnemyId.None;
            Position3D attackerPosition = Position3D.Zero;

            while (TryFindPendingEnemyAttack(run, out attackerId, out attackerPosition) == false
                && Time.realtimeSinceStartup < deadline)
            {
                yield return null;
            }

            Assert.That(
                attackerId.IsNone,
                Is.False,
                "The enemy reached range and began its windup.");

            int healthAtWindup = PlayerHealthOf(run);

            Assert.That(
                healthAtWindup,
                Is.EqualTo(100),
                "The player is undamaged when the windup starts.");

            PlayerSnapshot player = new ArenaRunSnapshotMapper().Map(run.CreateSnapshot()).Player;
            float offsetX = player.Position.X - attackerPosition.X;
            float offsetZ = player.Position.Z - attackerPosition.Z;

            // The camera is not rotated, so the view basis maps Vertical to +Z
            // and Horizontal to +X; step away along the dominant escape axis.
            if (Mathf.Abs(offsetZ) >= Mathf.Abs(offsetX))
            {
                _device.Axes[VerticalAxis] = offsetZ >= 0f ? 1f : -1f;
            }
            else
            {
                _device.Axes[HorizontalAxis] = offsetX >= 0f ? 1f : -1f;
            }

            yield return WaitUntil(() => HasPendingEnemyAttack(run) == false, 6f);

            _device.Axes.Remove(VerticalAxis);
            _device.Axes.Remove(HorizontalAxis);

            Assert.That(
                HasPendingEnemyAttack(run),
                Is.False,
                "The windup resolved after the player left the attack range.");

            Assert.That(
                PlayerHealthOf(run),
                Is.EqualTo(healthAtWindup),
                "An out-of-range impact resolves as a miss and damages nothing.");
            Assert.That(
                run.Status,
                Is.EqualTo(ArenaRunStatus.Playing),
                "Evading the windup leaves the run playing.");

            yield return WaitForSeconds(0.5f);

            Assert.That(
                PlayerHealthOf(run),
                Is.EqualTo(healthAtWindup),
                "The missed impact leaves the player's health unchanged.");
            Assert.That(run.Status, Is.EqualTo(ArenaRunStatus.Playing));
        }

        [UnityTest]
        public IEnumerator EnemyWindup_PlayerStaysInRange_DefeatsTheRun()
        {
            RecordingArenaSceneReloader reloader = new();
            ArenaSceneTestRig rig = CreateRig(CreateWindupEnemyConfiguration(), reloader);

            rig.BinderObject.SetActive(true);
            yield return null;

            ArenaRunSession session = rig.Scope.Container.Resolve<ArenaRunSession>();

            yield return WaitUntil(() => session.HasActiveRun, 2f);

            IArenaRunRepository repository =
                rig.Scope.Container.Resolve<IArenaRunRepository>();

            Assert.That(
                repository.TryGet(session.ActiveArenaRunId, out ArenaRun run),
                Is.True);

            float deadline = Time.realtimeSinceStartup + 25f;
            EnemyId attackerId = EnemyId.None;

            while (TryFindPendingEnemyAttack(run, out attackerId, out _) == false
                && Time.realtimeSinceStartup < deadline)
            {
                yield return null;
            }

            Assert.That(
                attackerId.IsNone,
                Is.False,
                "The enemy reached range and began its lethal windup.");

            int healthAtWindup = PlayerHealthOf(run);

            Assert.That(
                healthAtWindup,
                Is.EqualTo(100),
                "The player is undamaged when the windup starts.");

            // No movement or attack input: the lethal impact lands in range.
            yield return WaitUntil(() => run.Status == ArenaRunStatus.Defeat, 6f);

            Assert.That(
                run.Status,
                Is.EqualTo(ArenaRunStatus.Defeat),
                "The in-range impact defeated the player.");

            ArenaRunId defeatedRunId = session.ActiveArenaRunId;

            Assert.That(
                PlayerHealthOf(run),
                Is.EqualTo(0),
                "The lethal impact depleted the player's health.");
            Assert.That(
                rig.DefeatView.IsShown,
                Is.True,
                "The defeat window is shown over the published defeat state.");
            Assert.That(rig.DefeatView.ShownRunId, Is.EqualTo(defeatedRunId));
            Assert.That(
                reloader.ReloadCount,
                Is.EqualTo(0),
                "The defeated tick itself requests no reload.");
        }

        private ArenaSceneTestRig CreateRig(
            ArenaGameConfiguration configuration = null,
            IArenaSceneReloader reloader = null)
        {
            ArenaSceneTestRig rig = ArenaSceneTestRig.Create(
                configuration: configuration,
                reloader: reloader);

            _rigs.Add(rig);

            _device = new FakeInputDevice();
            rig.InputSource.Device = _device;

            return rig;
        }

        /// <summary>
        /// Keeps the production arena, player, weapons, and enemies, and varies
        /// only the wave catalogue (two boss-only waves), the restart delay, and
        /// the spawn interval.
        /// </summary>
        private static ArenaGameConfiguration CreateTwoWaveConfiguration()
        {
            ArenaGameConfiguration defaults = ArenaGameConfiguration.CreateDefault();

            WaveCatalog waves = new(
                new[]
                {
                    new WaveDefinition(WaveNumber.First, 0),
                    new WaveDefinition(WaveNumber.First.Next(), 0),
                });

            ArenaRunCreationParameters defaultsParameters = defaults.CreationParameters;

            ArenaRunCreationParameters parameters = new(
                defaultsParameters.ArenaDefinition,
                defaultsParameters.PlayerDefinition,
                defaultsParameters.WeaponCatalog,
                defaultsParameters.EnemyCatalog,
                waves);

            return new ArenaGameConfiguration(
                defaults.Arena,
                parameters,
                new GameDuration(1d),
                new GameDuration(0.1d),
                defaults.ArenaSceneName);
        }

        /// <summary>
        /// Shrinks the arena, weakens the player, and strengthens the regular
        /// enemies so the first wave closes and defeats the player quickly. The
        /// short restart delay keeps the single reload edge inside the scenario.
        /// </summary>
        private static ArenaGameConfiguration CreateDefeatConfiguration()
        {
            ArenaGameConfiguration defaults = ArenaGameConfiguration.CreateDefault();
            ArenaRunCreationParameters defaultsParameters = defaults.CreationParameters;

            ArenaDefinition arena = new(
                new ArenaBounds(-4f, 4f, -4f, 4f, 0f),
                Distance.FromValue(5f));

            PlayerDefinition player = new(
                Position3D.Zero,
                Health.Full(10),
                MovementSpeed.FromUnitsPerSecond(5f),
                CollisionRadius.FromValue(0.5f));

            EnemyCatalog enemies = new(
                new EnemyDefinition(
                    EnemyKind.Regular,
                    Health.Full(50),
                    MovementSpeed.FromUnitsPerSecond(20f),
                    CollisionRadius.FromValue(0.5f),
                    DamageAmount.FromPoints(100),
                    Distance.FromValue(2f),
                    new GameDuration(1d),
                    new GameDuration(0.2d)),
                new EnemyDefinition(
                    EnemyKind.Boss,
                    Health.Full(300),
                    MovementSpeed.FromUnitsPerSecond(1.75f),
                    CollisionRadius.FromValue(1f),
                    DamageAmount.FromPoints(25),
                    Distance.FromValue(3f),
                    new GameDuration(1.1d),
                    new GameDuration(0.4d)));

            WaveCatalog waves = new(new[] { new WaveDefinition(WaveNumber.First, 3) });

            ArenaRunCreationParameters parameters = new(
                arena,
                player,
                defaultsParameters.WeaponCatalog,
                enemies,
                waves);

            return new ArenaGameConfiguration(
                arena,
                parameters,
                new GameDuration(0.5d),
                new GameDuration(0.1d),
                defaults.ArenaSceneName);
        }

        /// <summary>
        /// A single boss-only wave, the mechanism scenario 5 uses for its bosses:
        /// clearing it ends the run in victory.
        /// </summary>
        private static ArenaGameConfiguration CreateBossOnlyVictoryConfiguration()
        {
            ArenaGameConfiguration defaults = ArenaGameConfiguration.CreateDefault();
            ArenaRunCreationParameters defaultsParameters = defaults.CreationParameters;

            WaveCatalog waves = new(
                new[] { new WaveDefinition(WaveNumber.First, 0) });

            ArenaRunCreationParameters parameters = new(
                defaultsParameters.ArenaDefinition,
                defaultsParameters.PlayerDefinition,
                defaultsParameters.WeaponCatalog,
                defaultsParameters.EnemyCatalog,
                waves);

            return new ArenaGameConfiguration(
                defaults.Arena,
                parameters,
                defaults.RestartDelay,
                new GameDuration(0.1d),
                defaults.ArenaSceneName);
        }

        /// <summary>
        /// Three boss-only waves, so the T-9 cut-over chain observes every wave
        /// transition: wave one, wave two, then the final victory.
        /// </summary>
        private static ArenaGameConfiguration CreateThreeWaveVictoryConfiguration()
        {
            ArenaGameConfiguration defaults = ArenaGameConfiguration.CreateDefault();
            ArenaRunCreationParameters defaultsParameters = defaults.CreationParameters;

            WaveCatalog waves = new(
                new[]
                {
                    new WaveDefinition(WaveNumber.First, 0),
                    new WaveDefinition(WaveNumber.First.Next(), 0),
                    new WaveDefinition(WaveNumber.First.Next().Next(), 0),
                });

            ArenaRunCreationParameters parameters = new(
                defaultsParameters.ArenaDefinition,
                defaultsParameters.PlayerDefinition,
                defaultsParameters.WeaponCatalog,
                defaultsParameters.EnemyCatalog,
                waves);

            return new ArenaGameConfiguration(
                defaults.Arena,
                parameters,
                defaults.RestartDelay,
                new GameDuration(0.1d),
                defaults.ArenaSceneName);
        }

        /// <summary>
        /// One regular enemy with a long windup, a short range, and lethal damage.
        /// The arena leaves the player enough room to step out of range, and the
        /// slow enemy speed keeps evasion decisive after the player stops.
        /// </summary>
        private static ArenaGameConfiguration CreateWindupEnemyConfiguration()
        {
            ArenaGameConfiguration defaults = ArenaGameConfiguration.CreateDefault();
            ArenaRunCreationParameters defaultsParameters = defaults.CreationParameters;

            ArenaDefinition arena = new(
                new ArenaBounds(-6f, 6f, -6f, 6f, 0f),
                Distance.FromValue(2f));

            PlayerDefinition player = new(
                Position3D.Zero,
                Health.Full(100),
                MovementSpeed.FromUnitsPerSecond(5f),
                CollisionRadius.FromValue(0.5f));

            EnemyDefinition regular = new(
                EnemyKind.Regular,
                Health.Full(50),
                MovementSpeed.FromUnitsPerSecond(2f),
                CollisionRadius.FromValue(0.5f),
                DamageAmount.FromPoints(100),
                Distance.FromValue(1.5f),
                new GameDuration(0.2d),
                new GameDuration(2d));

            EnemyCatalog enemies = new(
                regular,
                defaultsParameters.EnemyCatalog.Get(EnemyKind.Boss));

            WaveCatalog waves = new(new[] { new WaveDefinition(WaveNumber.First, 1) });

            ArenaRunCreationParameters parameters = new(
                arena,
                player,
                defaultsParameters.WeaponCatalog,
                enemies,
                waves);

            return new ArenaGameConfiguration(
                arena,
                parameters,
                new GameDuration(3d),
                new GameDuration(0.1d),
                defaults.ArenaSceneName);
        }

        private static bool TryFindBoss(ArenaRun run, out EnemyId bossId)
        {
            ArenaRunSnapshot snapshot = new ArenaRunSnapshotMapper().Map(run.CreateSnapshot());

            for (int index = 0; index < snapshot.Enemies.Count; index++)
            {
                if (snapshot.Enemies[index].Kind == EnemyKind.Boss)
                {
                    bossId = snapshot.Enemies[index].EnemyId;
                    return true;
                }
            }

            bossId = EnemyId.None;
            return false;
        }

        private static void AimAt(ArenaSceneTestRig rig, ArenaRun run, EnemyId enemyId)
        {
            if (run.TryGetEnemyPosition(enemyId, out Position3D enemy) == false)
            {
                return;
            }

            Vector3 offset = new(
                enemy.X - run.PlayerPosition.X,
                0f,
                enemy.Z - run.PlayerPosition.Z);

            if (offset.sqrMagnitude < 1e-4f)
            {
                return;
            }

            rig.Camera.transform.rotation =
                Quaternion.LookRotation(offset.normalized, Vector3.up);
        }

        private static void SeedEnemy(ArenaRun run, EnemyId enemyId, Position3D position)
        {
            EnemySpawnRequestOutcome request = run.RequestEnemySpawn();

            Assert.That(request.HasRequest, Is.True, "The run has a regular spawn available.");

            EnemySpawnResolutionOutcome resolution = run.ApplyEnemySpawn(
                new EnemySpawnResolution(request.Request.Correlation, enemyId, position));

            Assert.That(
                resolution.IsSpawned,
                Is.True,
                "The seeded enemy was accepted: " + resolution.RejectionReason);
        }

        private GameObject RegisterCollider(
            IEnemyColliderRegistration colliders,
            EnemyId enemyId,
            Vector3 position)
        {
            GameObject gameObject = new("Enemy Collider " + enemyId);
            gameObject.transform.position = position;

            BoxCollider collider = gameObject.AddComponent<BoxCollider>();
            collider.size = new Vector3(1f, 2f, 1f);

            _colliders.Add(gameObject);

            Physics.SyncTransforms();

            colliders.Register(enemyId, collider);

            return gameObject;
        }

        private static bool AreWithinMeleeReach(ArenaRun run)
        {
            const float reach = 2.5f;

            for (int index = 0; index < MeleeEnemyIds.Length; index++)
            {
                if (run.TryGetEnemyPosition(MeleeEnemyIds[index], out Position3D position) == false)
                {
                    return false;
                }

                float offsetX = position.X - run.PlayerPosition.X;
                float offsetZ = position.Z - run.PlayerPosition.Z;

                if ((offsetX * offsetX) + (offsetZ * offsetZ) > reach * reach)
                {
                    return false;
                }
            }

            return true;
        }

        private static void SyncColliders(
            ArenaRun run,
            Dictionary<EnemyId, GameObject> tracked)
        {
            foreach (KeyValuePair<EnemyId, GameObject> entry in tracked)
            {
                if (run.TryGetEnemyPosition(entry.Key, out Position3D position))
                {
                    entry.Value.transform.position = new Vector3(position.X, position.Y, position.Z);
                }
            }

            Physics.SyncTransforms();
        }

        private static int PlayerHealthOf(ArenaRun run)
        {
            ArenaRunSnapshot snapshot = new ArenaRunSnapshotMapper().Map(run.CreateSnapshot());

            return snapshot.Player.Health.Current;
        }

        private static bool HasPendingEnemyAttack(ArenaRun run)
        {
            return TryFindPendingEnemyAttack(run, out _, out _);
        }

        private static bool TryFindPendingEnemyAttack(
            ArenaRun run,
            out EnemyId enemyId,
            out Position3D position)
        {
            ArenaRunSnapshot snapshot = new ArenaRunSnapshotMapper().Map(run.CreateSnapshot());

            for (int index = 0; index < snapshot.Enemies.Count; index++)
            {
                if (snapshot.Enemies[index].HasPendingAttack)
                {
                    enemyId = snapshot.Enemies[index].EnemyId;
                    position = snapshot.Enemies[index].Position;
                    return true;
                }
            }

            enemyId = EnemyId.None;
            position = Position3D.Zero;
            return false;
        }

        private void DestroyTrackedColliders(Dictionary<EnemyId, GameObject> tracked)
        {
            foreach (KeyValuePair<EnemyId, GameObject> entry in tracked)
            {
                if (entry.Value == null)
                {
                    continue;
                }

                _colliders.Remove(entry.Value);
                UnityEngine.Object.DestroyImmediate(entry.Value);
            }

            tracked.Clear();
        }

        private static int HealthOf(ArenaRun run, EnemyId enemyId)
        {
            ArenaRunSnapshot snapshot = new ArenaRunSnapshotMapper().Map(run.CreateSnapshot());

            for (int index = 0; index < snapshot.Enemies.Count; index++)
            {
                if (snapshot.Enemies[index].EnemyId == enemyId)
                {
                    return snapshot.Enemies[index].Health.Current;
                }
            }

            return -1;
        }

        private IEnumerator Strike()
        {
            _device.ButtonsDown.Add(AttackButton);

            yield return null;

            _device.ButtonsDown.Remove(AttackButton);
        }

        private IEnumerator SwitchToMelee(ArenaRun run)
        {
            _device.KeysDown.Add(KeyCode.Q);

            yield return null;

            _device.KeysDown.Remove(KeyCode.Q);

            yield return null;

            Assert.That(
                run.SelectedWeapon,
                Is.EqualTo(WeaponKind.Melee),
                "The weapon switch input selected melee.");
        }

        private static IEnumerator WaitUntil(Func<bool> condition, float timeoutSeconds)
        {
            float deadline = Time.realtimeSinceStartup + timeoutSeconds;

            while (condition() == false && Time.realtimeSinceStartup < deadline)
            {
                yield return null;
            }
        }

        private static IEnumerator WaitForSeconds(float seconds)
        {
            float deadline = Time.realtimeSinceStartup + seconds;

            while (Time.realtimeSinceStartup < deadline)
            {
                yield return null;
            }
        }

        private sealed class RecordingArenaSceneReloader : IArenaSceneReloader
        {
            public int ReloadCount { get; private set; }

            public void ReloadArenaScene()
            {
                ReloadCount++;
            }
        }

        private sealed class FakeInputDevice : IUnityInputDevice
        {
            public Dictionary<string, float> Axes { get; } = new();

            public HashSet<string> ButtonsDown { get; } = new();

            public HashSet<KeyCode> KeysDown { get; } = new();

            public float GetAxisRaw(string axisName)
            {
                return Axes.TryGetValue(axisName, out float value) ? value : 0f;
            }

            public bool GetButtonDown(string buttonName)
            {
                return ButtonsDown.Contains(buttonName);
            }

            public bool GetKeyDown(KeyCode key)
            {
                return KeysDown.Contains(key);
            }
        }
    }
}
