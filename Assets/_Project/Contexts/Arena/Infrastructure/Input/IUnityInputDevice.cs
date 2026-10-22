using UnityEngine;

namespace Game.Arena.Infrastructure.Input
{
    public interface IUnityInputDevice
    {
        float GetAxisRaw(string axisName);

        bool GetButtonDown(string buttonName);

        bool GetKeyDown(KeyCode key);
    }
}
