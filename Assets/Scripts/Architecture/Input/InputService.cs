using Assets.Scripts.Architecture.Input.Events;
using Assets.Scripts.Architecture.SignalBus.Interfaces;
using Assets.Scripts.Gameplay.PlayerFeature.Components;
using UnityEngine;
using UnityEngine.InputSystem;

namespace Assets.Scripts.Architecture.Input
{
    public class InputService
    {
        private readonly InputDefault _input;
        private readonly ISignalBus<IEvent> _signalBus;

        private readonly JumpByInputEvent _jumpByInputEvent;
        private readonly AttackByInputEvent _attackByInputEvent;
        private readonly SwapWeaponByInputEvent _swapWeaponByInputEvent;

        public InputService(InputDefault input, ISignalBus<IEvent> signalBus)
        {
            _input = input;
            _signalBus = signalBus;

            _jumpByInputEvent = new();
            _attackByInputEvent = new();
            _swapWeaponByInputEvent = new();
        }

        public bool Enabled { get; private set; } = false;

        public void Enable()
        {
            if (Enabled)
                return;

            Enabled = true;
            _input.Enable();
            Subscribe();
        }

        public void Disable()
        {
            if (Enabled == false)
                return;

            Enabled = false;
            _input.Disable();
            Unsubscribe();
        }

        private void Subscribe()
        {
            _input.PcPlayer.Move.performed += OnMovePerformed;
            _input.PcPlayer.Jump.performed += OnJumpPerformed;
            _input.PcPlayer.Attack.performed += OnAttackPerformed;
            _input.PcPlayer.WeaponSwap.performed += OnWeaponSwapPerformed;
        }

        private void Unsubscribe()
        {
            _input.PcPlayer.Move.performed -= OnMovePerformed;
            _input.PcPlayer.Jump.performed -= OnJumpPerformed;
            _input.PcPlayer.Attack.performed -= OnAttackPerformed;
            _input.PcPlayer.WeaponSwap.performed -= OnWeaponSwapPerformed;
        }

        private void OnWeaponSwapPerformed(InputAction.CallbackContext context)
        {
            _signalBus.TryPublish(_swapWeaponByInputEvent);
        }

        private void OnAttackPerformed(InputAction.CallbackContext context)
        {
            _signalBus.TryPublish(_attackByInputEvent);
        }

        private void OnJumpPerformed(InputAction.CallbackContext context)
        {
            _signalBus.TryPublish(_jumpByInputEvent);
        }

        private void OnMovePerformed(InputAction.CallbackContext context)
        {
            Vector2 moveDirection = context.action.ReadValue<Vector2>();

            _signalBus.TryPublish(new MoveByInputEvent(moveDirection));
        }
    }
}