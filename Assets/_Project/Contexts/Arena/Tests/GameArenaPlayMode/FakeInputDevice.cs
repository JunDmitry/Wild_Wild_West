using System.Collections.Generic;
using Game.Arena.Infrastructure.Input;
using UnityEngine;

namespace Game.Arena.PlayMode.Tests
{
    internal sealed class FakeInputDevice : IUnityInputDevice
    {
        public Dictionary<string, float> Axes { get; } = new();

        public HashSet<string> ButtonsDown { get; } = new();

        public HashSet<KeyCode> KeysDown { get; } = new();

        public float GetAxisRaw(string axisName)
        {
            return Axes.TryGetValue(axisName, out float value) ? value : 0f;
        }

        public bool GetButtonDown(string buttonName)
        {
            return ButtonsDown.Contains(buttonName);
        }

        public bool GetKeyDown(KeyCode key)
        {
            return KeysDown.Contains(key);
        }
    }
}
