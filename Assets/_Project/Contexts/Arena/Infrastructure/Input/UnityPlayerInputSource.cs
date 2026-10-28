using System;
using Game.Arena.Application.Input;
using Game.Arena.Application.Ports;
using Game.Arena.Domain.Geometry;
using UnityEngine;

namespace Game.Arena.Infrastructure.Input
{
    public sealed class UnityPlayerInputSource : MonoBehaviour, IPlayerInputSource
    {
        private const string HorizontalAxis = "Horizontal";
        private const string VerticalAxis = "Vertical";
        private const string AttackButton = "Fire1";

        [SerializeField] private Transform _viewTransform;
        [SerializeField] private KeyCode _switchWeaponKey = KeyCode.Q;

        private IUnityInputDevice _device;
        private Direction3D _lastViewDirection = Direction3D.Forward;
        private readonly PlayerFrameInput _frameInput = new();

        public Transform ViewTransform
        {
            get => _viewTransform;
            set => _viewTransform = value;
        }

        public KeyCode SwitchWeaponKey
        {
            get => _switchWeaponKey;
            set => _switchWeaponKey = value;
        }

        public IUnityInputDevice Device
        {
            get => _device ?? (_device = new UnityInputDevice());
            set => _device = value ?? throw new ArgumentNullException(nameof(value));
        }

        public PlayerFrameInput Read()
        {
            IUnityInputDevice device = Device;

            MovementAxes movementAxes = new(
                device.GetAxisRaw(VerticalAxis),
                device.GetAxisRaw(HorizontalAxis));

            _frameInput.Fill(
                movementAxes,
                ReadViewDirection(),
                device.GetButtonDown(AttackButton),
                device.GetKeyDown(_switchWeaponKey));

            return _frameInput;
        }

        private Direction3D ReadViewDirection()
        {
            Transform view = _viewTransform != null
                ? _viewTransform
                : Camera.main != null ? Camera.main.transform : null;

            if (view != null)
            {
                Vector3 forward = view.forward;

                if (Direction3D.TryFrom(
                        new Displacement3D(forward.x, 0f, forward.z),
                        out Direction3D viewDirection))
                {
                    _lastViewDirection = viewDirection;
                }
            }

            return _lastViewDirection;
        }
    }
}
