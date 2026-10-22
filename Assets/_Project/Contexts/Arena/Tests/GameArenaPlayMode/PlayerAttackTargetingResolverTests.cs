using System.Collections.Generic;
using Game.Arena.Domain.Combat;
using Game.Arena.Domain.Concurrency;
using Game.Arena.Domain.Geometry;
using Game.Arena.Domain.Identity;
using Game.Arena.Domain.Interactions;
using Game.Arena.Domain.Interactions.Attack;
using Game.Arena.Infrastructure.Targeting;
using NUnit.Framework;
using UnityEngine;

namespace Game.Arena.PlayMode.Tests
{
    [TestFixture]
    public sealed class PlayerAttackTargetingResolverTests
    {
        private readonly List<GameObject> _objects = new();
        private TestEnemyColliderRegistry _registry;
        private UnityPlayerAttackTargetingResolver _resolver;

        [SetUp]
        public void SetUp()
        {
            _objects.Clear();
            _registry = new TestEnemyColliderRegistry();
            _resolver = new UnityPlayerAttackTargetingResolver(_registry);
        }

        [TearDown]
        public void TearDown()
        {
            for (int index = 0; index < _objects.Count; index++)
            {
                Object.DestroyImmediate(_objects[index]);
            }

            _objects.Clear();
        }

        [Test]
        public void Resolve_EnemyOnRayWithinRange_ReturnsThatEnemy()
        {
            EnemyId enemyId = EnemyId.FromValue(1UL);
            _registry.Register(CreateCollider("Enemy", new Vector3(0f, 0f, 5f)), enemyId);

            IReadOnlyList<EnemyId> result = _resolver.Resolve(CreateRequest(Direction3D.Forward, 10f));

            Assert.That(result, Is.EqualTo(new[] { enemyId }));
        }

        [Test]
        public void Resolve_TwoEnemiesOnRay_ReturnsNearestEnemyOnly()
        {
            EnemyId nearEnemy = EnemyId.FromValue(1UL);
            EnemyId farEnemy = EnemyId.FromValue(2UL);

            _registry.Register(CreateCollider("FarEnemy", new Vector3(0f, 0f, 7f)), farEnemy);
            _registry.Register(CreateCollider("NearEnemy", new Vector3(0f, 0f, 3f)), nearEnemy);

            IReadOnlyList<EnemyId> result = _resolver.Resolve(CreateRequest(Direction3D.Forward, 10f));

            Assert.That(result, Is.EqualTo(new[] { nearEnemy }));
        }

        [Test]
        public void Resolve_EnemyBeyondRange_ReturnsNoTargets()
        {
            _registry.Register(CreateCollider("Enemy", new Vector3(0f, 0f, 5f)), EnemyId.FromValue(1UL));

            IReadOnlyList<EnemyId> result = _resolver.Resolve(CreateRequest(Direction3D.Forward, 3f));

            Assert.That(result, Is.Empty);
        }

        [Test]
        public void Resolve_EnemyBehindOrigin_ReturnsNoTargets()
        {
            _registry.Register(CreateCollider("Enemy", new Vector3(0f, 0f, -5f)), EnemyId.FromValue(1UL));

            IReadOnlyList<EnemyId> result = _resolver.Resolve(CreateRequest(Direction3D.Forward, 10f));

            Assert.That(result, Is.Empty);
        }

        [Test]
        public void Resolve_WhenNoEnemyIsOnRay_ReturnsEmptyListNotNull()
        {
            IReadOnlyList<EnemyId> result = _resolver.Resolve(CreateRequest(Direction3D.Forward, 10f));

            Assert.That(result, Is.Not.Null);
            Assert.That(result, Is.Empty);
        }

        [Test]
        public void Resolve_PlayerOwnCollider_IsIgnored()
        {
            EnemyId enemyId = EnemyId.FromValue(1UL);
            CreateCollider("Player", Vector3.zero);
            _registry.Register(CreateCollider("Enemy", new Vector3(0f, 0f, 5f)), enemyId);

            IReadOnlyList<EnemyId> result = _resolver.Resolve(CreateRequest(Direction3D.Forward, 10f));

            Assert.That(result, Is.EqualTo(new[] { enemyId }));
        }

        [Test]
        public void Resolve_NonEnemyColliderOnRay_DoesNotBlockTheShot()
        {
            EnemyId enemyId = EnemyId.FromValue(1UL);
            CreateCollider("Wall", new Vector3(0f, 0f, 2f));
            _registry.Register(CreateCollider("Enemy", new Vector3(0f, 0f, 6f)), enemyId);

            IReadOnlyList<EnemyId> result = _resolver.Resolve(CreateRequest(Direction3D.Forward, 10f));

            Assert.That(result, Is.EqualTo(new[] { enemyId }));
        }

        [Test]
        public void Resolve_AfterEnemyMovesOutOfRange_UsesCurrentPosition()
        {
            EnemyId enemyId = EnemyId.FromValue(1UL);
            GameObject enemy = CreateCollider("Enemy", new Vector3(0f, 0f, 5f));
            _registry.Register(enemy, enemyId);

            Assert.That(_resolver.Resolve(CreateRequest(Direction3D.Forward, 10f)), Is.EqualTo(new[] { enemyId }));

            enemy.transform.position = new Vector3(0f, 0f, 50f);
            Physics.SyncTransforms();

            Assert.That(_resolver.Resolve(CreateRequest(Direction3D.Forward, 10f)), Is.Empty);
        }

        [Test]
        public void ResolveMelee_EnemyAheadInsideArc_ReturnsThatEnemy()
        {
            EnemyId enemyId = EnemyId.FromValue(1UL);
            _registry.Register(CreateCollider("Enemy", new Vector3(0f, 0f, 5f)), enemyId);

            IReadOnlyList<EnemyId> result = _resolver.Resolve(
                CreateMeleeRequest(Direction3D.Forward, 10f, AttackArc.FromDegrees(90f)));

            Assert.That(result, Is.EqualTo(new[] { enemyId }));
        }

        [Test]
        public void ResolveMelee_EnemyBehindOutsideArc_ReturnsNoTargets()
        {
            _registry.Register(CreateCollider("Enemy", new Vector3(0f, 0f, -5f)), EnemyId.FromValue(1UL));

            IReadOnlyList<EnemyId> result = _resolver.Resolve(
                CreateMeleeRequest(Direction3D.Forward, 10f, AttackArc.FromDegrees(90f)));

            Assert.That(result, Is.Empty);
        }

        [Test]
        public void ResolveMelee_EnemyBesideOutsideArc_ReturnsNoTargets()
        {
            _registry.Register(CreateCollider("Enemy", new Vector3(5f, 0f, 0f)), EnemyId.FromValue(1UL));

            IReadOnlyList<EnemyId> result = _resolver.Resolve(
                CreateMeleeRequest(Direction3D.Forward, 10f, AttackArc.FromDegrees(90f)));

            Assert.That(result, Is.Empty);
        }

        [Test]
        public void ResolveMelee_FullArc_ReturnsEnemyBehind()
        {
            EnemyId enemyId = EnemyId.FromValue(1UL);
            _registry.Register(CreateCollider("Enemy", new Vector3(0f, 0f, -5f)), enemyId);

            IReadOnlyList<EnemyId> result = _resolver.Resolve(
                CreateMeleeRequest(Direction3D.Forward, 10f, AttackArc.Full));

            Assert.That(result, Is.EqualTo(new[] { enemyId }));
        }

        [Test]
        public void ResolveMelee_EveryEnemyInsideArc_ReturnsAllOfThem()
        {
            EnemyId first = EnemyId.FromValue(1UL);
            EnemyId second = EnemyId.FromValue(2UL);
            EnemyId third = EnemyId.FromValue(3UL);

            _registry.Register(CreateCollider("First", new Vector3(0f, 0f, 5f)), first);
            _registry.Register(CreateCollider("Second", new Vector3(1f, 0f, -5f)), second);
            _registry.Register(CreateCollider("Third", new Vector3(-2f, 0f, 4f)), third);

            IReadOnlyList<EnemyId> result = _resolver.Resolve(
                CreateMeleeRequest(Direction3D.Forward, 10f, AttackArc.Full));

            Assert.That(result, Is.EquivalentTo(new[] { first, second, third }));
        }

        [Test]
        public void ResolveMelee_EnemyWithTwoColliders_ReturnsEnemyOnce()
        {
            EnemyId enemyId = EnemyId.FromValue(1UL);
            _registry.Register(CreateCollider("Body", new Vector3(0f, 0f, 5f)), enemyId);
            _registry.Register(CreateCollider("Head", new Vector3(0f, 1.5f, 5f)), enemyId);

            IReadOnlyList<EnemyId> result = _resolver.Resolve(
                CreateMeleeRequest(Direction3D.Forward, 10f, AttackArc.FromDegrees(90f)));

            Assert.That(result, Is.EqualTo(new[] { enemyId }));
        }

        [Test]
        public void ResolveMelee_EnemyStackedOnOrigin_IsInsideAnyArc()
        {
            EnemyId enemyId = EnemyId.FromValue(1UL);
            _registry.Register(CreateCollider("Enemy", Vector3.zero), enemyId);

            IReadOnlyList<EnemyId> result = _resolver.Resolve(
                CreateMeleeRequest(Direction3D.Forward, 10f, AttackArc.FromDegrees(90f)));

            Assert.That(result, Is.EqualTo(new[] { enemyId }));
        }

        [Test]
        public void ResolveMelee_NoEnemies_ReturnsEmptyListNotNull()
        {
            IReadOnlyList<EnemyId> result = _resolver.Resolve(
                CreateMeleeRequest(Direction3D.Forward, 10f, AttackArc.FromDegrees(90f)));

            Assert.That(result, Is.Not.Null);
            Assert.That(result, Is.Empty);
        }

        private static PlayerAttackImpactRequest CreateRequest(Direction3D aimDirection, float range)
        {
            return CreateRequest(WeaponKind.Ranged, aimDirection, range, AttackArc.Full);
        }

        private static PlayerAttackImpactRequest CreateMeleeRequest(
            Direction3D aimDirection,
            float range,
            AttackArc arc)
        {
            return CreateRequest(WeaponKind.Melee, aimDirection, range, arc);
        }

        private static PlayerAttackImpactRequest CreateRequest(
            WeaponKind weaponKind,
            Direction3D aimDirection,
            float range,
            AttackArc arc)
        {
            InteractionCorrelation correlation = new(
                ArenaRunId.FromValue(1UL),
                InteractionId.None.Next(),
                AggregateRevision.Initial);

            return new PlayerAttackImpactRequest(
                correlation,
                AttackId.None.Next(),
                weaponKind,
                Position3D.Zero,
                aimDirection,
                Distance.FromValue(range),
                arc);
        }

        private GameObject CreateCollider(string name, Vector3 position)
        {
            GameObject gameObject = new(name);
            gameObject.transform.position = position;

            BoxCollider collider = gameObject.AddComponent<BoxCollider>();
            collider.size = new Vector3(1f, 2f, 1f);

            _objects.Add(gameObject);
            Physics.SyncTransforms();

            return gameObject;
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
