using System;
using Assets.Scripts.Architecture.Lifecycle;
using Assets.Scripts.Gameplay.Common.Interfaces;

namespace Assets.Scripts.Architecture.Presentation.Interfaces
{
    public interface IPresenter : IInitializable, IDisposable
    {
        void Bind(IModel model, IView view);
        void Enable();
        void Disable();
    }
}