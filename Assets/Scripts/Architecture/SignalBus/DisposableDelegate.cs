using System;

namespace Assets.Scripts.Gameplay.PlayerFeature.Components
{
    public class DisposableDelegate : IDisposable
    {
        private readonly Action _onDispose;

        private bool _disposed;

        public DisposableDelegate(Action onDispose)
        {
            ThrowIf.Null(onDispose, nameof(onDispose));

            _onDispose = onDispose;
        }

        public void Dispose()
        {
            if (_disposed)
                return;

            _disposed = true;
            _onDispose?.Invoke();
        }
    }
}