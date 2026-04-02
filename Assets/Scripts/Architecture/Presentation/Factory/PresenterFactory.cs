using Assets.Scripts.Architecture.Presentation.Interfaces;
using Assets.Scripts.Architecture.UI.Interfaces;
using Assets.Scripts.Gameplay.Common.Interfaces;
using Assets.Scripts.Gameplay.PlayerFeature.Components;
using System;
using System.Collections.Generic;

namespace Assets.Scripts.Architecture.Presentation.Factory
{
    public class PresenterFactory : IPresenterFactory
    {
        private readonly Dictionary<Type, Type> _presenterTypeByViewType;
        private readonly IResolver _resolver;

        public PresenterFactory(IResolver resolver)
        {
            _resolver = resolver;
            _presenterTypeByViewType = new();
        }

        public IPresenter CreatePresenterForActive<TData>(IModel model, IActiveModelView<TData> modelView)
        {
            Type presenterType = _presenterTypeByViewType[modelView.GetType()];
            IPresenter presenter = (IPresenter) _resolver.Instantiate(presenterType, model, modelView);
            
            modelView.Bind(model.Id, presenter);

            return presenter;
        }

        public IPresenter CreatePresenterForPassive<TData>(IModel model, IPassiveModelView<TData> modelView)
        {
            Type presenterType = _presenterTypeByViewType[modelView.GetType()];
            IPresenter presenter = (IPresenter)_resolver.Instantiate(presenterType, model, modelView);

            modelView.Bind(model.Id);

            return presenter;
        }
    }
}