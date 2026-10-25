using System;
using Game.Arena.Application.Ports;
using UnityEngine.SceneManagement;

namespace Game.Arena.Infrastructure.Scenes
{
    /// <summary>
    /// Unity adapter behind <see cref="IArenaSceneReloader"/>: loads the
    /// configured arena scene in single mode. Composition supplies the scene
    /// name at T-8; a missing name is a configuration error, rejected here
    /// rather than at the first defeat.
    /// </summary>
    public sealed class UnityArenaSceneReloader : IArenaSceneReloader
    {
        private readonly string _sceneName;

        public UnityArenaSceneReloader(string sceneName)
        {
            if (sceneName == null)
            {
                throw new ArgumentNullException(nameof(sceneName));
            }

            if (string.IsNullOrWhiteSpace(sceneName))
            {
                throw new ArgumentException(
                    "Scene name must not be empty or whitespace.",
                    nameof(sceneName));
            }

            _sceneName = sceneName;
        }

        public void ReloadArenaScene()
        {
            SceneManager.LoadScene(_sceneName, LoadSceneMode.Single);
        }
    }
}
