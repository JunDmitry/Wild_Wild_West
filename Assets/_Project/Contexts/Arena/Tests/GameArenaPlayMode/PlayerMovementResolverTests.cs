using System.Collections.Generic;
using Game.Arena.Domain.Concurrency;
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
    public sealed class PlayerMovementResolverTests
    {
        private readonly List<GameObject> _objects = new();

        [SetUp]
        public void SetUp()
        {
            _objects.Clear();
        }

        [TearDown]
        public void TearDown()
        {
            for (int index = 0; index < _objects.Count; index++)
            {
                Object.DestroyImmediate(_objects[index]);
            }

            _objects.Clear();
            Physics.SyncTransforms();
        }

        [Test]
        public void Resolve_UnblockedPath_ReturnsRequestedPosition()
        {
            GameObject player = CreatePlayer(Vector3.zero, withCollider: true);
            UnityPlayerMovementResolver resolver = new(player.transform);

            Position3D accepted = resolver.Resolve(
                CreateRequest(Position3D.Zero, Direction3D.Forward, 5f));

            Assert.That(accepted.X, Is.EqualTo(0f).Within(1e-4f));
            Assert.That(accepted.Y, Is.EqualTo(0f));
            Assert.That(accepted.Z, Is.EqualTo(5f).Within(1e-4f));
        }

        [Test]
        public void Resolve_ColliderInPath_StopsAtColliderRadius()
        {
            GameObject player = CreatePlayer(Vector3.zero, withCollider: true);
            CreateWall(new Vector3(2.5f, 1f, 0f), new Vector3(1f, 2f, 4f));
            UnityPlayerMovementResolver resolver = new(player.transform);

            Position3D accepted = resolver.Resolve(
                CreateRequest(Position3D.Zero, Direction3D.Right, 5f));

            Assert.That(accepted.X, Is.EqualTo(1.5f).Within(0.02f));
            Assert.That(accepted.Y, Is.EqualTo(0f));
            Assert.That(accepted.Z, Is.EqualTo(0f).Within(1e-4f));
        }

        [Test]
        public void Resolve_ColliderInPath_StaysOnRequestedSegmentAndNeverBeyondDistance()
        {
            GameObject player = CreatePlayer(Vector3.zero, withCollider: true);
            CreateWall(new Vector3(3f, 1f, 3f), new Vector3(1f, 2f, 1f));
            UnityPlayerMovementResolver resolver = new(player.transform);

            PlayerMovementRequest request = CreateRequest(
                Position3D.Zero,
                Direction3D.From(new Displacement3D(1f, 0f, 1f)),
                5f);

            Position3D accepted = resolver.Resolve(request);

            float travelled = accepted.GroundDistanceTo(request.From);

            Assert.That(travelled, Is.GreaterThan(0f));
            Assert.That(travelled, Is.LessThanOrEqualTo(request.RequestedDistance.Value));
            Assert.That(accepted.X, Is.EqualTo(accepted.Z).Within(1e-3f));
        }

        [Test]
        public void Resolve_ColliderBeyondRequestedDistance_ReturnsRequestedPosition()
        {
            GameObject player = CreatePlayer(Vector3.zero, withCollider: true);
            CreateWall(new Vector3(10f, 1f, 0f), new Vector3(1f, 2f, 4f));
            UnityPlayerMovementResolver resolver = new(player.transform);

            Position3D accepted = resolver.Resolve(
                CreateRequest(Position3D.Zero, Direction3D.Right, 5f));

            Assert.That(accepted.X, Is.EqualTo(5f).Within(1e-4f));
        }

        [Test]
        public void Resolve_PlayerOwnCollider_DoesNotBlockMovement()
        {
            GameObject player = CreatePlayer(Vector3.zero, withCollider: true);
            UnityPlayerMovementResolver resolver = new(player.transform);

            Position3D accepted = resolver.Resolve(
                CreateRequest(Position3D.Zero, Direction3D.Right, 1f));

            Assert.That(accepted.X, Is.EqualTo(1f).Within(1e-4f));
        }

        [Test]
        public void Resolve_GroundPlane_DoesNotBlockHorizontalMovement()
        {
            GameObject player = CreatePlayer(Vector3.zero, withCollider: false);
            GameObject ground = CreateObject("Ground", Vector3.zero);
            BoxCollider groundCollider = ground.AddComponent<BoxCollider>();
            groundCollider.size = new Vector3(40f, 1f, 40f);
            groundCollider.center = new Vector3(0f, -0.5f, 0f);
            Physics.SyncTransforms();

            UnityPlayerMovementResolver resolver = new(player.transform);

            Position3D accepted = resolver.Resolve(
                CreateRequest(Position3D.Zero, Direction3D.Right, 1f));

            Assert.That(accepted.X, Is.EqualTo(1f).Within(1e-4f));
        }

        [Test]
        public void Resolve_RequestY_IsPreservedRegardlessOfTransformHeight()
        {
            GameObject player = CreatePlayer(new Vector3(0f, 0.5f, 0f), withCollider: true);
            UnityPlayerMovementResolver resolver = new(player.transform);

            Position3D accepted = resolver.Resolve(
                CreateRequest(new Position3D(0f, 0.25f, 0f), Direction3D.Forward, 1f));

            Assert.That(accepted.Y, Is.EqualTo(0.25f));
        }

        private GameObject CreatePlayer(Vector3 position, bool withCollider)
        {
            GameObject player = CreateObject("Player", position);

            if (withCollider)
            {
                CapsuleCollider collider = player.AddComponent<CapsuleCollider>();
                collider.height = 2f;
                collider.radius = 0.5f;
                collider.center = new Vector3(0f, 1f, 0f);
            }

            Physics.SyncTransforms();

            return player;
        }

        private GameObject CreateWall(Vector3 center, Vector3 size)
        {
            GameObject wall = CreateObject("Wall", center);
            BoxCollider collider = wall.AddComponent<BoxCollider>();
            collider.size = size;
            Physics.SyncTransforms();

            return wall;
        }

        private GameObject CreateObject(string name, Vector3 position)
        {
            GameObject gameObject = new(name);
            gameObject.transform.position = position;
            _objects.Add(gameObject);

            return gameObject;
        }

        private static PlayerMovementRequest CreateRequest(
            Position3D from,
            Direction3D direction,
            float distance)
        {
            InteractionCorrelation correlation = new(
                ArenaRunId.FromValue(1UL),
                InteractionId.None.Next(),
                AggregateRevision.Initial);

            return new PlayerMovementRequest(
                correlation,
                new PlanarMovementIntent(from, direction, Distance.FromValue(distance)),
                CollisionRadius.FromValue(0.5f));
        }
    }
}
