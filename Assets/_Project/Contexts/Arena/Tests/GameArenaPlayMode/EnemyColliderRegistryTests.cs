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
using Game.Arena.Presentation.Enemies;
using NUnit.Framework;
using UnityEngine;

namespace Game.Arena.PlayMode.Tests
{
    [TestFixture]
    public sealed class EnemyColliderRegistryTests
    {
        private readonly List<GameObject> _objects = new();
        private EnemyColliderRegistry _colliders;

        [SetUp]
        public void SetUp()
        {
            _objects.Clear();
            _colliders = new EnemyColliderRegistry();
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
        public void RegisteredCollider_ResolvesToItsEnemy()
        {
            EnemyId enemyId = EnemyId.FromValue(1UL);
            Collider collider = CreateCollider();

            _colliders.Register(enemyId, collider);

            Assert.That(_colliders.TryGetEnemyId(collider, out EnemyId resolved), Is.True);
            Assert.That(resolved, Is.EqualTo(enemyId));
        }

        [Test]
        public void UnknownCollider_ResolvesToNone()
        {
            Collider collider = CreateCollider();

            Assert.That(_colliders.TryGetEnemyId(collider, out EnemyId resolved), Is.False);
            Assert.That(resolved, Is.EqualTo(EnemyId.None));
        }

        [Test]
        public void UnregisteredEnemy_StopsResolving()
        {
            EnemyId enemyId = EnemyId.FromValue(1UL);
            Collider collider = CreateCollider();
            _colliders.Register(enemyId, collider);

            _colliders.Unregister(enemyId);

            Assert.That(_colliders.TryGetEnemyId(collider, out EnemyId resolved), Is.False);
            Assert.That(resolved, Is.EqualTo(EnemyId.None));
        }

        [Test]
        public void UnregisteringAnUnknownEnemy_IsANoOp()
        {
            EnemyId enemyId = EnemyId.FromValue(1UL);
            Collider collider = CreateCollider();
            _colliders.Register(enemyId, collider);

            _colliders.Unregister(EnemyId.FromValue(2UL));

            Assert.That(_colliders.TryGetEnemyId(collider, out EnemyId resolved), Is.True);
            Assert.That(resolved, Is.EqualTo(enemyId));
        }

        [Test]
        public void ReRegisteringAnEnemy_ReplacesItsCollider()
        {
            EnemyId enemyId = EnemyId.FromValue(1UL);
            Collider previous = CreateCollider();
            Collider replacement = CreateCollider();

            _colliders.Register(enemyId, previous);
            _colliders.Register(enemyId, replacement);

            Assert.That(_colliders.TryGetEnemyId(replacement, out EnemyId resolved), Is.True);
            Assert.That(resolved, Is.EqualTo(enemyId));
            Assert.That(_colliders.TryGetEnemyId(previous, out EnemyId stale), Is.False);
            Assert.That(stale, Is.EqualTo(EnemyId.None));
        }

        [Test]
        public void ColliderOwnedByAnotherEnemy_MovesToTheNewOwner()
        {
            EnemyId first = EnemyId.FromValue(1UL);
            EnemyId second = EnemyId.FromValue(2UL);
            Collider collider = CreateCollider();

            _colliders.Register(first, collider);
            _colliders.Register(second, collider);

            Assert.That(_colliders.TryGetEnemyId(collider, out EnemyId resolved), Is.True);
            Assert.That(resolved, Is.EqualTo(second));

            _colliders.Unregister(first);

            Assert.That(_colliders.TryGetEnemyId(collider, out EnemyId stillRegistered), Is.True);
            Assert.That(stillRegistered, Is.EqualTo(second));
        }

        [Test]
        public void Register_NullCollider_Throws()
        {
            Assert.Throws<ArgumentNullException>(
                () => _colliders.Register(EnemyId.FromValue(1UL), null));
        }

        [Test]
        public void DestroyedCollider_ResolvesToNone()
        {
            EnemyId enemyId = EnemyId.FromValue(1UL);
            GameObject owner = Track(new GameObject("EnemyCollider"));
            BoxCollider collider = owner.AddComponent<BoxCollider>();
            _colliders.Register(enemyId, collider);

            UnityEngine.Object.DestroyImmediate(owner);

            Assert.That(_colliders.TryGetEnemyId(collider, out EnemyId resolved), Is.False);
            Assert.That(resolved, Is.EqualTo(EnemyId.None));
        }

        [Test]
        public void LiveSeam_RegisterAndUnregisterFollowTheViewLifetime()
        {
            EnemyId enemyId = EnemyId.FromValue(1UL);
            EnemyViewRegistry views = CreateViewRegistry();

            views.Sync(CreateSnapshot(Enemy(enemyId, 0f, 0f)));

            Assert.That(views.TryGetView(enemyId, out EnemyView view), Is.True);
            Collider collider = view.TargetCollider;
            Assert.That(collider, Is.Not.Null);
            Assert.That(_colliders.TryGetEnemyId(collider, out EnemyId resolved), Is.True);
            Assert.That(resolved, Is.EqualTo(enemyId));

            views.Sync(CreateSnapshot());

            Assert.That(views.IsRendered(enemyId), Is.False);
            Assert.That(_colliders.TryGetEnemyId(collider, out EnemyId removed), Is.False);
            Assert.That(removed, Is.EqualTo(EnemyId.None));
        }

        private EnemyViewRegistry CreateViewRegistry()
        {
            GameObject templateObject = Track(new GameObject("EnemyTemplate"));
            templateObject.AddComponent<EnemyView>();
            templateObject.AddComponent<BoxCollider>();

            GameObject root = Track(new GameObject("Enemies"));
            EnemyViewRegistry views = root.AddComponent<EnemyViewRegistry>();
            views.Template = templateObject.GetComponent<EnemyView>();
            views.ViewCreated += _colliders.Register;
            views.ViewDestroyed += _colliders.Unregister;

            return views;
        }

        private Collider CreateCollider()
        {
            GameObject owner = Track(new GameObject("EnemyCollider"));
            return owner.AddComponent<BoxCollider>();
        }

        private GameObject Track(GameObject gameObject)
        {
            _objects.Add(gameObject);
            return gameObject;
        }

        private static ArenaRunSnapshot CreateSnapshot(params EnemySnapshot[] enemies)
        {
            PlayerSnapshot player = new(
                PlayerId.FromValue(1UL),
                Position3D.Zero,
                Health.Full(100),
                WeaponKind.Melee,
                new GameTimePoint(0d),
                new GameTimePoint(0d),
                false,
                default);

            return new ArenaRunSnapshot(
                ArenaRunId.FromValue(1UL),
                AggregateRevision.Initial,
                ArenaRunStatus.Playing,
                new GameTimePoint(0d),
                new ArenaBounds(-20f, 20f, -20f, 20f, 0f),
                player,
                enemies,
                new WaveSnapshot(WaveNumber.First, WavePhase.RegularCombat, 0, BossStatus.NotSpawned),
                WaveNumber.FromValue(3));
        }

        private static EnemySnapshot Enemy(EnemyId enemyId, float x, float z)
        {
            return new EnemySnapshot(
                enemyId,
                EnemyKind.Regular,
                new Position3D(x, 0f, z),
                Health.Full(100),
                CollisionRadius.FromValue(0.5f),
                new GameTimePoint(0d),
                false,
                default);
        }
    }
}
