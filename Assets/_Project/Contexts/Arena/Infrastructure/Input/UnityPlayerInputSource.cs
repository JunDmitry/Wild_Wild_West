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

        [SerializeField] private Transform viewTransform;
        [SerializeField] private KeyCode switchWeaponKey = KeyCode.Q;

        private IUnityInputDevice _device;
        private Direction3D _lastViewDirection = Direction3D.Forward;

        public Transform ViewTransform
        {
            get => viewTransform;
            set => viewTransform = value;
        }

        public KeyCode SwitchWeaponKey
        {
            get => switchWeaponKey;
            set => switchWeaponKey = value;
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

            return new PlayerFrameInput(
                movementAxes,
                ReadViewDirection(),
                device.GetButtonDown(AttackButton),
                device.GetKeyDown(switchWeaponKey));
        }

        private Direction3D ReadViewDirection()
        {
            Transform view = viewTransform != null
                ? viewTransform
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
