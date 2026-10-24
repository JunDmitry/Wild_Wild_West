using System.Collections.Generic;
using Game.Arena.Application.Input;
using Game.Arena.Application.Ports;
using Game.Arena.Application.Ticks;
using Game.Arena.Domain.Geometry;
using Game.Arena.Domain.Identity;
using Game.Arena.Infrastructure.Movement;
using Game.Arena.Infrastructure.Targeting;
using NUnit.Framework;
using UnityEngine;

namespace Game.Arena.PlayMode.Tests
{
    [TestFixture]
    public sealed class PlayerMovementBoundsSlideTests
    {
        private const float PlayerRadiusValue = 0.5f;
        private const float MaximumCenterZ = 19.5f;

        private static readonly ArenaBounds Bounds = new(-20f, 20f, -20f, 20f, 0f);
        private static readonly CollisionRadius PlayerRadius = CollisionRadius.FromValue(PlayerRadiusValue);

        private readonly List<GameObject> _objects = new();
        private MutableInputSource _input;
        private GameObject _playerObject;

        [SetUp]
        public void SetUp()
        {
            _objects.Clear();
            _input = new MutableInputSource();

            _playerObject = CreateObject("Player");
            CapsuleCollider collider = _playerObject.AddComponent<CapsuleCollider>();
            collider.height = 2f;
            collider.radius = PlayerRadiusValue;
            collider.center = new Vector3(0f, 1f, 0f);
            Physics.SyncTransforms();
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
        public void Tick_DiagonalInputAtBound_SlidesAlongBoundAndStaysInside()
        {
            PlayModeTickHarness harness = CreateHarness(new Position3D(0f, 0f, MaximumCenterZ));

            _input.Input = new PlayerFrameInput(
                new MovementAxes(1f, 1f),
                Direction3D.Forward,
                false,
                false);

            Position3D position = harness.TickFrame().Snapshot.Player.Position;

            Assert.That(position.Z, Is.EqualTo(MaximumCenterZ).Within(1e-3f));
            Assert.That(position.X, Is.GreaterThan(0f));
            Assert.That(position.X, Is.EqualTo(0.5f * 0.70710678f).Within(0.01f));
            Assert.That(Bounds.Contains(position, PlayerRadius), Is.True);
        }

        [Test]
        public void Tick_StraightOutwardInputAtBound_LeavesPositionUnchanged()
        {
            Position3D start = new(0f, 0f, MaximumCenterZ);
            PlayModeTickHarness harness = CreateHarness(start);

            _input.Input = new PlayerFrameInput(
                new MovementAxes(1f, 0f),
                Direction3D.Forward,
                false,
                false);

            Position3D position = harness.TickFrame().Snapshot.Player.Position;

            Assert.That(position, Is.EqualTo(start));
            Assert.That(Bounds.Contains(position, PlayerRadius), Is.True);
        }

        [Test]
        public void Tick_DiagonalInputIntoCorner_StaysInsideBounds()
        {
            Position3D start = new(19.4f, 0f, 19.4f);
            PlayModeTickHarness harness = CreateHarness(start);

            _input.Input = new PlayerFrameInput(
                new MovementAxes(1f, 1f),
                Direction3D.Forward,
                false,
                false);

            Position3D position = harness.TickFrame().Snapshot.Player.Position;

            Assert.That(position.GroundDistanceTo(start), Is.GreaterThan(0f));
            Assert.That(position.X, Is.LessThanOrEqualTo(MaximumCenterZ + 1e-3f));
            Assert.That(position.Z, Is.LessThanOrEqualTo(MaximumCenterZ + 1e-3f));
            Assert.That(Bounds.Contains(position, PlayerRadius), Is.True);
        }

        private PlayModeTickHarness CreateHarness(Position3D playerStart)
        {
            return new PlayModeTickHarness(
                new EmptyEnemyColliderRegistry(),
                playerInputSource: _input,
                playerMovementResolver: new UnityPlayerMovementResolver(_playerObject.transform),
                playerStart: playerStart);
        }

        private GameObject CreateObject(string name)
        {
            GameObject gameObject = new(name);
            _objects.Add(gameObject);

            return gameObject;
        }

        private sealed class MutableInputSource : IPlayerInputSource
        {
            public MutableInputSource()
            {
                Input = new PlayerFrameInput(
                    MovementAxes.Zero,
                    Direction3D.Forward,
                    false,
                    false);
            }

            public PlayerFrameInput Input { get; set; }

            public PlayerFrameInput Read()
            {
                return Input;
            }
        }

        private sealed class EmptyEnemyColliderRegistry : IEnemyColliderRegistry
        {
            public bool TryGetEnemyId(Collider collider, out EnemyId enemyId)
            {
                enemyId = EnemyId.None;
                return false;
            }
        }
    }
}
