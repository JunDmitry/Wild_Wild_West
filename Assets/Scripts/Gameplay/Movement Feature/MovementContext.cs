using System;
using UnityEngine;

namespace Assets.Scripts.Gameplay.Movement_Feature
{
    [Serializable]
    public class MovementContext
    {
        [SerializeField, Min(float.Epsilon)] private float _forwardSpeed = float.Epsilon;
        [SerializeField, Min(float.Epsilon)] private float _strafeSpeed = float.Epsilon;
        [SerializeField, Min(float.Epsilon)] private float _backwardSpeed = float.Epsilon;

        public float ForwardSpeed => _forwardSpeed;
        public float StrafeSpeed => _strafeSpeed;
        public float BackwardSpeed => _backwardSpeed;
    }

    [SerializeField]
    public class JumpContext
    {
        [SerializeField] private AnimationCurve _jumpCurve;
        [SerializeField] private float _jumpSpeed;
        [SerializeField] private float _jumpDuration;

        public AnimationCurve JumpCurve => _jumpCurve;
        public float JumpSpeed => _jumpSpeed;
        public float JumpDuration => _jumpDuration;
    }
}