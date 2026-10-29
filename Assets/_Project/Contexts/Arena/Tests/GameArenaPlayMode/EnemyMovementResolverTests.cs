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

        [Test]
        public void ResolveQueriesAtMostThreeIntentsPerBatch()
        {
            UnityEnemyMovementResolver resolver = new(new TestEnemyTransformRegistry(), _arena);
            EnemyMovementIntent[] intents = CreateLineOfEnemies(10, 3f, Direction3D.Left, 0.5f);
            CreateLeftWalls(intents);

            IReadOnlyList<EnemyMovementBatchResolutionEntry> first =
                resolver.Resolve(CreateRequest(intents));

            for (int index = 0; index < first.Count; index++)
            {
                if (index < 3)
                {
                    AssertShortened(first, index);
                }
                else
                {
                    AssertUnshortened(first, index);
                }
            }

            IReadOnlyList<EnemyMovementBatchResolutionEntry> second =
                resolver.Resolve(CreateRequest(intents));

            for (int index = 0; index < second.Count; index++)
            {
                if (index < 6)
                {
                    AssertShortened(second, index);
                }
                else
                {
                    AssertUnshortened(second, index);
                }
            }
        }

        [Test]
        public void EveryIntentIsQueriedWithinTheStalenessBound()
        {
            UnityEnemyMovementResolver resolver = new(new TestEnemyTransformRegistry(), _arena);
            EnemyMovementIntent[] intents = CreateLineOfEnemies(10, 3f, Direction3D.Left, 0.5f);
            CreateLeftWalls(intents);

            IReadOnlyList<EnemyMovementBatchResolutionEntry> entries = null;

            for (int resolve = 0; resolve < 4; resolve++)
            {
                entries = resolver.Resolve(CreateRequest(intents));
            }

            for (int index = 0; index < entries.Count; index++)
            {
                AssertShortened(entries, index);
            }
        }

        [Test]
        public void FirstSightEnemyIsQueriedBeforeCachedPeers()
        {
            UnityEnemyMovementResolver resolver = new(new TestEnemyTransformRegistry(), _arena);
            EnemyMovementIntent first = CreateIntent(1UL, new Position3D(10f, GroundY, 0f), Direction3D.Left, 0.5f);
            EnemyMovementIntent second = CreateIntent(2UL, new Position3D(10f, GroundY, 4f), Direction3D.Left, 0.5f);
            EnemyMovementIntent third = CreateIntent(3UL, new Position3D(10f, GroundY, 8f), Direction3D.Left, 0.5f);
            EnemyMovementIntent newcomer = CreateIntent(4UL, new Position3D(10f, GroundY, 12f), Direction3D.Left, 0.5f);

            resolver.Resolve(CreateRequest(first, second, third));

            CreateWall(new Vector3(9.2f, GroundY, 0f), new Vector3(0.2f, 1f, 2f));
            CreateWall(new Vector3(9.2f, GroundY, 12f), new Vector3(0.2f, 1f, 2f));

            IReadOnlyList<EnemyMovementBatchResolutionEntry> entries =
                resolver.Resolve(CreateRequest(first, second, third, newcomer));

            AssertShortened(entries, 3);
            AssertUnshortened(entries, 2);
        }

        [Test]
        public void ChangedDirectionBreaksTheAgeTie()
        {
            UnityEnemyMovementResolver resolver = new(new TestEnemyTransformRegistry(), _arena);
            EnemyMovementIntent first = CreateIntent(1UL, new Position3D(10f, GroundY, 0f), Direction3D.Left, 0.5f);
            EnemyMovementIntent second = CreateIntent(2UL, new Position3D(10f, GroundY, 4f), Direction3D.Left, 0.5f);
            EnemyMovementIntent third = CreateIntent(3UL, new Position3D(10f, GroundY, 8f), Direction3D.Left, 0.5f);

            resolver.Resolve(CreateRequest(first, second, third));

            CreateWall(new Vector3(9.2f, GroundY, 0f), new Vector3(0.2f, 1f, 2f));
            CreateWall(new Vector3(10f, GroundY, 7.2f), new Vector3(2f, 1f, 0.2f));

            EnemyMovementIntent turnedThird = CreateIntent(
                3UL,
                new Position3D(10f, GroundY, 8f),
                Direction3D.Back,
                0.5f);
            EnemyMovementIntent newcomer = CreateIntent(
                4UL,
                new Position3D(10f, GroundY, -6f),
                Direction3D.Left,
                0.5f);

            IReadOnlyList<EnemyMovementBatchResolutionEntry> entries =
                resolver.Resolve(CreateRequest(first, second, turnedThird, newcomer));

            AssertShortened(entries, 0);
            AssertUnshortened(entries, 1);

            Assert.That(entries[2].AcceptedPosition.X, Is.EqualTo(10f).Within(0.0001f));
            Assert.That(entries[2].AcceptedPosition.Z, Is.GreaterThan(7.5f));
            Assert.That(entries[2].AcceptedPosition.Z, Is.LessThan(8f));
        }

        [Test]
        public void ReusedResultLiesOnTheRequestedPath()
        {
            UnityEnemyMovementResolver resolver = new(new TestEnemyTransformRegistry(), _arena);
            EnemyMovementIntent[] intents = CreateLineOfEnemies(4, 4f, Direction3D.Left, 0.5f);
            CreateLeftWalls(intents);

            IReadOnlyList<EnemyMovementBatchResolutionEntry> first =
                resolver.Resolve(CreateRequest(intents));

            IReadOnlyList<EnemyMovementBatchResolutionEntry> second =
                resolver.Resolve(CreateRequest(intents));

            AssertShortened(first, 2);
            AssertShortened(second, 2);
            Assert.That(
                second[2].AcceptedPosition.X,
                Is.EqualTo(first[2].AcceptedPosition.X).Within(0.0001f),
                "The reused result did not reapply the cached reachable distance.");
            Assert.That(
                second[2].AcceptedPosition.Z,
                Is.EqualTo(first[2].AcceptedPosition.Z).Within(0.0001f));

            Position3D from = intents[2].Intent.From;
            Position3D reused = second[2].AcceptedPosition;

            Assert.That(reused.Y, Is.EqualTo(from.Y).Within(0.0001f));
            Assert.That(reused.Z, Is.EqualTo(from.Z).Within(0.0001f));
            Assert.That(reused.GroundDistanceTo(from), Is.LessThanOrEqualTo(0.5f));
            Assert.That(reused.GroundDistanceTo(from), Is.GreaterThan(0f));
        }

        [Test]
        public void SkippedEnemyDoesNotCrossACachedObstructionAcrossTicks()
        {
            UnityEnemyMovementResolver resolver = new(new TestEnemyTransformRegistry(), _arena);
            EnemyMovementIntent[] intents = CreateLineOfEnemies(4, 4f, Direction3D.Left, 0.5f);

            // Walls ahead of the first three enemies only. The fourth has a
            // clear path, so its blind first-sight step crosses nothing.
            for (int index = 0; index < 3; index++)
            {
                Position3D from = intents[index].Intent.From;
                CreateWall(new Vector3(from.X - 0.8f, GroundY, from.Z), new Vector3(0.2f, 1f, 2f));
            }

            // Wall face at x = 9.3; with the 0.5 radius the centre may never
            // fall below 9.8. A reused per-tick allowance would take it inside.
            const float MinimumCentreX = 9.8f;

            for (int tick = 0; tick < 6; tick++)
            {
                IReadOnlyList<EnemyMovementBatchResolutionEntry> entries =
                    resolver.Resolve(CreateRequest(intents));

                for (int index = 0; index < 3; index++)
                {
                    Assert.That(
                        entries[index].AcceptedPosition.X,
                        Is.GreaterThanOrEqualTo(MinimumCentreX),
                        "Enemy " + index + " crossed the wall on tick " + tick + ".");
                }

                Advance(intents, entries);
            }
        }

        [Test]
        public void SkippedEnemyDoesNotMoveIntoTheBlockerItDetected()
        {
            UnityEnemyMovementResolver resolver = new(new TestEnemyTransformRegistry(), _arena);

            // The target is the third intent, so the age-priority selection
            // skips it on the second resolve and makes it reuse a result.
            EnemyMovementIntent[] intents =
            {
                CreateIntent(1UL, new Position3D(10f, GroundY, -6f), Direction3D.Left, 0.5f),
                CreateIntent(2UL, new Position3D(10f, GroundY, -2f), Direction3D.Left, 0.5f),
                CreateIntent(3UL, new Position3D(10f, GroundY, 0f), Direction3D.Left, 0.5f),
                CreateIntent(4UL, new Position3D(10f, GroundY, 6f), Direction3D.Left, 0.5f),
            };

            // A stationary 1x1x1 body whose +x face is at x = 8.9. The target
            // starts with a 0.5 step against a 0.6 gap, so a reused per-tick
            // allowance would carry it inside the body.
            CreateBody("Blocker", new Vector3(8.4f, GroundY, 0f));

            const float MinimumTargetX = 9.4f;

            for (int tick = 0; tick < 4; tick++)
            {
                IReadOnlyList<EnemyMovementBatchResolutionEntry> entries =
                    resolver.Resolve(CreateRequest(intents));

                Assert.That(
                    entries[2].AcceptedPosition.X,
                    Is.GreaterThanOrEqualTo(MinimumTargetX),
                    "The target moved into the blocker on tick " + tick + ".");

                Advance(intents, entries);
            }
        }

        [Test]
        public void UnblockedEnemyKeepsItsFullStepOnSkippedTicks()
        {
            UnityEnemyMovementResolver resolver = new(new TestEnemyTransformRegistry(), _arena);
            EnemyMovementIntent[] intents = CreateLineOfEnemies(4, 4f, Direction3D.Left, 0.5f);

            for (int tick = 0; tick < 4; tick++)
            {
                IReadOnlyList<EnemyMovementBatchResolutionEntry> entries =
                    resolver.Resolve(CreateRequest(intents));

                for (int index = 0; index < intents.Length; index++)
                {
                    Assert.That(
                        intents[index].Intent.From.GroundDistanceTo(entries[index].AcceptedPosition),
                        Is.EqualTo(0.5f).Within(0.0001f),
                        "Enemy " + index + " stalled on tick " + tick + " without an obstruction.");
                }

                Advance(intents, entries);
            }
        }

        private static void Advance(
            EnemyMovementIntent[] intents,
            IReadOnlyList<EnemyMovementBatchResolutionEntry> entries)
        {
            for (int index = 0; index < intents.Length; index++)
            {
                intents[index] = WithFrom(intents[index], entries[index].AcceptedPosition);
            }
        }

        private static EnemyMovementIntent WithFrom(EnemyMovementIntent intent, Position3D from)
        {
            return new EnemyMovementIntent(
                intent.EnemyId,
                new PlanarMovementIntent(from, intent.Intent.Direction, intent.Intent.RequestedDistance),
                intent.CollisionRadius);
        }

        private static EnemyMovementIntent[] CreateLineOfEnemies(
            int count,
            float spacing,
            Direction3D direction,
            float distance)
        {
            EnemyMovementIntent[] intents = new EnemyMovementIntent[count];
            float start = -spacing * (count - 1) * 0.5f;

            for (int index = 0; index < count; index++)
            {
                intents[index] = CreateIntent(
                    (ulong)(index + 1),
                    new Position3D(10f, GroundY, start + (spacing * index)),
                    direction,
                    distance);
            }

            return intents;
        }

        private void CreateLeftWalls(EnemyMovementIntent[] intents)
        {
            for (int index = 0; index < intents.Length; index++)
            {
                Position3D from = intents[index].Intent.From;
                CreateWall(new Vector3(from.X - 0.8f, GroundY, from.Z), new Vector3(0.2f, 1f, 2f));
            }
        }

        private static void AssertShortened(
            IReadOnlyList<EnemyMovementBatchResolutionEntry> entries,
            int index)
        {
            Assert.That(
                entries[index].AcceptedPosition.X,
                Is.GreaterThan(9.5f),
                "Entry " + index + " was not shortened by the wall.");
            Assert.That(
                entries[index].AcceptedPosition.X,
                Is.LessThan(10f),
                "Entry " + index + " did not stay behind its origin.");
        }

        private static void AssertUnshortened(
            IReadOnlyList<EnemyMovementBatchResolutionEntry> entries,
            int index)
        {
            Assert.That(
                entries[index].AcceptedPosition.X,
                Is.EqualTo(9.5f).Within(0.0001f),
                "Entry " + index + " was shortened although it should not have been queried.");
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
            CreateWall(position, new Vector3(0.2f, 1f, 4f));
        }

        private void CreateWall(Vector3 position, Vector3 size)
        {
            GameObject wall = new("Wall");
            wall.transform.position = position;

            BoxCollider collider = wall.AddComponent<BoxCollider>();
            collider.center = new Vector3(0f, 0.5f, 0f);
            collider.size = size;

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
