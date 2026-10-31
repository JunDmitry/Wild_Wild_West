using System;
using System.Collections.Generic;
using Game.Arena.Application.Ports;
using Game.Arena.Domain.Combat;
using Game.Arena.Domain.Concurrency;
using Game.Arena.Domain.Configuration;
using Game.Arena.Domain.Geometry;
using Game.Arena.Domain.Identity;
using Game.Arena.Domain.Interactions;
using Game.Arena.Domain.Interactions.Spawn;
using Game.Arena.Infrastructure.Spawning;
using NUnit.Framework;
using UnityEngine;

namespace Game.Arena.PlayMode.Tests
{
    [TestFixture]
    public sealed class EnemySpawnPlacementResolverTests
    {
        private const float Tolerance = 1e-3f;

        private readonly List<GameObject> _objects = new();
        private ArenaDefinition _arena;

        [SetUp]
        public void SetUp()
        {
            _arena = new ArenaDefinition(
                new ArenaBounds(-20f, 20f, -20f, 20f, 0f),
                Distance.FromValue(5f));
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
        public void Resolve_CameraFacingOneSide_ReturnsPointOutsideItsFrustum()
        {
            Camera camera = CreateCamera(new Vector3(0f, 10f, 0f), Vector3.forward, 60f);
            Transform player = CreatePlayer(Vector3.zero, Quaternion.identity);
            UnityEnemySpawnPlacementResolver resolver = new(_arena, camera, player);

            Position3D spawn = resolver.ResolveSpawnPosition(
                CreateRequest(0.5f),
                ReadOnlySpan<EnemyFootprint>.Empty);

            Assert.That(IsInsideView(camera, spawn, 0.5f), Is.False);
            AssertIsSpawnBandPoint(spawn);
        }

        [Test]
        public void Resolve_ReturnsPointAcceptedByTheArenaDefinition()
        {
            Transform player = CreatePlayer(Vector3.zero, Quaternion.identity);
            UnityEnemySpawnPlacementResolver resolver = new(_arena, null, player);

            Position3D spawn = resolver.ResolveSpawnPosition(
                CreateRequest(0.5f),
                ReadOnlySpan<EnemyFootprint>.Empty);

            Assert.That(_arena.IsSpawnPosition(spawn), Is.True);
            Assert.That(spawn.Y, Is.EqualTo(_arena.Bounds.GroundY));
        }

        [Test]
        public void Resolve_ReturnsPointThatClearsTheArenaEdgeByCollisionRadius()
        {
            const float radius = 1f;
            Transform player = CreatePlayer(Vector3.zero, Quaternion.identity);
            UnityEnemySpawnPlacementResolver resolver = new(_arena, null, player);

            Position3D spawn = resolver.ResolveSpawnPosition(
                CreateRequest(radius),
                ReadOnlySpan<EnemyFootprint>.Empty);

            float expectedClearance = Mathf.Min(radius, _arena.SpawnBand.Value);
            Assert.That(OutwardDistanceFromArena(spawn), Is.GreaterThanOrEqualTo(expectedClearance - Tolerance));
            Assert.That(IsInsideArenaFootprint(spawn), Is.False);
        }

        [Test]
        public void Resolve_WholeBandVisible_ReturnsBandPointFarthestFromAim()
        {
            Camera camera = CreateCamera(new Vector3(0f, 150f, -150f), Vector3.zero, 179f);
            Transform player = CreatePlayer(Vector3.zero, Quaternion.identity);
            UnityEnemySpawnPlacementResolver resolver = new(_arena, camera, player);

            Position3D spawn = resolver.ResolveSpawnPosition(
                CreateRequest(0.5f),
                ReadOnlySpan<EnemyFootprint>.Empty);

            AssertIsSpawnBandPoint(spawn);
            Assert.That(spawn.Z, Is.LessThan(_arena.Bounds.MinimumZ));
            Assert.That(
                Vector3.Dot(new Vector3(spawn.X, 0f, spawn.Z), Vector3.forward),
                Is.LessThan(0f));
        }

        [Test]
        public void Resolve_WithoutCamera_ReturnsBandPointBehindTheAim()
        {
            Transform player = CreatePlayer(Vector3.zero, Quaternion.identity);
            UnityEnemySpawnPlacementResolver resolver = new(_arena, null, player);

            Position3D spawn = resolver.ResolveSpawnPosition(
                CreateRequest(0.5f),
                ReadOnlySpan<EnemyFootprint>.Empty);

            AssertIsSpawnBandPoint(spawn);
            Assert.That(spawn.Z, Is.LessThan(_arena.Bounds.MinimumZ));
        }

        [Test]
        public void Resolve_PlayerWithoutHorizontalAim_StillReturnsBandPoint()
        {
            Transform player = CreatePlayer(Vector3.zero, Quaternion.Euler(90f, 0f, 0f));
            UnityEnemySpawnPlacementResolver resolver = new(_arena, null, player);

            Position3D spawn = resolver.ResolveSpawnPosition(
                CreateRequest(0.5f),
                ReadOnlySpan<EnemyFootprint>.Empty);

            AssertIsSpawnBandPoint(spawn);
        }

        private void AssertIsSpawnBandPoint(Position3D spawn)
        {
            Assert.That(_arena.IsSpawnPosition(spawn), Is.True);
            Assert.That(spawn.Y, Is.EqualTo(_arena.Bounds.GroundY));
        }

        private bool IsInsideArenaFootprint(Position3D position)
        {
            ArenaBounds bounds = _arena.Bounds;

            return position.X >= bounds.MinimumX
                && position.X <= bounds.MaximumX
                && position.Z >= bounds.MinimumZ
                && position.Z <= bounds.MaximumZ;
        }

        private float OutwardDistanceFromArena(Position3D position)
        {
            ArenaBounds bounds = _arena.Bounds;
            float distance = 0f;

            if (position.X < bounds.MinimumX)
            {
                distance = Mathf.Max(distance, bounds.MinimumX - position.X);
            }
            else if (position.X > bounds.MaximumX)
            {
                distance = Mathf.Max(distance, position.X - bounds.MaximumX);
            }

            if (position.Z < bounds.MinimumZ)
            {
                distance = Mathf.Max(distance, bounds.MinimumZ - position.Z);
            }
            else if (position.Z > bounds.MaximumZ)
            {
                distance = Mathf.Max(distance, position.Z - bounds.MaximumZ);
            }

            return distance;
        }

        private static bool IsInsideView(Camera camera, Position3D position, float radius)
        {
            Plane[] planes = GeometryUtility.CalculateFrustumPlanes(camera);
            Bounds body = new(
                new Vector3(position.X, position.Y, position.Z),
                new Vector3(radius * 2f, radius * 2f, radius * 2f));

            return GeometryUtility.TestPlanesAABB(planes, body);
        }

        private static EnemySpawnRequest CreateRequest(float collisionRadius)
        {
            InteractionCorrelation correlation = new(
                ArenaRunId.FromValue(1UL),
                InteractionId.None.Next(),
                AggregateRevision.Initial);

            return new EnemySpawnRequest(
                correlation,
                EnemyKind.Regular,
                CollisionRadius.FromValue(collisionRadius));
        }

        private Camera CreateCamera(Vector3 position, Vector3 lookAt, float fieldOfView)
        {
            GameObject gameObject = new("Test Camera");
            _objects.Add(gameObject);
            gameObject.transform.position = position;
            gameObject.transform.LookAt(lookAt);

            Camera camera = gameObject.AddComponent<Camera>();
            camera.fieldOfView = fieldOfView;
            camera.aspect = 1f;
            camera.nearClipPlane = 0.3f;
            camera.farClipPlane = 1000f;

            return camera;
        }

        private Transform CreatePlayer(Vector3 position, Quaternion rotation)
        {
            GameObject gameObject = new("Test Player");
            _objects.Add(gameObject);
            gameObject.transform.SetPositionAndRotation(position, rotation);

            return gameObject.transform;
        }
    }
}
