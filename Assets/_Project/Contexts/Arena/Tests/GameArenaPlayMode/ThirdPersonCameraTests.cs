using System.Collections.Generic;
using Game.Arena.Presentation.Camera;
using NUnit.Framework;
using UnityEngine;

namespace Game.Arena.PlayMode.Tests
{
    [TestFixture]
    public sealed class ThirdPersonCameraTests
    {
        private readonly List<GameObject> _objects = new();
        private GameObject _cameraObject;
        private GameObject _targetObject;
        private ThirdPersonCameraController _controller;

        [SetUp]
        public void SetUp()
        {
            _objects.Clear();

            _targetObject = new GameObject("Player");
            _targetObject.transform.position = Vector3.zero;
            _objects.Add(_targetObject);

            _cameraObject = new GameObject("Camera");
            _cameraObject.AddComponent<Camera>();
            _objects.Add(_cameraObject);

            _controller = _cameraObject.AddComponent<ThirdPersonCameraController>();
            _controller.Target = _targetObject.transform;
            _controller.Distance = 6f;
            _controller.Height = 2f;
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
        public void ApplyLook_MouseHorizontal_ChangesYaw()
        {
            float before = _controller.Yaw;

            _controller.ApplyLook(10f, 0f);

            Assert.That(
                _controller.Yaw,
                Is.EqualTo(before + (10f * _controller.YawSensitivity)).Within(1e-3f));
        }

        [Test]
        public void ApplyLook_MouseUp_ClampsPitchAtMaximum()
        {
            _controller.ApplyLook(0f, -1000f);

            Assert.That(_controller.Pitch, Is.EqualTo(_controller.MaximumPitch).Within(1e-4f));
        }

        [Test]
        public void ApplyLook_MouseDown_ClampsPitchAtMinimum()
        {
            _controller.ApplyLook(0f, 1000f);

            Assert.That(_controller.Pitch, Is.EqualTo(_controller.MinimumPitch).Within(1e-4f));
        }

        [Test]
        public void Follow_PlacesCameraAtConfiguredDistanceFromTargetPoint()
        {
            _controller.Follow();

            Vector3 focus = _targetObject.transform.position + (Vector3.up * _controller.Height);

            Assert.That(
                Vector3.Distance(_cameraObject.transform.position, focus),
                Is.EqualTo(_controller.Distance).Within(1e-3f));
        }

        [Test]
        public void Follow_PointsCameraAtTargetPoint()
        {
            _controller.ApplyLook(30f, 15f);
            _controller.Follow();

            Vector3 focus = _targetObject.transform.position + (Vector3.up * _controller.Height);
            Vector3 toFocus = (focus - _cameraObject.transform.position).normalized;

            Assert.That(
                Vector3.Dot(_cameraObject.transform.forward, toFocus),
                Is.EqualTo(1f).Within(1e-3f));
        }

        [Test]
        public void Follow_PlayerStaysOnScreenAtEveryAllowedPitch()
        {
            Camera camera = _cameraObject.GetComponent<Camera>();
            float minimum = _controller.MinimumPitch;
            float maximum = _controller.MaximumPitch;

            for (int step = 0; step <= 10; step++)
            {
                SetPitch(Mathf.Lerp(minimum, maximum, step / 10f));
                _controller.Follow();

                AssertVisible(camera, _targetObject.transform.position);
                AssertVisible(camera, _targetObject.transform.position + (Vector3.up * 1.8f));
            }
        }

        [Test]
        public void Follow_WithStationaryTarget_CameraStillOrbits()
        {
            _controller.Follow();
            Vector3 firstPosition = _cameraObject.transform.position;
            Vector3 targetPosition = _targetObject.transform.position;

            _controller.ApplyLook(45f, 0f);
            _controller.Follow();
            Vector3 secondPosition = _cameraObject.transform.position;

            Assert.That(Vector3.Distance(firstPosition, secondPosition), Is.GreaterThan(0.1f));
            Assert.That(_targetObject.transform.position, Is.EqualTo(targetPosition));
        }

        private void SetPitch(float desiredPitch)
        {
            float mouseY = (_controller.Pitch - desiredPitch) / _controller.PitchSensitivity;
            _controller.ApplyLook(0f, mouseY);
        }

        private static void AssertVisible(Camera camera, Vector3 worldPoint)
        {
            Vector3 viewportPoint = camera.WorldToViewportPoint(worldPoint);

            Assert.That(viewportPoint.z, Is.GreaterThan(0f), "Player point is behind the camera.");
            Assert.That(viewportPoint.x, Is.InRange(0f, 1f), "Player point is off screen horizontally.");
            Assert.That(viewportPoint.y, Is.InRange(0f, 1f), "Player point is off screen vertically.");
        }
    }
}
