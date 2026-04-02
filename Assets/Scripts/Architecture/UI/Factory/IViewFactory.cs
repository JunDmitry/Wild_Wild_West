using Assets.Scripts.Architecture.UI.Interfaces;
using UnityEngine;

namespace Assets.Scripts.Architecture.UI.Factory
{
    public interface IViewFactory
    {
        TView CreateView<TView>(TView prefab, Vector3 position = default, Quaternion rotation = default, Transform parent = null)
            where TView : Component, IModelView;

        TView CreateView<TView>(string prefabPath, Vector3 position = default, Quaternion rotation = default, Transform parent = null)
            where TView : Component, IModelView;
    }
}