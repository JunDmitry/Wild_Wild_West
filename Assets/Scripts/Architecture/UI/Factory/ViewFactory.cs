using Assets.Scripts.Architecture.UI.Interfaces;
using Assets.Scripts.Gameplay.Services.AssetManagement;
using UnityEngine;
using Zenject;

namespace Assets.Scripts.Architecture.UI.Factory
{
    public class ViewFactory : IViewFactory
    {
        private readonly DiContainer _container;
        private readonly IAssetProvider _assetProvider;

        public ViewFactory(DiContainer container, IAssetProvider assetProvider)
        {
            _container = container;
            _assetProvider = assetProvider;
        }

        public TView CreateView<TView>(string prefabPath, Vector3 position = default, Quaternion rotation = default, Transform parent = null)
            where TView : Component, IModelView
        {
            TView prefab = _assetProvider.LoadAsset<TView>(prefabPath);

            return CreateView(prefab, position, rotation, parent);
        }

        public TView CreateView<TView>(TView prefab, Vector3 position = default, Quaternion rotation = default, Transform parent = null)
            where TView : Component, IModelView
        {
            TView view = Object.Instantiate(prefab, position, rotation, parent);
            _container.Inject(view);

            return view;
        }
    }
}