namespace Assets.Scripts.Gameplay.PlayerFeature.Components
{
    public interface IIdentifierService<T> : IService
    {
        T GetNextId();
    }
}