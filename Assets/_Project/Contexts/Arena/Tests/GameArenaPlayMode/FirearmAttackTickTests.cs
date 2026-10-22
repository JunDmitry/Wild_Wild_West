using System.Collections.Generic;
using Game.Arena.Domain.Geometry;
using Game.Arena.Domain.Identity;
using Game.Arena.Infrastructure.Targeting;
using NUnit.Framework;
using UnityEngine;

namespace Game.Arena.PlayMode.Tests
{
    [TestFixture]
    public sealed class FirearmAttackTickTests
    {
        private readonly List<GameObject> _objects = new();
        private TestEnemyColliderRegistry _registry;
        private PlayModeTickHarness _harness;

        [SetUp]
        public void SetUp()
        {
            _objects.Clear();
            _registry = new TestEnemyColliderRegistry();
            _harness = new PlayModeTickHarness(_registry);
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
        public void FirearmAttack_EnemyInRange_ReducesHealthByWeaponDamage()
        {
            EnemyId enemyId = _harness.SeedEnemy(1UL, new Position3D(0f, 0f, 21f));
            _registry.Register(CreateCollider("Enemy", new Vector3(0f, 0f, 21f)), enemyId);

            Assert.That(_harness.ReadEnemyHealth(enemyId), Is.EqualTo(PlayModeTickHarness.EnemyHealth));

            _harness.Tick(true, Direction3D.Forward);
            _harness.TickIdle();
            _harness.TickIdle();

            Assert.That(
                _harness.ReadEnemyHealth(enemyId),
                Is.EqualTo(PlayModeTickHarness.EnemyHealth - PlayModeTickHarness.RangedDamage));
        }

        [Test]
        public void FirearmAttack_IntoEmptySpace_ChangesNoHealth()
        {
            EnemyId enemyId = _harness.SeedEnemy(1UL, new Position3D(21f, 0f, 0f));
            _registry.Register(CreateCollider("Enemy", new Vector3(21f, 0f, 0f)), enemyId);

            _harness.Tick(true, Direction3D.Forward);
            _harness.TickIdle();
            _harness.TickIdle();

            Assert.That(_harness.ReadEnemyHealth(enemyId), Is.EqualTo(PlayModeTickHarness.EnemyHealth));
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
