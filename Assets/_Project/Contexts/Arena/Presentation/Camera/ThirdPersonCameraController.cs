using UnityEngine;

namespace Game.Arena.Presentation.Camera
{
    public sealed class ThirdPersonCameraController : MonoBehaviour
    {
        private const string MouseHorizontalAxis = "Mouse X";
        private const string MouseVerticalAxis = "Mouse Y";

        [SerializeField] private Transform target;
        [SerializeField] private float distance = 6f;
        [SerializeField] private float height = 2f;
        [SerializeField] private float yawSensitivity = 3f;
        [SerializeField] private float pitchSensitivity = 3f;
        [SerializeField] private float minimumPitch = -10f;
        [SerializeField] private float maximumPitch = 60f;
        [SerializeField] private float initialPitch = 20f;

        private float _yaw;
        private float _pitch;

        public Transform Target
        {
            get => target;
            set => target = value;
        }

        public float Distance
        {
            get => distance;
            set => distance = value;
        }

        public float Height
        {
            get => height;
            set => height = value;
        }

        public float MinimumPitch => minimumPitch;

        public float MaximumPitch => maximumPitch;

        public float YawSensitivity => yawSensitivity;

        public float PitchSensitivity => pitchSensitivity;

        public float Yaw => _yaw;

        public float Pitch => _pitch;

        private void Awake()
        {
            _yaw = transform.eulerAngles.y;
            _pitch = Mathf.Clamp(initialPitch, minimumPitch, maximumPitch);
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
            _yaw += mouseX * yawSensitivity;
            _pitch = Mathf.Clamp(_pitch - (mouseY * pitchSensitivity), minimumPitch, maximumPitch);
        }

        public void Follow()
        {
            if (target == null)
            {
                return;
            }

            Quaternion rotation = Quaternion.Euler(_pitch, _yaw, 0f);
            Vector3 focus = target.position + (Vector3.up * height);
            Vector3 position = focus - ((rotation * Vector3.forward) * distance);

            transform.SetPositionAndRotation(position, rotation);
        }
    }
}
