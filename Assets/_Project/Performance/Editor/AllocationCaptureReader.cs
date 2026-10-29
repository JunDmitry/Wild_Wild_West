using System;
using System.Collections.Generic;
using System.Globalization;
using System.IO;
using System.Text;
using UnityEditor;
using UnityEditor.Profiling;
using UnityEditorInternal;
using UnityEngine;

namespace Game.Performance.Editor
{
    /// <summary>
    /// TEMPORARY attribution tool for coder task t9-gc-residual-b. Loads a
    /// player profiler capture written with <c>-profiler-enable
    /// -profiler-log-file</c> and sums the GC.Alloc samples per call-stack
    /// path, so each per-frame allocation site can be named from player
    /// evidence. Deleted before the acceptance runs.
    /// </summary>
    public static class AllocationCaptureReader
    {
        private const string CaptureFlag = "-perfcapture";
        private const string OutputFlag = "-perfcaptureout";
        private const int TopCount = 60;

        public static void Dump()
        {
            int exitCode = 1;

            try
            {
                exitCode = DumpCore();
            }
            catch (Exception exception)
            {
                Debug.LogError("[Performance] Capture reader threw: " + exception);
            }

            if (Application.isBatchMode)
            {
                EditorApplication.Exit(exitCode);
            }
        }

        private static int DumpCore()
        {
            string capture = ReadArgument(CaptureFlag);
            string output = ReadArgument(OutputFlag);

            if (string.IsNullOrWhiteSpace(capture) || File.Exists(capture) == false)
            {
                Debug.LogError("[Performance] '-perfcapture <path>' must name an existing capture.");
                return 2;
            }

            if (string.IsNullOrWhiteSpace(output))
            {
                output = Path.Combine(Path.GetDirectoryName(capture) ?? ".", "alloc-stacks.txt");
            }

            ProfilerDriver.LoadProfile(capture, false);

            int first = ProfilerDriver.firstFrameIndex;
            int last = ProfilerDriver.lastFrameIndex;
            Dictionary<string, double> bytesByPath = new(StringComparer.Ordinal);
            Dictionary<string, int> framesByPath = new(StringComparer.Ordinal);
            Dictionary<string, int> samplesByPath = new(StringComparer.Ordinal);

            for (int frame = first; frame <= last; frame++)
            {
                HierarchyFrameDataView view;

                try
                {
                    view = ProfilerDriver.GetHierarchyFrameDataView(
                        frame,
                        0,
                        HierarchyFrameDataView.ViewModes.MergeSamplesWithTheSameName,
                        0,
                        false);
                }
                catch (Exception)
                {
                    continue;
                }

                if (view == null)
                {
                    continue;
                }

                int itemCount = view.GetItemCount();

                for (int id = 0; id < itemCount; id++)
                {
                    float gcBytes;

                    try
                    {
                        gcBytes = view.GetItemColumnDataAsFloat(id, HierarchyFrameDataView.columnGcMemory);
                    }
                    catch (Exception)
                    {
                        continue;
                    }

                    if (gcBytes <= 0f)
                    {
                        continue;
                    }

                    string path = view.GetItemPath(id);

                    if (string.IsNullOrEmpty(path))
                    {
                        path = view.GetItemName(id) ?? "(unnamed)";
                    }

                    bytesByPath[path] = bytesByPath.TryGetValue(path, out double total)
                        ? total + gcBytes
                        : gcBytes;
                    framesByPath[path] = framesByPath.TryGetValue(path, out int frames)
                        ? frames + 1
                        : 1;
                    samplesByPath[path] = samplesByPath.TryGetValue(path, out int samples)
                        ? samples + 1
                        : 1;
                }
            }

            List<KeyValuePair<string, double>> ranked = new(bytesByPath);
            ranked.Sort((left, right) => right.Value.CompareTo(left.Value));

            StringBuilder builder = new();
            builder.AppendLine("frames\t" + first + "\t" + last);
            builder.AppendLine("totalBytes\t" + Sum(ranked).ToString("F0", CultureInfo.InvariantCulture));
            builder.AppendLine("bytes\tframes\tsamples\tpath");

            for (int index = 0; index < ranked.Count && index < TopCount; index++)
            {
                string path = ranked[index].Key;

                builder.AppendLine(
                    ranked[index].Value.ToString("F0", CultureInfo.InvariantCulture)
                    + "\t" + framesByPath[path]
                    + "\t" + samplesByPath[path]
                    + "\t" + path);
            }

            File.WriteAllText(output, builder.ToString());
            Debug.Log("[Performance] Allocation stacks written to " + output);
            return 0;
        }

        private static double Sum(List<KeyValuePair<string, double>> ranked)
        {
            double total = 0d;

            for (int index = 0; index < ranked.Count; index++)
            {
                total += ranked[index].Value;
            }

            return total;
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
    }
}
