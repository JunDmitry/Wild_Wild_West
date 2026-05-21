using Assets.Scripts.Architecture.Presentation.Interfaces;
using Assets.Scripts.Architecture.UI.Interfaces;
using Assets.Scripts.Gameplay.Common.Interfaces;
using System;

public abstract class Presenter<TModel, TView> : IPresenter, IDisposable
    where TModel : class, IModel
    where TView : class, IModelView
{
    private bool _isVisible;

    protected Presenter(TModel model, TView view)
    {
    }

    public void Disable()
    {
        throw new System.NotImplementedException();
    }

    public void Enable()
    {
        throw new System.NotImplementedException();
    }

    public void Dispose()
    {
    }

    protected virtual void Dispose(bool disposing)
    {
    }

    public void Bind(IModel model, IView view)
    {
        throw new NotImplementedException();
    }

    public void Initialize()
    {
        throw new NotImplementedException();
    }
}