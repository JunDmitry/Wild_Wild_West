using System;

namespace Assets.Scripts.Gameplay.PlayerFeature.Components
{
    public class VisibilityComponent : IVisibilityComponent
    {
        private bool _isVisible;

        public event Action<bool> Visibled;

        public int Id => throw new NotImplementedException();

        public void SetVisible()
        {
            if (_isVisible)
                return;

            _isVisible = true;
            Visibled?.Invoke(_isVisible);
        }

        public void SetHide()
        {
            if (_isVisible == false)
                return;

            _isVisible = false;
            Visibled?.Invoke(_isVisible);
        }
    }
}