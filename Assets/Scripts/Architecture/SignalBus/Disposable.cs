using System;

namespace Assets.Scripts.Gameplay.PlayerFeature.Components
{
    public class Disposable : IDisposable
    {
        private readonly Action _onDispose;

        private bool _disposed;

        public Disposable(Action onDispose)
        {
            ThrowIf.Null(onDispose, nameof(onDispose));

            _onDispose = onDispose;
        }

        public static IDisposable Create(Action onDispose)
        {
            return new Disposable(onDispose);
        }

        public static IDisposable Combine(params IDisposable[] disposables)
        {
            return new Disposable(() =>
            {
                foreach (IDisposable disposable in disposables)
                    disposable.Dispose();
            });
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