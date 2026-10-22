using System.Collections.Generic;
using Game.Arena.Application.Input;
using Game.Arena.Domain.Geometry;
using Game.Arena.Infrastructure.Input;
using NUnit.Framework;
using UnityEngine;

namespace Game.Arena.PlayMode.Tests
{
    [TestFixture]
    public sealed class PlayerInputSourceTests
    {
        private readonly List<GameObject> _objects = new();
        private FakeInputDevice _device;
        private GameObject _viewObject;
        private UnityPlayerInputSource _source;

        [SetUp]
        public void SetUp()
        {
            _objects.Clear();
            _device = new FakeInputDevice();

            _viewObject = CreateObject("View");
            _viewObject.transform.rotation = Quaternion.identity;

            GameObject sourceObject = CreateObject("Input");
            _source = sourceObject.AddComponent<UnityPlayerInputSource>();
            _source.ViewTransform = _viewObject.transform;
            _source.Device = _device;
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
        public void Read_RawDeviceAxes_ArePassedThroughUnchanged()
        {
            _device.Axes["Horizontal"] = 0.75f;
            _device.Axes["Vertical"] = -0.25f;

            PlayerFrameInput input = _source.Read();

            Assert.That(input.MovementAxes.Forward, Is.EqualTo(-0.25f));
            Assert.That(input.MovementAxes.Right, Is.EqualTo(0.75f));
        }

        [Test]
        public void Read_CameraFacingRight_ProducesHorizontalUnitViewDirection()
        {
            _viewObject.transform.rotation = Quaternion.Euler(0f, 90f, 0f);

            Direction3D viewDirection = _source.Read().ViewDirection;

            Assert.That(viewDirection.Y, Is.EqualTo(0f));
            Assert.That(viewDirection.X, Is.EqualTo(1f).Within(1e-4f));
            Assert.That(viewDirection.Z, Is.EqualTo(0f).Within(1e-4f));
        }

        [Test]
        public void Read_TiltedCamera_ProjectsForwardOntoGroundPlane()
        {
            _viewObject.transform.rotation = Quaternion.Euler(45f, 0f, 0f);

            Direction3D viewDirection = _source.Read().ViewDirection;

            Assert.That(viewDirection.Y, Is.EqualTo(0f));
            Assert.That(viewDirection.Z, Is.EqualTo(1f).Within(1e-4f));
        }

        [Test]
        public void Read_DegenerateCamera_ReusesLastValidViewDirection()
        {
            _viewObject.transform.rotation = Quaternion.Euler(0f, 45f, 0f);
            Direction3D first = _source.Read().ViewDirection;

            _viewObject.transform.rotation = Quaternion.Euler(-90f, 45f, 0f);
            Direction3D second = _source.Read().ViewDirection;

            Assert.That(second, Is.EqualTo(first));
        }

        [Test]
        public void Read_FirstFrameLookingStraightDown_FallsBackToForward()
        {
            _viewObject.transform.rotation = Quaternion.Euler(-90f, 0f, 0f);

            Direction3D viewDirection = _source.Read().ViewDirection;

            Assert.That(viewDirection, Is.EqualTo(Direction3D.Forward));
        }

        [Test]
        public void Read_AttackAndSwitchDown_ReportsBothPresses()
        {
            _device.ButtonsDown.Add("Fire1");
            _device.KeysDown.Add(KeyCode.Q);

            PlayerFrameInput input = _source.Read();

            Assert.That(input.AttackPressed, Is.True);
            Assert.That(input.SwitchWeaponPressed, Is.True);
        }

        [Test]
        public void Read_ButtonsNotDown_ReportsNeitherPress()
        {
            PlayerFrameInput input = _source.Read();

            Assert.That(input.AttackPressed, Is.False);
            Assert.That(input.SwitchWeaponPressed, Is.False);
        }

        private GameObject CreateObject(string name)
        {
            GameObject gameObject = new(name);
            _objects.Add(gameObject);

            return gameObject;
        }
    }
}
