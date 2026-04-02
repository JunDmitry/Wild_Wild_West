using System.Collections.Generic;

namespace Assets.Scripts.Gameplay.StateMachine
{
    public interface IStateChanger
    {
        void ChangeState(State state);
    }

    public class StateMachine : IStateChanger
    {
        private State _currentState;

        public void ChangeState(State state)
        {
            _currentState?.Exit();
            _currentState = state;
            _currentState?.Enter();
        }

        public void Update()
        {
            _currentState?.Update();
        }
    }

    public abstract class State
    {
        private readonly IStateChanger _stateChanger;
        private readonly List<Transition> _transitions;

        public State(IStateChanger stateChanger)
        {
            _transitions = new();
            _stateChanger = stateChanger;
        }

        public void AddTransition(Transition transition)
        {
            ThrowIf.Null(transition, nameof(transition));
            ThrowIf.Invalid(_transitions.Contains(transition), $"Transition already added in State");

            _transitions.Add(transition);
        }

        public virtual void Enter()
        {
        }

        public void Update()
        {
            foreach (Transition transition in _transitions)
            {
                if (transition.TryTransit(out State nextState) == false)
                    continue;

                _stateChanger.ChangeState(nextState);

                return;
            }

            Tick();
        }

        public virtual void Exit()
        {
        }

        protected virtual void Tick()
        { }
    }

    public abstract class Transition
    {
        private State _nextState;

        public Transition(State nextState)
        {
            _nextState = nextState;
        }

        public bool TryTransit(out State nextState)
        {
            nextState = _nextState;

            return CanTransit();
        }

        protected abstract bool CanTransit();
    }

    // Player.SwapWeapon <- WeaponSwapper(subscribe for swap weapon input) -> Animator.Controller = swappedWeaponController
    // Player.Jump(Jumper) <- PlayerController(ToJumpTransition / JumpState) -> Animator.SetJump()
    // Player.Attack(Attacker) <- PlayerController(ToAttackTransition / AttackState) -> Animator.SetAttack()
    // Player.Move(Mover) <- PlayerController(ToMoveTransition / MoveState) -> Animator.SetDirection()
}