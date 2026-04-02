using UnityEngine;

namespace Assets.Scripts.Gameplay.Services.AssetManagement
{
    public interface IAssetProvider
    {
        GameObject LoadAsset(string assetPath);
        T LoadAsset<T>(string path) where T : Component;
    }
}