using System;
using UnityEngine;

namespace Assets.Scripts.Gameplay.IKAnimation
{
    [Serializable]
    public class IkWeaponAimSettings
    {
        [Header("Main settings")]
        [SerializeField] private Transform _aimTarget;

        [SerializeField] private Transform _holdPoint;
        [SerializeField] private Transform _pivot;
        [SerializeField] private Transform _body;

        [Header("IK Weights")]
        [SerializeField, Range(0f, 1f)] private float _handIkWeight = .9f;

        [SerializeField, Range(0f, 1f)] private float _elbowIkWeight = .7f;
        [SerializeField, Range(0f, 1f)] private float _bodyIkWeight = .5f;

        [Header("Aim settings")]
        [SerializeField] private float _maxAimDistance = 100f;

        [SerializeField] private LayerMask _aimLayerMask = Physics.AllLayers;

        [Header("Offsets")]
        [SerializeField] private Vector3 _handPositionOffset;

        [SerializeField] private Vector3 _handRotationOffset;

        [Header("Constraints")]
        [SerializeField, Range(-180, 0)] private float _minHandLowerAngle = -30f;

        [SerializeField, Range(0, 180)] private float _maxHandRaiseAngle = 80f;

        public Transform AimTarget => _aimTarget;
        public Transform HoldPoint => _holdPoint;
        public Transform Pivot => _pivot;
        public Transform Body => _body;

        public float HandIkWeight => _handIkWeight;
        public float ElbowIkWeight => _elbowIkWeight;
        public float BodyIkWeight => _bodyIkWeight;

        public float MaxAimDistance => _maxAimDistance;
        public LayerMask AimLayerMask => _aimLayerMask;

        public Vector3 HandPositionOffset => _handPositionOffset;
        public Vector3 HandRotationOffset => _handRotationOffset;

        public float MinHandLowerAngle => _minHandLowerAngle;
        public float MaxHandRaiseAngle => _maxHandRaiseAngle;
    }
}