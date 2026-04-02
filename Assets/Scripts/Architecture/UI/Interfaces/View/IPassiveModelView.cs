namespace Assets.Scripts.Architecture.UI.Interfaces
{
    public interface IPassiveModelView<in TData> : IModelView
    {
        void Bind(int id);
        void UpdateView(TData data);
    }
}