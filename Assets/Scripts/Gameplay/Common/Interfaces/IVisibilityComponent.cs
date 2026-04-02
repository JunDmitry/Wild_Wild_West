using Assets.Scripts.Gameplay.Common.Interfaces;
using System;

namespace Assets.Scripts.Gameplay.PlayerFeature.Components
{
    public interface IVisibilityComponent : IModel
    {
        event Action<bool> Visibled;

        void SetHide();

        void SetVisible();
    }
}