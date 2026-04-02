using Assets.Scripts.Gameplay.Common.Interfaces;

namespace Assets.Scripts.Gameplay.PlayerFeature.Components
{
    public interface IModelFactory<TModel, TData> : IFactory
        where TModel : class, IModel
    {
        TModel CreateModel(TData data);
    }
}