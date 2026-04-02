namespace Assets.Scripts.Architecture.UI.Interfaces
{
    public interface IModelView
    {
        int Id { get; }

        void Show();

        void Hide();
    }
}