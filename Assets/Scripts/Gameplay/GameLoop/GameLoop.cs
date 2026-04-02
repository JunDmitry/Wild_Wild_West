using System;
using UnityEngine;

namespace Assets.Scripts.Gameplay.Services.UpdateService
{
    public class GameLoop : MonoBehaviour
    {
        private IUpdaterService _updaterService;

        private void Update()
        {
            _updaterService?.Update(Time.deltaTime);
        }

        private void OnDestroy()
        {
            if (_updaterService is IDisposable disposable)
                disposable.Dispose();
        }
    }
}