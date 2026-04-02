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

    public void Hide()
    {
        throw new System.NotImplementedException();
    }

    public void Show()
    {
        throw new System.NotImplementedException();
    }

    public void Dispose()
    {
    }

    protected virtual void Dispose(bool disposing)
    {
    }
}