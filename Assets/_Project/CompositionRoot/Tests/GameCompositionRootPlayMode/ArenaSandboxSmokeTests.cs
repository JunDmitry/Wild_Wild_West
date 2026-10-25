using System;
using System.Collections;
using System.Collections.Generic;
using Game.Arena.Application.ReadModels;
using Game.Arena.Application.Sessions;
using Game.Arena.Domain.Aggregates.ArenaRun;
using Game.Arena.Domain.Combat;
using Game.Arena.Domain.Geometry;
using Game.Arena.Domain.Identity;
using Game.Arena.Domain.Interactions.Spawn;
using Game.Arena.Domain.Repositories;
using Game.Arena.Infrastructure.Input;
using Game.Arena.Infrastructure.Targeting;
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
    /// victory) need production seams the request forbids adding: a configurable
    /// multi-wave <see cref="ArenaGameConfiguration"/> and a substitutable
    /// <c>IArenaSceneReloader</c>. They are recorded in
    /// <c>docs/architect/coder-inbox/t8-6-arena-smoke.md</c> instead.
    /// </summary>
    [TestFixture]
    public sealed class ArenaSandboxSmokeTests
    {
        private const string AttackButton = "Fire1";
        private const string VerticalAxis = "Vertical";

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

        private ArenaSceneTestRig CreateRig()
        {
            ArenaSceneTestRig rig = ArenaSceneTestRig.Create();

            _rigs.Add(rig);

            _device = new FakeInputDevice();
            rig.InputSource.Device = _device;

            return rig;
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
