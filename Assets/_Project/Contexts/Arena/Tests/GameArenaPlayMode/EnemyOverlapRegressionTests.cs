using System;
using System.Collections.Generic;
using Game.Arena.Application.Ports;
using Game.Arena.Domain.Concurrency;
using Game.Arena.Domain.Configuration;
using Game.Arena.Domain.Geometry;
using Game.Arena.Domain.Identity;
using Game.Arena.Domain.Interactions;
using Game.Arena.Domain.Interactions.Movement;
using Game.Arena.Domain.Movement;
using Game.Arena.Infrastructure.Movement;
using Game.Arena.Infrastructure.Targeting;
using NUnit.Framework;
using UnityEngine;

namespace Game.Arena.PlayMode.Tests
{
    /// <summary>
    /// Regression coverage for enemy–enemy overlap. Each scenario asks the
    /// production resolver to resolve a batch in which the enemies move
    /// simultaneously, applies the accepted positions, and asserts that no
    /// pair ends closer than the sum of their collision radii. The current
    /// resolver only sweeps each enemy's own requested distance, so a pair
    /// that closes from both sides can pass through each other; these tests
    /// fail until the simultaneous motion is accounted for.
    /// </summary>
    [TestFixture]
    public sealed class EnemyOverlapRegressionTests
    {
        private const float ArenaHalfExtent = 30f;
        private const float GroundY = 0f;
        private const float Radius = 0.5f;
        private const float OverlapTolerance = 1e-3f;

        private readonly List<GameObject> _created = new();
        private ArenaDefinition _arena;
        private TestEnemyTransformRegistry _transforms;
        private TestEnemyColliderRegistry _colliders;

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

            _transforms = new TestEnemyTransformRegistry();
            _colliders = new TestEnemyColliderRegistry();
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
            Physics.SyncTransforms();
        }

        [Test]
        public void HeadOnSimultaneousMovement_DoesNotOverlap()
        {
            RegisterEnemy(1UL, new Vector3(10f, GroundY, 0f));
            RegisterEnemy(2UL, new Vector3(5.5f, GroundY, 0f));

            EnemyMovementBatchResolutionEntry[] entries = Resolve(
                CreateIntent(1UL, new Position3D(10f, GroundY, 0f), Direction3D.Left, 2.5f),
                CreateIntent(2UL, new Position3D(5.5f, GroundY, 0f), Direction3D.Right, 2.5f));

            AssertNoOverlap(entries, 0, 1);
        }

        [Test]
        public void PerpendicularCrossing_DoesNotOverlap()
        {
            RegisterEnemy(1UL, new Vector3(0f, GroundY, 0f));
            RegisterEnemy(2UL, new Vector3(2f, GroundY, 2f));

            EnemyMovementBatchResolutionEntry[] entries = Resolve(
                CreateIntent(1UL, new Position3D(0f, GroundY, 0f), Direction3D.Right, 2f),
                CreateIntent(2UL, new Position3D(2f, GroundY, 2f), Direction3D.Back, 2f));

            AssertNoOverlap(entries, 0, 1);
        }

        [Test]
        public void ThreeWayConvergence_DoesNotOverlap()
        {
            RegisterEnemy(1UL, new Vector3(5f, GroundY, 0f));
            RegisterEnemy(2UL, new Vector3(-5f, GroundY, 0f));
            RegisterEnemy(3UL, new Vector3(0f, GroundY, 5f));

            EnemyMovementBatchResolutionEntry[] entries = Resolve(
                CreateIntent(1UL, new Position3D(5f, GroundY, 0f), Direction3D.Left, 5f),
                CreateIntent(2UL, new Position3D(-5f, GroundY, 0f), Direction3D.Right, 5f),
                CreateIntent(3UL, new Position3D(0f, GroundY, 5f), Direction3D.Back, 5f));

            AssertNoOverlap(entries, 0, 1);
            AssertNoOverlap(entries, 0, 2);
            AssertNoOverlap(entries, 1, 2);
        }

        [Test]
        public void DiagonalApproach_DoesNotOverlap()
        {
            RegisterEnemy(1UL, new Vector3(0f, GroundY, 0f));
            RegisterEnemy(2UL, new Vector3(4f, GroundY, 4f));
            Direction3D diagonal = Direction3D.From(new Displacement3D(1f, 0f, 1f));
            Direction3D opposite = Direction3D.From(new Displacement3D(-1f, 0f, -1f));

            EnemyMovementBatchResolutionEntry[] entries = Resolve(
                CreateIntent(1UL, new Position3D(0f, GroundY, 0f), diagonal, 2.8284271f),
                CreateIntent(2UL, new Position3D(4f, GroundY, 4f), opposite, 2.8284271f));

            AssertNoOverlap(entries, 0, 1);
        }

        [Test]
        public void PreExistingOverlap_IsNotWorsened()
        {
            RegisterEnemy(1UL, new Vector3(0f, GroundY, 0f));
            RegisterEnemy(2UL, new Vector3(0.5f, GroundY, 0f));
            Position3D first = new(0f, GroundY, 0f);
            Position3D second = new(0.5f, GroundY, 0f);
            float before = GroundDistance(first, second);

            EnemyMovementBatchResolutionEntry[] entries = Resolve(
                CreateIntent(1UL, first, Direction3D.Right, 0.5f),
                CreateIntent(2UL, second, Direction3D.Left, 0.5f));

            float after = GroundDistance(
                entries[0].AcceptedPosition,
                entries[1].AcceptedPosition);

            Assert.That(
                after,
                Is.GreaterThanOrEqualTo(before - OverlapTolerance),
                "Simultaneous movement worsened an existing overlap: " + before + " -> " + after);
        }

        [Test]
        public void ColliderlessConvergence_DoesNotOverlap()
        {
            // Models two live enemies that are not presented yet (spawn band
            // outside Arena Bounds): they have no view and no physics collider,
            // but both move in the same batch.
            UnityEnemyMovementResolver resolver = new(_transforms, _arena, _colliders);

            EnemyMovementBatchResolutionEntry[] entries = Resolve(
                resolver,
                CreateIntent(1UL, new Position3D(10f, GroundY, 0f), Direction3D.Left, 2.5f),
                CreateIntent(2UL, new Position3D(5.5f, GroundY, 0f), Direction3D.Right, 2.5f));

            AssertNoOverlap(entries, 0, 1);
        }

        [Test]
        public void StationaryEnemyOutsideBatch_BlocksMover()
        {
            // Enemy 2 is stationary and the Domain omits it from the movement
            // batch (pending attack or already in attack range). It has no view
            // and no collider, so only the live occupancy can stop enemy 1.
            Position3D moverStart = new(10f, GroundY, 0f);
            Position3D stationary = new(5.5f, GroundY, 0f);
            EnemyFootprint[] occupied =
            {
                new EnemyFootprint(
                    EnemyId.FromValue(1UL),
                    moverStart,
                    CollisionRadius.FromValue(Radius)),
                new EnemyFootprint(
                    EnemyId.FromValue(2UL),
                    stationary,
                    CollisionRadius.FromValue(Radius)),
            };

            EnemyMovementBatchResolutionEntry[] entries = Resolve(
                new UnityEnemyMovementResolver(_transforms, _arena, _colliders),
                occupied,
                CreateIntent(1UL, moverStart, Direction3D.Left, 4f));

            float distance = GroundDistance(entries[0].AcceptedPosition, stationary);

            Assert.That(
                distance,
                Is.GreaterThanOrEqualTo((Radius + Radius) - OverlapTolerance),
                "The mover reached the stationary Enemy's footprint: distance "
                + distance + ".");
        }

        [Test]
        public void StationaryEnemyWithoutViewOrCollider_BlocksMover()
        {
            // Enemy 2's footprint straddles the Arena boundary (centre 29.8,
            // radius 0.5), so Presentation creates no view and therefore no
            // collider. It is also outside the movement batch. Only the live
            // occupancy can stop the mover before it overlaps.
            Position3D moverStart = new(27f, GroundY, 0f);
            Position3D stationary = new(29.8f, GroundY, 0f);
            EnemyFootprint[] occupied =
            {
                new EnemyFootprint(
                    EnemyId.FromValue(1UL),
                    moverStart,
                    CollisionRadius.FromValue(Radius)),
                new EnemyFootprint(
                    EnemyId.FromValue(2UL),
                    stationary,
                    CollisionRadius.FromValue(Radius)),
            };

            EnemyMovementBatchResolutionEntry[] entries = Resolve(
                new UnityEnemyMovementResolver(_transforms, _arena, _colliders),
                occupied,
                CreateIntent(1UL, moverStart, Direction3D.Right, 4f));

            float distance = GroundDistance(entries[0].AcceptedPosition, stationary);

            Assert.That(
                distance,
                Is.GreaterThanOrEqualTo((Radius + Radius) - OverlapTolerance),
                "The mover reached the viewless, colliderless Enemy's footprint: distance "
                + distance + ".");
        }

        [Test]
        public void RepeatedConvergence_StaysNonOverlappingEveryTick()
        {
            UnityEnemyMovementResolver resolver = new(_transforms, _arena, _colliders);
            EnemyMovementIntent[] intents =
            {
                CreateIntent(1UL, new Position3D(10f, GroundY, 0f), Direction3D.Left, 2.5f),
                CreateIntent(2UL, new Position3D(-10f, GroundY, 0f), Direction3D.Right, 2.5f),
            };

            for (int tick = 0; tick < 8; tick++)
            {
                IReadOnlyList<EnemyMovementBatchResolutionEntry> resolved =
                    resolver.Resolve(CreateRequest(intents), ReadOnlySpan<EnemyFootprint>.Empty);

                Assert.That(resolved.Count, Is.EqualTo(intents.Length));
                AssertNoOverlap(resolved, 0, 1);

                for (int index = 0; index < intents.Length; index++)
                {
                    intents[index] = WithFrom(intents[index], resolved[index].AcceptedPosition);
                }
            }
        }

        private void RegisterEnemy(ulong value, Vector3 position)
        {
            EnemyId enemyId = EnemyId.FromValue(value);
            Transform body = CreateBody("Enemy " + value, position);
            _transforms.Register(enemyId, body);
            _colliders.Register(body.GetComponent<Collider>(), enemyId);
        }

        private EnemyMovementBatchResolutionEntry[] Resolve(params EnemyMovementIntent[] intents)
        {
            return Resolve(
                new UnityEnemyMovementResolver(_transforms, _arena, _colliders),
                ReadOnlySpan<EnemyFootprint>.Empty,
                intents);
        }

        private static EnemyMovementBatchResolutionEntry[] Resolve(
            UnityEnemyMovementResolver resolver,
            params EnemyMovementIntent[] intents)
        {
            return Resolve(resolver, ReadOnlySpan<EnemyFootprint>.Empty, intents);
        }

        private static EnemyMovementBatchResolutionEntry[] Resolve(
            UnityEnemyMovementResolver resolver,
            ReadOnlySpan<EnemyFootprint> occupied,
            params EnemyMovementIntent[] intents)
        {
            IReadOnlyList<EnemyMovementBatchResolutionEntry> entries =
                resolver.Resolve(CreateRequest(intents), occupied);

            EnemyMovementBatchResolutionEntry[] copy = new EnemyMovementBatchResolutionEntry[entries.Count];

            for (int index = 0; index < entries.Count; index++)
            {
                copy[index] = entries[index];
            }

            return copy;
        }

        private static void AssertNoOverlap(
            IReadOnlyList<EnemyMovementBatchResolutionEntry> entries,
            int first,
            int second)
        {
            float distance = GroundDistance(
                entries[first].AcceptedPosition,
                entries[second].AcceptedPosition);

            Assert.That(
                distance,
                Is.GreaterThanOrEqualTo((Radius + Radius) - OverlapTolerance),
                "Enemies " + entries[first].EnemyId + " and " + entries[second].EnemyId
                + " overlap after resolution: distance " + distance
                + " is below the radii sum " + (Radius + Radius) + ".");
        }

        private static float GroundDistance(Position3D first, Position3D second)
        {
            float dx = first.X - second.X;
            float dz = first.Z - second.Z;

            return Mathf.Sqrt((dx * dx) + (dz * dz));
        }

        private static EnemyMovementIntent WithFrom(EnemyMovementIntent intent, Position3D from)
        {
            return new EnemyMovementIntent(
                intent.EnemyId,
                new PlanarMovementIntent(from, intent.Intent.Direction, intent.Intent.RequestedDistance),
                intent.CollisionRadius);
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
                CollisionRadius.FromValue(Radius));
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

            public bool TryGetCollider(EnemyId enemyId, out Collider collider)
            {
                if (_transforms.TryGetValue(enemyId, out Transform transform) && transform != null)
                {
                    collider = transform.GetComponent<Collider>();

                    return collider != null;
                }

                collider = null;

                return false;
            }
        }

        private sealed class TestEnemyColliderRegistry : IEnemyColliderRegistry
        {
            private readonly Dictionary<Collider, EnemyId> _enemies = new();

            public void Register(Collider collider, EnemyId enemyId)
            {
                _enemies[collider] = enemyId;
            }

            public bool TryGetEnemyId(Collider collider, out EnemyId enemyId)
            {
                return _enemies.TryGetValue(collider, out enemyId);
            }
        }
    }
}
