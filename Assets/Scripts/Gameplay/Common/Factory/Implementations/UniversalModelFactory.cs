using Assets.Scripts.Gameplay.Common.Interfaces;

namespace Assets.Scripts.Gameplay.PlayerFeature.Components
{
    public class UniversalModelFactory
    {
        private readonly IFactoryContainer _factoryContainer;

        public UniversalModelFactory(IFactoryContainer factoryContainer)
        {
            _factoryContainer = factoryContainer;
        }

        public TModel CreateModel<TModel, TData>(TData data)
            where TModel : class, IModel
        {
            IModelFactory<TModel, TData> modelFactory = _factoryContainer.GetFactory<ModelFactory<TModel, TData>>();

            return modelFactory.CreateModel(data);
        }
    }
}