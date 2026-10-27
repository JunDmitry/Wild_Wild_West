using System;
using System.Globalization;
using System.IO;
using UnityEditor;
using UnityEditor.Build.Reporting;
using UnityEngine;

namespace Game.Performance.Editor
{
    /// <summary>
    /// The <c>-executeMethod</c> entry point that builds the measurement player
    /// (ADR-0034, docs/architect/migration/performance-measurement.md § Build and
    /// run). It enables <c>PERF_MEASUREMENT</c> for the Standalone group only for
    /// the duration of the build and restores the previous defines afterwards.
    /// </summary>
    public static class PerformancePlayerBuilder
    {
        private const string MeasureDefine = "PERF_MEASUREMENT";
        private const string ArenaScenePath = "Assets/_Project/Scenes/ArenaScene.unity";
        private const string OutputDirectory = "Builds/Performance";
        private const string PlayerName = "WildWildWest-Performance.exe";
        private const string BuildInfoName = "build-info.json";
        private const string CommitFlag = "-perfcommit";

        public static void Build()
        {
            int exitCode = 1;
            UnityEditor.Build.NamedBuildTarget targetGroup = UnityEditor.Build.NamedBuildTarget.Standalone;
            string previousDefines = PlayerSettings.GetScriptingDefineSymbols(UnityEditor.Build.NamedBuildTarget.Standalone);

            try
            {
                PlayerSettings.SetScriptingDefineSymbols(
                    targetGroup,
                    AppendDefine(previousDefines, MeasureDefine));

                string outputDirectory = Path.GetFullPath(OutputDirectory);
                Directory.CreateDirectory(outputDirectory);

                BuildPlayerOptions options = new()
                {
                    scenes = new[] { ArenaScenePath },
                    locationPathName = Path.Combine(outputDirectory, PlayerName),
                    target = BuildTarget.StandaloneWindows64,
                    targetGroup = targetGroup.ToBuildTargetGroup(),
                    options = BuildOptions.Development,
                    extraScriptingDefines = new[] { MeasureDefine },
                };

                BuildReport report = BuildPipeline.BuildPlayer(options);

                if (report.summary.result != BuildResult.Succeeded)
                {
                    Debug.LogError("[Performance] Player build failed: " + report.summary.result);
                }
                else
                {
                    WriteBuildInfo(outputDirectory, ReadArgument(CommitFlag) ?? "unknown");
                    Debug.Log("[Performance] Player built at " + options.locationPathName);
                    exitCode = 0;
                }
            }
            catch (Exception exception)
            {
                Debug.LogError("[Performance] Player build threw: " + exception);
            }
            finally
            {
                PlayerSettings.SetScriptingDefineSymbols(targetGroup, previousDefines);
            }

            if (Application.isBatchMode)
            {
                EditorApplication.Exit(exitCode);
            }
        }

        private static void WriteBuildInfo(string outputDirectory, string commit)
        {
            string json = "{"
                + "\"commit\":" + Quote(commit) + ","
                + "\"unityVersion\":" + Quote(Application.unityVersion) + ","
                + "\"builtAtUtc\":" + Quote(DateTime.UtcNow.ToString(
                    "yyyy-MM-ddTHH:mm:ssZ",
                    CultureInfo.InvariantCulture))
                + "}";

            File.WriteAllText(Path.Combine(outputDirectory, BuildInfoName), json);
        }

        private static string AppendDefine(string defines, string define)
        {
            if (string.IsNullOrWhiteSpace(defines))
            {
                return define;
            }

            foreach (string existing in defines.Split(';'))
            {
                if (string.Equals(existing.Trim(), define, StringComparison.Ordinal))
                {
                    return defines;
                }
            }

            return defines + ";" + define;
        }

        private static string ReadArgument(string flag)
        {
            string[] arguments = Environment.GetCommandLineArgs();

            for (int index = 0; index < arguments.Length - 1; index++)
            {
                if (string.Equals(arguments[index], flag, StringComparison.OrdinalIgnoreCase))
                {
                    return arguments[index + 1];
                }
            }

            return null;
        }

        private static string Quote(string value)
        {
            return "\"" + (value ?? string.Empty).Replace("\\", "\\\\").Replace("\"", "\\\"") + "\"";
        }
    }
}
