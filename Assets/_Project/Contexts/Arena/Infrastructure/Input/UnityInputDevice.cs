using UnityEngine;

namespace Game.Arena.Infrastructure.Input
{
    public sealed class UnityInputDevice : IUnityInputDevice
    {
        public float GetAxisRaw(string axisName)
        {
            return UnityEngine.Input.GetAxisRaw(axisName);
        }

        public bool GetButtonDown(string buttonName)
        {
            return UnityEngine.Input.GetButtonDown(buttonName);
        }

        public bool GetKeyDown(KeyCode key)
        {
            return UnityEngine.Input.GetKeyDown(key);
        }
    }
}
