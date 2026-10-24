using System.Collections.Generic;
using Game.Arena.Domain.Concurrency;
using Game.Arena.Domain.Configuration;
using Game.Arena.Domain.Geometry;
using Game.Arena.Domain.Identity;
using Game.Arena.Domain.Interactions;
using Game.Arena.Domain.Interactions.Movement;
using Game.Arena.Domain.Movement;
using Game.Arena.Infrastructure.Movement;
using NUnit.Framework;
using UnityEngine;

namespace Game.Arena.PlayMode.Tests
{
    [TestFixture]
    public sealed class EnemyMovementResolverTests
    {
        private const float ArenaHalfExtent = 20f;
        private const float GroundY = 0f;

        private readonly List<GameObject> _created = new();
        private ArenaDefinition _arena;

        [SetUp]
        public void SetUp()
        {
            _arena = new ArenaDefinition(
                new ArenaBounds(
                    -ArenaHalfExtent,
                    ArenaHalfExtent,
                    -ArenaHalfExtent,
                    ArenaHalfExtent,
                    GroundY),
                Distance.FromValue(5f));

            _created.Clear();
        }

        [TearDown]
        public void TearDown()
        {
            for (int index = 0; index < _created.Count; index++)
            {
                UnityEngine.Object.DestroyImmediate(_created[index]);
            }

            _created.Clear();
        }

        [Test]
        public void ReturnsOneEntryPerIntentAndNeverNull()
        {
            UnityEnemyMovementResolver resolver = new(new TestEnemyTransformRegistry(), _arena);

            EnemyMovementBatchRequest request = CreateRequest(
                CreateIntent(1UL, new Position3D(10f, GroundY, 0f), Direction3D.Left, 0.5f),
                CreateIntent(2UL, new Position3D(0f, GroundY, 10f), Direction3D.Back, 0.5f));

            IReadOnlyList<EnemyMovementBatchResolutionEntry> entries = resolver.Resolve(request);

            Assert.That(entries, Is.Not.Null);
            Assert.That(entries.Count, Is.EqualTo(2));
            Assert.That(entries[0].EnemyId, Is.EqualTo(EnemyId.FromValue(1UL)));
            Assert.That(entries[1].EnemyId, Is.EqualTo(EnemyId.FromValue(2UL)));
        }

        [Test]
        public void UnblockedIntentTravelsTheRequestedDistance()
        {
            UnityEnemyMovementResolver resolver = new(new TestEnemyTransformRegistry(), _arena);

            EnemyMovementBatchResolutionEntry entry = ResolveSingle(
                resolver,
                CreateIntent(1UL, new Position3D(10f, GroundY, 0f), Direction3D.Left, 0.5f));

            Assert.That(entry.AcceptedPosition.X, Is.EqualTo(9.5f).Within(0.0001f));
            Assert.That(entry.AcceptedPosition.Y, Is.EqualTo(GroundY));
            Assert.That(entry.AcceptedPosition.Z, Is.EqualTo(0f).Within(0.0001f));
        }

        [Test]
        public void TransformPositionDoesNotSupplyTheMovementOrigin()
        {
            TestEnemyTransformRegistry registry = new();
            registry.Register(EnemyId.FromValue(1UL), CreateBody("Enemy", new Vector3(0f, 0f, 0f)));
            UnityEnemyMovementResolver resolver = new(registry, _arena);

            EnemyMovementBatchResolutionEntry entry = ResolveSingle(
                resolver,
                CreateIntent(1UL, new Position3D(10f, GroundY, 0f), Direction3D.Left, 0.5f));

            Assert.That(entry.AcceptedPosition.X, Is.EqualTo(9.5f).Within(0.0001f));
        }

        [Test]
        public void OwnColliderDoesNotBlockTheStep()
        {
            TestEnemyTransformRegistry registry = new();
            registry.Register(EnemyId.FromValue(1UL), CreateBody("Enemy", new Vector3(8.9f, 0f, 0f)));
            UnityEnemyMovementResolver resolver = new(registry, _arena);

            EnemyMovementBatchResolutionEntry entry = ResolveSingle(
                resolver,
                CreateIntent(1UL, new Position3D(10f, GroundY, 0f), Direction3D.Left, 0.5f));

            Assert.That(entry.AcceptedPosition.X, Is.EqualTo(9.5f).Within(0.0001f));
        }

        [Test]
        public void WallShortensTheStepAndKeepsItOnThePath()
        {
            TestEnemyTransformRegistry registry = new();
            registry.Register(EnemyId.FromValue(1UL), CreateBody("Enemy", new Vector3(10f, 0f, 0f)));
            CreateWall(new Vector3(9.2f, 0f, 0f));
            UnityEnemyMovementResolver resolver = new(registry, _arena);

            EnemyMovementBatchResolutionEntry entry = ResolveSingle(
                resolver,
                CreateIntent(1UL, new Position3D(10f, GroundY, 0f), Direction3D.Left, 0.5f));

            Assert.That(entry.AcceptedPosition.X, Is.GreaterThan(9.5f));
            Assert.That(entry.AcceptedPosition.X, Is.LessThan(10f));
            Assert.That(entry.AcceptedPosition.Y, Is.EqualTo(GroundY));
            Assert.That(entry.AcceptedPosition.Z, Is.EqualTo(0f).Within(0.0001f));
        }

        [Test]
        public void ArenaEdgeClampsTheStepToTheCollisionRadius()
        {
            UnityEnemyMovementResolver resolver = new(new TestEnemyTransformRegistry(), _arena);

            EnemyMovementBatchResolutionEntry entry = ResolveSingle(
                resolver,
                CreateIntent(1UL, new Position3D(19.4f, GroundY, 0f), Direction3D.Right, 5f));

            Assert.That(entry.AcceptedPosition.X, Is.EqualTo(ArenaHalfExtent - 0.5f).Within(0.001f));
            Assert.That(entry.AcceptedPosition.Z, Is.EqualTo(0f).Within(0.0001f));
        }

        [Test]
        public void EnemyOutsideTheBoundsMayMoveInward()
        {
            UnityEnemyMovementResolver resolver = new(new TestEnemyTransformRegistry(), _arena);

            EnemyMovementBatchResolutionEntry entry = ResolveSingle(
                resolver,
                CreateIntent(1UL, new Position3D(21f, GroundY, 0f), Direction3D.Left, 0.2f));

            Assert.That(entry.AcceptedPosition.X, Is.EqualTo(20.8f).Within(0.0001f));
        }

        [Test]
        public void MissingTransformStillReturnsAnUnblockedPosition()
        {
            UnityEnemyMovementResolver resolver = new(new TestEnemyTransformRegistry(), _arena);

            EnemyMovementBatchResolutionEntry entry = ResolveSingle(
                resolver,
                CreateIntent(1UL, new Position3D(10f, GroundY, 0f), Direction3D.Left, 0.5f));

            Assert.That(entry.AcceptedPosition.X, Is.EqualTo(9.5f).Within(0.0001f));
        }

        [Test]
        public void ChangedIntentDirectionIsHonoredOnTheNextResolve()
        {
            UnityEnemyMovementResolver resolver = new(new TestEnemyTransformRegistry(), _arena);

            EnemyMovementBatchResolutionEntry first = ResolveSingle(
                resolver,
                CreateIntent(1UL, new Position3D(10f, GroundY, 0f), Direction3D.Left, 0.5f));

            EnemyMovementBatchResolutionEntry second = ResolveSingle(
                resolver,
                CreateIntent(1UL, new Position3D(10f, GroundY, 0f), Direction3D.Back, 0.5f));

            Assert.That(first.AcceptedPosition.X, Is.EqualTo(9.5f).Within(0.0001f));
            Assert.That(second.AcceptedPosition.X, Is.EqualTo(10f).Within(0.0001f));
            Assert.That(second.AcceptedPosition.Z, Is.EqualTo(-0.5f).Within(0.0001f));
        }

        private static EnemyMovementBatchResolutionEntry ResolveSingle(
            UnityEnemyMovementResolver resolver,
            EnemyMovementIntent intent)
        {
            IReadOnlyList<EnemyMovementBatchResolutionEntry> entries =
                resolver.Resolve(CreateRequest(intent));

            Assert.That(entries.Count, Is.EqualTo(1));

            return entries[0];
        }

        private static EnemyMovementBatchRequest CreateRequest(params EnemyMovementIntent[] intents)
        {
            return new EnemyMovementBatchRequest(
                new InteractionCorrelation(
                    ArenaRunId.FromValue(1UL),
                    InteractionId.None.Next(),
                    AggregateRevision.Initial),
                intents);
        }

        private static EnemyMovementIntent CreateIntent(
            ulong enemyId,
            Position3D from,
            Direction3D direction,
            float distance)
        {
            return new EnemyMovementIntent(
                EnemyId.FromValue(enemyId),
                new PlanarMovementIntent(from, direction, Distance.FromValue(distance)),
                CollisionRadius.FromValue(0.5f));
        }

        private Transform CreateBody(string name, Vector3 position)
        {
            GameObject body = new(name);
            body.transform.position = position;

            BoxCollider collider = body.AddComponent<BoxCollider>();
            collider.center = new Vector3(0f, 0.5f, 0f);
            collider.size = new Vector3(1f, 1f, 1f);

            _created.Add(body);
            Physics.SyncTransforms();

            return body.transform;
        }

        private void CreateWall(Vector3 position)
        {
            GameObject wall = new("Wall");
            wall.transform.position = position;

            BoxCollider collider = wall.AddComponent<BoxCollider>();
            collider.center = new Vector3(0f, 0.5f, 0f);
            collider.size = new Vector3(0.2f, 1f, 4f);

            _created.Add(wall);
            Physics.SyncTransforms();
        }

        private sealed class TestEnemyTransformRegistry : IEnemyTransformRegistry
        {
            private readonly Dictionary<EnemyId, Transform> _transforms = new();

            public void Register(EnemyId enemyId, Transform transform)
            {
                _transforms[enemyId] = transform;
            }

            public bool TryGetTransform(EnemyId enemyId, out Transform transform)
            {
                return _transforms.TryGetValue(enemyId, out transform);
            }
        }
    }
}
