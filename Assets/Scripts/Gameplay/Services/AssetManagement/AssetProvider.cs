using UnityEngine;

namespace Assets.Scripts.Gameplay.Services.AssetManagement
{
    public class AssetProvider : IAssetProvider
    {
        public GameObject LoadAsset(string assetPath)
        {
            return Resources.Load<GameObject>(assetPath);
        }

        public T LoadAsset<T>(string path)
            where T : Component
        {
            return Resources.Load<T>(path);
        }
    }
}