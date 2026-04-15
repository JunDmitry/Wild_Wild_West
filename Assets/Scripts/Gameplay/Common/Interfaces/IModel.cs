using Assets.Scripts.Gameplay.Configs;

namespace Assets.Scripts.Gameplay.Common.Interfaces
{
    public interface IModel
    {
        int Id { get; }
    }

    public interface IModel<out TData> : IModel
        where TData : IModelData
    {
        TData Data { get; }
    }
}