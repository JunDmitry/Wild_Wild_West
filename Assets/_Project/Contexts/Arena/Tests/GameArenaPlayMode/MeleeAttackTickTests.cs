using System.Collections.Generic;
using Game.Arena.Domain.Combat;
using Game.Arena.Domain.Geometry;
using Game.Arena.Domain.Identity;
using Game.Arena.Infrastructure.Targeting;
using NUnit.Framework;
using UnityEngine;

namespace Game.Arena.PlayMode.Tests
{
    [TestFixture]
    public sealed class MeleeAttackTickTests
    {
        private const int MeleeDamage = 20;
        private const float MeleeRange = 25f;
        private const int TicksAfterAttackStart = 4;

        private readonly List<GameObject> _objects = new();
        private TestEnemyColliderRegistry _registry;
        private PlayModeTickHarness _harness;

        [SetUp]
        public void SetUp()
        {
            _objects.Clear();
            _registry = new TestEnemyColliderRegistry();
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
        public void MeleeAttack_EnemyAheadInsideArc_LosesWeaponDamage()
        {
            CreateHarness(AttackArc.FromDegrees(90f));
            EnemyId enemyId = SeedEnemy(1UL, new Vector3(0f, 0f, 21f));

            PerformMeleeAttack();

            Assert.That(
                _harness.ReadEnemyHealth(enemyId),
                Is.EqualTo(PlayModeTickHarness.EnemyHealth - MeleeDamage));
        }

        [Test]
        public void MeleeAttack_EnemyBehindOutsideArc_ChangesNoHealth()
        {
            CreateHarness(AttackArc.FromDegrees(90f));
            EnemyId enemyId = SeedEnemy(1UL, new Vector3(0f, 0f, -21f));

            PerformMeleeAttack();

            Assert.That(
                _harness.ReadEnemyHealth(enemyId),
                Is.EqualTo(PlayModeTickHarness.EnemyHealth));
        }

        [Test]
        public void MeleeAttack_EnemyBesideOutsideArc_ChangesNoHealth()
        {
            CreateHarness(AttackArc.FromDegrees(90f));
            EnemyId enemyId = SeedEnemy(1UL, new Vector3(21f, 0f, 0f));

            PerformMeleeAttack();

            Assert.That(
                _harness.ReadEnemyHealth(enemyId),
                Is.EqualTo(PlayModeTickHarness.EnemyHealth));
        }

        [Test]
        public void MeleeAttack_FullArcEnemyBehind_LosesWeaponDamage()
        {
            CreateHarness(null);
            EnemyId enemyId = SeedEnemy(1UL, new Vector3(0f, 0f, -21f));

            PerformMeleeAttack();

            Assert.That(
                _harness.ReadEnemyHealth(enemyId),
                Is.EqualTo(PlayModeTickHarness.EnemyHealth - MeleeDamage));
        }

        [Test]
        public void MeleeAttack_FullArcThreeEnemies_EachLosesWeaponDamageOnce()
        {
            CreateHarness(null);
            EnemyId first = SeedEnemy(1UL, new Vector3(0f, 0f, 21f));
            EnemyId second = SeedEnemy(2UL, new Vector3(5f, 0f, 21f));
            EnemyId third = SeedEnemy(3UL, new Vector3(-5f, 0f, 21f));

            PerformMeleeAttack();

            int damagedHealth = PlayModeTickHarness.EnemyHealth - MeleeDamage;

            Assert.That(_harness.ReadEnemyHealth(first), Is.EqualTo(damagedHealth));
            Assert.That(_harness.ReadEnemyHealth(second), Is.EqualTo(damagedHealth));
            Assert.That(_harness.ReadEnemyHealth(third), Is.EqualTo(damagedHealth));
        }

        private void CreateHarness(AttackArc? meleeArc)
        {
            _harness = new PlayModeTickHarness(_registry, MeleeRange, meleeArc);
        }

        private void PerformMeleeAttack()
        {
            _harness.Tick(false, Direction3D.Forward, true);
            _harness.Tick(true, Direction3D.Forward);

            for (int index = 0; index < TicksAfterAttackStart; index++)
            {
                _harness.TickIdle();
            }
        }

        private EnemyId SeedEnemy(ulong value, Vector3 position)
        {
            EnemyId enemyId = EnemyId.FromValue(value);
            _registry.Register(CreateCollider("Enemy" + value, position), enemyId);

            _harness.SeedEnemy(value, new Position3D(position.x, position.y, position.z));

            return enemyId;
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
