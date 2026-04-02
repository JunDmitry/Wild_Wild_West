using Assets.Scripts.Architecture.Presentation.Interfaces;

namespace Assets.Scripts.Architecture.UI.Interfaces
{
    public interface IActiveModelView<in TData> : IModelView
    {
        void Bind(int id, IPresenter presenter);

        void UpdateView(TData data);
    }
}