using System;
using UnityEngine;
using Zenject;

namespace Assets.Scripts.Gameplay.Services.UpdateService
{
    public class GameLoop : MonoBehaviour
    {
        private IUpdaterService _updaterService;

        private void Update()
        {
            _updaterService?.Update(Time.deltaTime);
        }

        private void FixedUpdate()
        {
            _updaterService.FixedUpdate(Time.fixedDeltaTime);
        }

        private void LateUpdate()
        {
            _updaterService.LateUpdate(Time.deltaTime);
        }

        private void OnDestroy()
        {
            if (_updaterService is IDisposable disposable)
                disposable.Dispose();
        }

        [Inject]
        private void Construct(IUpdaterService updaterService)
        {
            _updaterService = updaterService;
        }
    }
}