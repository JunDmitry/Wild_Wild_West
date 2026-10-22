using System.Collections.Generic;
using Game.Arena.Application.Ticks;
using Game.Arena.Domain.Geometry;
using Game.Arena.Domain.Identity;
using Game.Arena.Infrastructure.Input;
using Game.Arena.Infrastructure.Movement;
using Game.Arena.Infrastructure.Targeting;
using Game.Arena.Presentation.Player;
using NUnit.Framework;
using UnityEngine;

namespace Game.Arena.PlayMode.Tests
{
    [TestFixture]
    public sealed class PlayerTickAdapterTests
    {
        private readonly List<GameObject> _objects = new();
        private FakeInputDevice _device;
        private UnityPlayerInputSource _input;
        private GameObject _cameraObject;
        private GameObject _playerObject;
        private PlayModeTickHarness _harness;

        [SetUp]
        public void SetUp()
        {
            _objects.Clear();
            _device = new FakeInputDevice();

            _cameraObject = CreateObject("Camera");
            _cameraObject.AddComponent<Camera>();
            _cameraObject.transform.rotation = Quaternion.identity;

            _playerObject = CreateObject("Player");
            CapsuleCollider collider = _playerObject.AddComponent<CapsuleCollider>();
            collider.height = 2f;
            collider.radius = 0.5f;
            collider.center = new Vector3(0f, 1f, 0f);
            Physics.SyncTransforms();

            GameObject inputObject = CreateObject("Input");
            _input = inputObject.AddComponent<UnityPlayerInputSource>();
            _input.ViewTransform = _cameraObject.transform;
            _input.Device = _device;

            _harness = new PlayModeTickHarness(
                new EmptyEnemyColliderRegistry(),
                playerInputSource: _input,
                playerMovementResolver: new UnityPlayerMovementResolver(_playerObject.transform));
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
        public void Tick_CameraTurnedThenRead_PlayerMovesAlongNewViewDirection()
        {
            _device.Axes["Vertical"] = 1f;
            _cameraObject.transform.rotation = Quaternion.Euler(0f, 90f, 0f);

            ArenaRunTickResult result = _harness.TickFrame();
            Position3D position = result.Snapshot.Player.Position;

            Assert.That(position.X, Is.GreaterThan(0.4f));
            Assert.That(Mathf.Abs(position.Z), Is.LessThan(0.01f));
        }

        [Test]
        public void Tick_DiagonalInput_IsNotFasterThanSingleAxis()
        {
            _device.Axes["Vertical"] = 1f;
            _device.Axes["Horizontal"] = 0f;

            Position3D before = _harness.TickFrame().Snapshot.Player.Position;
            Position3D afterSingle = _harness.TickFrame().Snapshot.Player.Position;
            float singleAxisDistance = before.GroundDistanceTo(afterSingle);

            _device.Axes["Horizontal"] = 1f;

            Position3D beforeDiagonal = afterSingle;
            Position3D afterDiagonal = _harness.TickFrame().Snapshot.Player.Position;
            float diagonalDistance = beforeDiagonal.GroundDistanceTo(afterDiagonal);

            Assert.That(singleAxisDistance, Is.GreaterThan(0f));
            Assert.That(diagonalDistance, Is.EqualTo(singleAxisDistance).Within(0.01f));
            Assert.That(diagonalDistance, Is.LessThanOrEqualTo(singleAxisDistance + 0.01f));
        }

        [Test]
        public void Tick_ThenPresent_PlayerObjectMatchesSnapshotPosition()
        {
            _playerObject.AddComponent<PlayerView>();
            PlayerPresenter presenter = _playerObject.AddComponent<PlayerPresenter>();
            _device.Axes["Vertical"] = 1f;

            ArenaRunTickResult result = _harness.TickFrame();
            presenter.Present(result.Snapshot);

            Position3D expected = result.Snapshot.Player.Position;
            Vector3 actual = _playerObject.transform.position;

            Assert.That(actual.x, Is.EqualTo(expected.X).Within(1e-4f));
            Assert.That(actual.y, Is.EqualTo(expected.Y).Within(1e-4f));
            Assert.That(actual.z, Is.EqualTo(expected.Z).Within(1e-4f));
        }

        [Test]
        public void Tick_NoInput_PlayerDoesNotMove()
        {
            Position3D before = _harness.TickFrame().Snapshot.Player.Position;
            Position3D after = _harness.TickFrame().Snapshot.Player.Position;

            Assert.That(after, Is.EqualTo(before));
        }

        private GameObject CreateObject(string name)
        {
            GameObject gameObject = new(name);
            _objects.Add(gameObject);

            return gameObject;
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
