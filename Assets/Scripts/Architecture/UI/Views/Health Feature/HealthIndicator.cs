using Assets.Scripts.Architecture.UI.Interfaces;

namespace Assets.Scripts.Architecture.UI.Views.HealthFeature
{
    public abstract class HealthIndicator : View, IPassiveModelView<HealthViewData>
    {
        public int Id { get; private set; }

        public abstract void UpdateView(HealthViewData data);

        public void Show()
        {
            if (enabled)
                return;

            OnShow();
            enabled = true;
        }

        public void Hide()
        {
            if (enabled == false)
                return;

            OnHide();
            enabled = false;
        }

        public void Bind(int id)
        {
            if (Id == id)
                return;

            OnBind(id);
            Id = id;
        }

        protected abstract void OnShow();

        protected abstract void OnHide();

        protected virtual void OnBind(int id)
        { }
    }
}