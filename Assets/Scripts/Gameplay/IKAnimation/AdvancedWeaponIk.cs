using UnityEngine;

namespace Assets.Scripts.Gameplay.IKAnimation
{
    public class AdvancedWeaponIk : MonoBehaviour
    {
        private const float TargetWeight = 1f;
        private const float WeightChangeSpeed = 5f;
        private const float WeightEpsilon = .01f;
        private const int MaxEulerValue = 180;
        private const int MaxAngle = 360;

        [SerializeField] private IkWeaponAimSettings _aimSettings;
        [SerializeField] private Animator _animator;
        [SerializeField] private Transform _cameraTransform;

#if UNITY_EDITOR

        [Header("Debug")]
        [SerializeField] private Color _debugRayColor = Color.red;

        [SerializeField] private bool _showDebugGizmos = true;
#endif

        private Vector3 _actualAimPoint;
        private float _currentIkWeight = 0f;

        private void Awake()
        {
            _animator = _animator != null ? _animator : GetComponent<Animator>();
        }

        private void Start()
        {
            if (_cameraTransform == null)
                _cameraTransform = Camera.main != null ? Camera.main.transform : null;
        }

        private void Update()
        {
            UpdateAimTarget();
            UpdateIkWeights();
        }

        private void OnAnimatorIK(int layerIndex)
        {
            if (_animator == null || _aimSettings.HoldPoint == null)
                return;

            float resultWeight = _currentIkWeight * _aimSettings.HandIkWeight;

            if (_aimSettings.AimTarget != null && resultWeight > WeightEpsilon)
            {
                CalculateHandIk(resultWeight);
                CalculateElbowIk(resultWeight * _aimSettings.ElbowIkWeight);
                CalculateBodyIk(resultWeight * _aimSettings.BodyIkWeight);
            }
        }

#if UNITY_EDITOR

        private void OnDrawGizmos()
        {
            if (_showDebugGizmos == false || Application.isPlaying == false)
                return;

            Gizmos.color = _debugRayColor;
            Gizmos.DrawLine(_cameraTransform.position, _actualAimPoint);

            Gizmos.color = Color.red;
            Gizmos.DrawSphere(_actualAimPoint, 0.1f);

            if (_aimSettings.HoldPoint != null)
            {
                Gizmos.color = Color.green;
                Gizmos.DrawSphere(_aimSettings.HoldPoint.position, 0.05f);
            }

            if (_aimSettings.Pivot != null)
            {
                Gizmos.color = Color.blue;
                Gizmos.DrawSphere(_aimSettings.Pivot.position, 0.07f);
            }
        }

        private void OnGUI()
        {
            if (_showDebugGizmos == false)
                return;

            GUI.Box(new Rect(10, 10, 250, 66), "IK Debug Info");
            GUI.Label(new Rect(20, 30, 230, 20), $"IK Weight: {_currentIkWeight:F2}");
            GUI.Label(new Rect(20, 50, 230, 20), $"Aim Point: {_actualAimPoint:F1}");
        }

#endif

        private void UpdateAimTarget()
        {
            if (_cameraTransform == null)
                return;

            Ray ray = new(_cameraTransform.position, _cameraTransform.forward);

            if (Physics.Raycast(ray, out RaycastHit hit, _aimSettings.MaxAimDistance, _aimSettings.AimLayerMask))
                _actualAimPoint = hit.point;
            else
                _actualAimPoint = ray.GetPoint(_aimSettings.MaxAimDistance);

            if (_aimSettings.AimTarget != null)
                _aimSettings.AimTarget.position = _actualAimPoint;
        }

        private void UpdateIkWeights()
        {
            _currentIkWeight = Mathf.MoveTowards(_currentIkWeight, TargetWeight, Time.deltaTime * WeightChangeSpeed);
        }

        private void CalculateHandIk(float resultWeight)
        {
            Vector3 aimHandPosition = _animator.GetIKPosition(AvatarIKGoal.RightHand);
            Quaternion aimHandRotation = _animator.GetIKRotation(AvatarIKGoal.RightHand);

            Vector3 targetHandPosition = CalculateTargetHandPosition();
            targetHandPosition += _aimSettings.HoldPoint.TransformDirection(_aimSettings.HandPositionOffset);
            Vector3 resultHandPosition = Vector3.Lerp(aimHandPosition, targetHandPosition, resultWeight);

            Quaternion targetHandRotation = CalculateTargetHandRotation(resultHandPosition);
            targetHandRotation *= Quaternion.Euler(_aimSettings.HandRotationOffset);
            Quaternion resultHandRotation = Quaternion.Lerp(aimHandRotation, targetHandRotation, resultWeight);
            resultHandRotation = ApplyAngleConstraints(resultHandRotation);

            _animator.SetIKPositionWeight(AvatarIKGoal.RightHand, resultWeight);
            _animator.SetIKRotationWeight(AvatarIKGoal.RightHand, resultWeight);
            _animator.SetIKPosition(AvatarIKGoal.RightHand, resultHandPosition);
            _animator.SetIKRotation(AvatarIKGoal.RightHand, resultHandRotation);
        }

        private void CalculateElbowIk(float resultWeight)
        {
            if (resultWeight < WeightEpsilon)
                return;

            Vector3 elbowPosition = _animator.GetIKHintPosition(AvatarIKHint.RightElbow);
            Vector3 targetElbowPosition = CalculateTargetElbowPosition();
            Vector3 finalElbowPosition = Vector3.Lerp(elbowPosition, targetElbowPosition, resultWeight);

            _animator.SetIKHintPositionWeight(AvatarIKHint.RightElbow, resultWeight);
            _animator.SetIKHintPosition(AvatarIKHint.RightElbow, finalElbowPosition);
        }

        private void CalculateBodyIk(float resultWeight)
        {
            if (resultWeight < WeightEpsilon)
                return;

            Vector3 lookDirection = _actualAimPoint - _aimSettings.Body.position;
            lookDirection.y = 0f;

            if (lookDirection.magnitude <= WeightEpsilon)
                return;

            Quaternion targetBodyRotation = Quaternion.LookRotation(lookDirection);
            Quaternion currentBodyRotation = _aimSettings.Body.rotation;
            Quaternion finalBodyRotation = Quaternion.Slerp(currentBodyRotation, targetBodyRotation, resultWeight);

            _animator.SetLookAtPosition(_actualAimPoint);
            _animator.SetLookAtWeight(resultWeight, .3f, .8f, .5f, .5f);
        }

        private Vector3 CalculateTargetHandPosition()
        {
            if (_aimSettings.Pivot == null)
                return _actualAimPoint;

            Vector3 direction = (_actualAimPoint - _aimSettings.Pivot.position).normalized;
            float distance = Vector3.Distance(_aimSettings.Pivot.position, _aimSettings.HoldPoint.position);
            Vector3 targetPosition = _aimSettings.Pivot.position + direction * distance;

            return targetPosition;
        }

        private Quaternion CalculateTargetHandRotation(Vector3 handPosition)
        {
            Vector3 toTarget = _actualAimPoint - handPosition;

            if (toTarget.magnitude < WeightEpsilon)
                return Quaternion.identity;

            Quaternion lookRotation = Quaternion.LookRotation(toTarget);
            Quaternion weaponAdjustment = Quaternion.identity;

            return lookRotation * weaponAdjustment;
        }

        private Quaternion ApplyAngleConstraints(Quaternion angle)
        {
            Vector3 euler = angle.eulerAngles;

            if (euler.x > MaxEulerValue)
                euler.x -= MaxAngle;

            if (euler.y > MaxEulerValue)
                euler.y -= MaxAngle;

            if (euler.z > MaxEulerValue)
                euler.z -= MaxAngle;

            euler.x = Mathf.Clamp(euler.x, _aimSettings.MinHandLowerAngle, _aimSettings.MaxHandRaiseAngle);

            return Quaternion.Euler(euler);
        }

        private Vector3 CalculateTargetElbowPosition()
        {
            if (_aimSettings.HoldPoint == null)
                return _animator.GetIKHintPosition(AvatarIKHint.RightElbow);

            Vector3 shoulderPosition = _aimSettings.Pivot.position;
            Vector3 handPosition = _aimSettings.HoldPoint.position;
            Vector3 aimDirection = (_actualAimPoint - shoulderPosition).normalized;

            Vector3 right = Vector3.Cross(aimDirection, Vector3.up).normalized;
            Vector3 elbowOffset = right * .3f + Vector3.down * .2f;

            return shoulderPosition + (handPosition - shoulderPosition) * .5f + elbowOffset;
        }
    }
}