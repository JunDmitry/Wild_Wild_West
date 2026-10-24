using System;
using Game.Arena.Application.ReadModels;
using Game.Arena.Domain.Aggregates.ArenaRun;
using UnityEngine;

namespace Game.Arena.Presentation.Camera
{
    public sealed class ThirdPersonCameraController : MonoBehaviour
    {
        private const string MouseHorizontalAxis = "Mouse X";
        private const string MouseVerticalAxis = "Mouse Y";

        [SerializeField] private Transform _target;
        [SerializeField] private float _distance = 6f;
        [SerializeField] private float _height = 2f;
        [SerializeField] private float _yawSensitivity = 3f;
        [SerializeField] private float _pitchSensitivity = 3f;
        [SerializeField] private float _minimumPitch = -10f;
        [SerializeField] private float _maximumPitch = 60f;
        [SerializeField] private float _initialPitch = 20f;

        private float _yaw;
        private float _pitch;
        private bool _isLookEnabled = true;

        public Transform Target
        {
            get => _target;
            set => _target = value;
        }

        public float Distance
        {
            get => _distance;
            set => _distance = value;
        }

        public float Height
        {
            get => _height;
            set => _height = value;
        }

        public float MinimumPitch => _minimumPitch;

        public float MaximumPitch => _maximumPitch;

        public float YawSensitivity => _yawSensitivity;

        public float PitchSensitivity => _pitchSensitivity;

        public float Yaw => _yaw;

        public float Pitch => _pitch;

        public void Present(ArenaRunSnapshot snapshot)
        {
            if (snapshot == null)
            {
                throw new ArgumentNullException(nameof(snapshot));
            }

            _isLookEnabled = snapshot.Status == ArenaRunStatus.Playing;
        }

        private void Awake()
        {
            _yaw = transform.eulerAngles.y;
            _pitch = Mathf.Clamp(_initialPitch, _minimumPitch, _maximumPitch);
        }

        private void Update()
        {
            ApplyLook(
                UnityEngine.Input.GetAxis(MouseHorizontalAxis),
                UnityEngine.Input.GetAxis(MouseVerticalAxis));
        }

        private void LateUpdate()
        {
            Follow();
        }

        public void ApplyLook(float mouseX, float mouseY)
        {
            if (_isLookEnabled == false)
            {
                return;
            }

            _yaw += mouseX * _yawSensitivity;
            _pitch = Mathf.Clamp(_pitch - (mouseY * _pitchSensitivity), _minimumPitch, _maximumPitch);
        }

        public void Follow()
        {
            if (_target == null)
            {
                return;
            }

            Quaternion rotation = Quaternion.Euler(_pitch, _yaw, 0f);
            Vector3 focus = _target.position + (Vector3.up * _height);
            Vector3 position = focus - ((rotation * Vector3.forward) * _distance);

            transform.SetPositionAndRotation(position, rotation);
        }
    }
}
