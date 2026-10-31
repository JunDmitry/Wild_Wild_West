using System.Collections.Generic;
using Game.Arena.Domain.Geometry;
using Game.Arena.Presentation.Enemies;
using NUnit.Framework;
using UnityEngine;

namespace Game.Arena.PlayMode.Tests
{
    [TestFixture]
    public sealed class EnemyViewColliderScalingTests
    {
        private readonly List<GameObject> _objects = new();

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
        public void BossView_SizesTheTargetColliderFromTheDomainRadius()
        {
            GameObject prefab = Resources.Load<GameObject>("Prefabs/EnemyView");
            Assert.That(prefab, Is.Not.Null, "EnemyView prefab must live under Resources/Prefabs.");

            GameObject instance = UnityEngine.Object.Instantiate(prefab);
            _objects.Add(instance);

            EnemyView view = instance.GetComponent<EnemyView>();
            Assert.That(view, Is.Not.Null);

            Position3D position = new(5f, 0f, 3f);
            view.Show(position, CollisionRadius.FromValue(1f));
            Physics.SyncTransforms();

            Collider collider = view.TargetCollider;
            Assert.That(collider, Is.Not.Null);
            Assert.That(collider.bounds.extents.x, Is.EqualTo(1f).Within(1e-3f));
            Assert.That(collider.bounds.center.y, Is.EqualTo(position.Y + 1f).Within(1e-3f));
        }
    }
}
