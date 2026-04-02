using Assets.Scripts.Architecture.Presentation.Interfaces;
using Assets.Scripts.Architecture.UI.Interfaces;
using Assets.Scripts.Gameplay.Common.Interfaces;

namespace Assets.Scripts.Architecture.Presentation.Factory
{
    public interface IPresenterFactory
    {
        IPresenter CreatePresenterForPassive<TData>(IModel model, IPassiveModelView<TData> modelView);

        IPresenter CreatePresenterForActive<TData>(IModel model, IActiveModelView<TData> modelView);
    }
}