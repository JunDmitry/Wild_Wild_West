using Assets.Scripts.Architecture.Repository.Interfaces;
using Assets.Scripts.Gameplay.Common.Interfaces;
using Zenject;

namespace Assets.Scripts.Gameplay.PlayerFeature.Components
{
    public class ModelFactory<TModel, TData> : IModelFactory<TModel, TData>
        where TModel : class, IModel
    {
        private const int BufferSize = 2;

        private readonly DiContainer _container;
        private readonly IIdentifierService<int> _identifiers;
        private readonly IRepository<int, IModel> _repository;
        private readonly object[] _buffer;

        public ModelFactory(DiContainer container, IResolver resolver)
        {
            _container = container;
            _identifiers = resolver.Resolve<IIdentifierService<int>>();
            _repository = resolver.Resolve<IRepository<int, IModel>>();
            _buffer = new object[BufferSize];
        }

        public TModel CreateModel(TData data)
        {
            TModel model;

            lock (_buffer)
            {
                int id = _identifiers.GetNextId();

                _buffer[0] = id;
                _buffer[1] = data;

                model = _container.Instantiate<TModel>(_buffer);
                _repository.AddItem(id, model);
            }

            return model;
        }
    }
}