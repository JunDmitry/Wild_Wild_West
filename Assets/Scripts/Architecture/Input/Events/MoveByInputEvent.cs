using Assets.Scripts.Architecture.SignalBus.Interfaces;
using UnityEngine;

namespace Assets.Scripts.Architecture.Input.Events
{
    public class MoveByInputEvent : IEvent
    {
        public MoveByInputEvent(Vector2 direction)
        {
            Direction = direction;
        }

        public Vector2 Direction { get; } = Vector2.zero;
    }
}