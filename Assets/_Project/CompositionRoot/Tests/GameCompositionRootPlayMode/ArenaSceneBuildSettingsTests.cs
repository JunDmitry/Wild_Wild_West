using System.Collections;
using System.Collections.Generic;
using NUnit.Framework;
using UnityEngine;
using UnityEngine.SceneManagement;
using UnityEngine.TestTools;

namespace Game.CompositionRoot.PlayMode.Tests
{
    /// <summary>
    /// Proves the host wiring ADR-0032 depends on: both arena scenes are in the
    /// build settings with the bootstrap first, and a bootstrap scope whose load
    /// flag is off never performs a scene load.
    /// </summary>
    [TestFixture]
    public sealed class ArenaSceneBuildSettingsTests
    {
        private const string BootstrapScenePath = "Assets/_Project/Scenes/BootstrapScene.unity";
        private const string ArenaScenePath = "Assets/_Project/Scenes/ArenaScene.unity";
        private const string ArenaSceneName = "ArenaScene";

        private readonly List<GameObject> _objects = new();

        [TearDown]
        public void TearDown()
        {
            for (int index = 0; index < _objects.Count; index++)
            {
                UnityEngine.Object.DestroyImmediate(_objects[index]);
            }

            _objects.Clear();
        }

        [Test]
        public void BuildSettings_RegisterBothScenes_BootstrapFirst()
        {
            Assert.That(
                SceneUtility.GetBuildIndexByScenePath(BootstrapScenePath),
                Is.EqualTo(0),
                "The bootstrap scene must be the first build-settings entry.");

            Assert.That(
                SceneUtility.GetBuildIndexByScenePath(ArenaScenePath),
                Is.EqualTo(1),
                "The arena scene must follow the bootstrap entry.");

            // The test runner adds its own InitTestScene to the build settings
            // for the duration of a PlayMode run; no other scene belongs there.
            int sceneCount = SceneManager.sceneCountInBuildSettings;

            for (int index = 2; index < sceneCount; index++)
            {
                Assert.That(
                    SceneUtility.GetScenePathByBuildIndex(index),
                    Does.Contain("InitTestScene"),
                    "Only the test runner's own scene may follow the two arena scenes.");
            }
        }

        [UnityTest]
        public IEnumerator BootstrapScope_WithLoadOff_DoesNotLoadTheArenaScene()
        {
            GameObject bootstrapObject = new("Bootstrap Scope");
            _objects.Add(bootstrapObject);

            // Awake does not run on an inactive object, which is the only
            // window in which the flag can be turned off before Build.
            bootstrapObject.SetActive(false);
            BootstrapLifetimeScope bootstrap =
                bootstrapObject.AddComponent<BootstrapLifetimeScope>();
            bootstrap.LoadArenaOnStart = false;
            bootstrapObject.SetActive(true);

            yield return null;

            Assert.That(bootstrap.Container, Is.Not.Null);
            Assert.That(bootstrap.LoadArenaOnStart, Is.False);
            Assert.That(
                SceneManager.GetSceneByName(ArenaSceneName).isLoaded,
                Is.False,
                "A bootstrap scope with LoadArenaOnStart off must never load the Arena scene.");
        }
    }
}
