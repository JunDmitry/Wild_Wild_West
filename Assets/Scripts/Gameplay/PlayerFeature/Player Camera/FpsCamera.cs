using System;
using UnityEngine;

namespace Assets.Scripts.Gameplay.PlayerFeature.PlayerCamera
{
    public class FpsCamera : MonoBehaviour
    {
        private const string MouseX = "Mouse X";
        private const string MouseY = "Mouse Y";
        private const float CameraSpeedEpsilon = .1f;

        [SerializeField] private Camera _camera;
        [SerializeField, Min(float.Epsilon)] private float _cameraRotationSpeed = float.Epsilon;
        [SerializeField] private float _mouseSensitivity = 2f;
        [SerializeField] private Range _horizontalAngleRange;
        [SerializeField] private Range _verticalAngleRange;

        private float _currentHorizontalAngle;
        private float _currentVerticalAngle;

        private Quaternion _targetRotation;

        private void Start()
        {
            if (_camera == null)
                _camera = Camera.main;

            Vector3 euler = _camera.transform.localEulerAngles;
            _currentVerticalAngle = NormalizeAngle(euler.x);
            _currentHorizontalAngle = NormalizeAngle(euler.y);

            _targetRotation = _camera.transform.localRotation;

            Cursor.lockState = CursorLockMode.Locked;
            Cursor.visible = false;
        }

        private void Update()
        {
            if (_camera == null)
                return;

            float mouseX = Input.GetAxis(MouseX);
            float mouseY = -Input.GetAxis(MouseY);

            _currentHorizontalAngle += mouseX * _mouseSensitivity;
            _currentVerticalAngle += mouseY * _mouseSensitivity;

            _currentVerticalAngle = Mathf.Clamp(_currentVerticalAngle, _verticalAngleRange.Min, _verticalAngleRange.Max);
            _currentHorizontalAngle = Mathf.Clamp(_currentHorizontalAngle, _horizontalAngleRange.Min, _horizontalAngleRange.Max);

            _targetRotation = Quaternion.Euler(_currentVerticalAngle, _currentHorizontalAngle, 0);

            if (_cameraRotationSpeed <= CameraSpeedEpsilon)
            {
                _camera.transform.localRotation = _targetRotation;
            }
            else
            {
                _camera.transform.localRotation = Quaternion.RotateTowards(
                    _camera.transform.localRotation,
                    _targetRotation,
                    _cameraRotationSpeed * Time.deltaTime);
            }
        }

        private float NormalizeAngle(float angle)
        {
            float maxAngle = 360;
            float halfAngle = 180;

            angle %= maxAngle;

            if (angle > halfAngle)
                angle -= maxAngle;
            else if (angle < -halfAngle)
                angle += maxAngle;

            return angle;
        }

        private void OnDrawGizmos()
        {
            if (_camera == null || Application.isPlaying == false)
                return;

            float rayLength = 100f;
            float directionScale = 5f;
            float radius = .1f;

            Vector3 mousePosition = Input.mousePosition;
            Ray ray = _camera.ScreenPointToRay(mousePosition);

            Gizmos.color = Color.red;
            Gizmos.DrawRay(ray.origin, ray.direction * rayLength);

            Gizmos.color = Color.green;
            Vector3 forwardPoint = _camera.transform.position + _camera.transform.forward * directionScale;
            Gizmos.DrawLine(_camera.transform.position, forwardPoint);
            Gizmos.DrawSphere(forwardPoint, radius);
        }
    }

    [Serializable]
    public struct Range
    {
        [SerializeField] private float _min;
        [SerializeField] private float _max;

        public float Min => _min;

        public float Max
        {
            get
            {
                if (_max < _min)
                    _max = _min;

                return _max;
            }
        }
    }
}