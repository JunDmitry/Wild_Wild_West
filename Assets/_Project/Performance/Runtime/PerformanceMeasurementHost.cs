using System;
using Game.CompositionRoot;
using UnityEngine;
using UnityEngine.SceneManagement;

namespace Game.Performance
{
    /// <summary>
    /// The measurement entry point (ADR-0034). It is inert unless the player is
    /// started with <c>-perfmeasure</c>: a normal build never creates a scope, a
    /// subscription, or a scene change. With the flag it creates the persistent
    /// bootstrap scope in code, with the measurement catalogue and without the
    /// bootstrap scene-load, then substitutes the scripted input source on the
    /// Arena scene's binder before its <c>Start</c>.
    /// </summary>
    public sealed class PerformanceMeasurementHost : MonoBehaviour
    {
        private const string MeasureFlag = "-perfmeasure";
        private const string OutputFlag = "-perfout";
        private const string CommitFlag = "-perfcommit";
        private const string AllocationStacksFlag = "-perfallocstacks";
        private const string ArenaSceneName = "ArenaScene";

        private ScriptedPlayerInputSource _scriptedInput;

        public void Initialize(ScriptedPlayerInputSource scriptedInput)
        {
            _scriptedInput = scriptedInput
                ?? throw new ArgumentNullException(nameof(scriptedInput));
        }

        [RuntimeInitializeOnLoadMethod(RuntimeInitializeLoadType.BeforeSceneLoad)]
        private static void Bootstrap()
        {
            string[] arguments = Environment.GetCommandLineArgs();

            if (HasFlag(arguments, MeasureFlag) == false)
            {
                return;
            }

            string outputPath = ReadValue(arguments, OutputFlag);

            if (string.IsNullOrWhiteSpace(outputPath))
            {
                Debug.LogError("[Performance] '-perfout <path>' is required.");
                Environment.Exit(1);
                return;
            }

            // TEMPORARY attribution support (t9-gc-residual-b): capture the
            // call stack of every allocation so the residual can be named in a
            // player run. Inert unless the flag is present.
            if (HasFlag(arguments, AllocationStacksFlag))
            {
                UnityEngine.Profiling.Profiler.enableAllocationCallstacks = true;
            }

            string commit = ReadValue(arguments, CommitFlag);

            GameObject host = new GameObject("PerformanceMeasurementHost");
            DontDestroyOnLoad(host);
            host.SetActive(false);

            ArenaGameConfiguration configuration = PerformanceMeasurementConfiguration.Create();

            BootstrapLifetimeScope bootstrap = host.AddComponent<BootstrapLifetimeScope>();
            bootstrap.Configuration = configuration;
            bootstrap.LoadArenaOnStart = false;

            ScriptedPlayerInputSource scriptedInput = new ScriptedPlayerInputSource();

            PerformanceProbe probe = host.AddComponent<PerformanceProbe>();
            probe.Initialize(outputPath, commit, scriptedInput, configuration);

            PerformanceMeasurementHost driver = host.AddComponent<PerformanceMeasurementHost>();
            driver.Initialize(scriptedInput);

            host.SetActive(true);
        }

        private void Awake()
        {
            SceneManager.sceneLoaded += OnSceneLoaded;
        }

        private void OnDestroy()
        {
            SceneManager.sceneLoaded -= OnSceneLoaded;
        }

        private void OnSceneLoaded(Scene scene, LoadSceneMode mode)
        {
            if (scene.name != ArenaSceneName)
            {
                return;
            }

            ArenaSceneBinder binder = FindObjectOfType<ArenaSceneBinder>();

            if (binder == null)
            {
                Debug.LogError("[Performance] The Arena scene has no ArenaSceneBinder.");
                return;
            }

            ArenaSceneObjects production = binder.CreateSceneObjects();

            binder.SceneObjectsOverride = new ArenaSceneObjects(
                production.Camera,
                production.Player,
                _scriptedInput,
                production.EnemyTransforms,
                production.Reloader);
        }

        private static bool HasFlag(string[] arguments, string flag)
        {
            for (int index = 0; index < arguments.Length; index++)
            {
                if (string.Equals(arguments[index], flag, StringComparison.OrdinalIgnoreCase))
                {
                    return true;
                }
            }

            return false;
        }

        private static string ReadValue(string[] arguments, string flag)
        {
            for (int index = 0; index < arguments.Length - 1; index++)
            {
                if (string.Equals(arguments[index], flag, StringComparison.OrdinalIgnoreCase))
                {
                    return arguments[index + 1];
                }
            }

            return null;
        }
    }
}
