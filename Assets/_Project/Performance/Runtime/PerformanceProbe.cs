using System;
using System.Collections.Generic;
using System.Globalization;
using System.IO;
using System.Text;
using Game.Arena.Application.Sessions;
using Game.Arena.Domain.Aggregates.ArenaRun;
using Game.Arena.Domain.Combat;
using Game.Arena.Domain.Identity;
using Game.Arena.Domain.Repositories;
using Game.CompositionRoot;
using Unity.Profiling;
using Unity.Profiling.LowLevel.Unsafe;
using UnityEngine;
using UnityEngine.SceneManagement;
using VContainer;
using ArenaRunAggregate = Game.Arena.Domain.Aggregates.ArenaRun.ArenaRun;

namespace Game.Performance
{
    /// <summary>
    /// Captures the five ADR-0033 metrics over a fixed warm-up and capture window
    /// and writes one JSON result (ADR-0034,
    /// docs/architect/migration/performance-measurement.md § Capture protocol).
    /// The capture loop is allocation-free: every buffer is sized at startup and no
    /// strings are built until the window closes.
    /// </summary>
    public sealed class PerformanceProbe : MonoBehaviour
    {
        private const double WarmupSeconds = 5d;
        private const double CaptureSeconds = 55d;
        private const int MaxFrameSamples = 20000;
        private const string GcAllocMarker = "GC.Alloc";
        private const string ArenaSceneName = "ArenaScene";
        private const string MarkerDumpFlag = "-perfmarkers";

        private static readonly string[] PhysicsQueryMarkers =
        {
            "Physics.SphereCastNonAlloc",
            "Physics.RaycastNonAlloc",
            "Physics.OverlapSphereNonAlloc",
        };

        private const double AverageFrameTimeLimitMs = 16.7d;
        private const double P99FrameTimeLimitMs = 33.3d;
        private const double MaxFrameTimeLimitMs = 50d;
        private const int ActiveEnemyLimit = 24;
        private const int PhysicsQueryLimit = 5;

        private string _outputPath;
        private string _commit;
        private ScriptedPlayerInputSource _scriptedInput;
        private int _playerHealth;
        private int _regularEnemyHealth;
        private int _waveEnemies;
        private double _spawnIntervalSeconds;

        private ProfilerRecorder _gcAllocRecorder;
        private ProfilerRecorder[] _physicsRecorders;
        private string _physicsCounterName;
        private FrameTiming[] _frameTimings;
        private float[] _frameTimeSamples;

        private ArenaSceneLifetimeScope _sceneScope;
        private ArenaRunSession _session;
        private IArenaRunRepository _repository;

        private int _arenaSceneLoads;
        private bool _initialized;
        private bool _finished;
        private bool _recordersDisposed;
        private bool _runObserved;
        private bool _capturing;
        private double _warmupStartTime;
        private double _captureStartTime;
        private double _captureElapsedSeconds;
        private double _reportedCaptureSeconds;
        private ArenaRunId _activeRunId;

        private int _sampleCount;
        private long _capturedFrames;
        private long _framesWithAllocation;
        private long _totalAllocatedBytes;
        private long _harnessAllocatedBytes;
        private int _peakActiveEnemies;
        private int _peakPhysicsQueries;

        public void Initialize(
            string outputPath,
            string commit,
            ScriptedPlayerInputSource scriptedInput,
            ArenaGameConfiguration configuration)
        {
            _outputPath = outputPath;
            _commit = string.IsNullOrWhiteSpace(commit) ? "unknown" : commit;
            _scriptedInput = scriptedInput ?? throw new ArgumentNullException(nameof(scriptedInput));

            if (configuration == null)
            {
                throw new ArgumentNullException(nameof(configuration));
            }

            _playerHealth = configuration.CreationParameters.PlayerDefinition.InitialHealth.Maximum;
            _regularEnemyHealth = configuration.CreationParameters.EnemyCatalog
                .Get(EnemyKind.Regular).InitialHealth.Maximum;
            _waveEnemies = configuration.CreationParameters.WaveCatalog
                .Get(WaveNumber.First).RegularEnemyCount;
            _spawnIntervalSeconds = configuration.SpawnInterval.Seconds;

            _initialized = true;
        }

        private void Awake()
        {
            if (_initialized == false)
            {
                Abort("PerformanceProbe was activated without Initialize.");
                return;
            }

            _frameTimings = new FrameTiming[1];
            _frameTimeSamples = new float[MaxFrameSamples];
            _physicsRecorders = Array.Empty<ProfilerRecorder>();

            _gcAllocRecorder = ProfilerRecorder.StartNew(
                ProfilerCategory.Memory,
                GcAllocMarker,
                1,
                ProfilerRecorderOptions.SumAllSamplesInFrame
                    | ProfilerRecorderOptions.WrapAroundWhenCapacityReached);

            if (_gcAllocRecorder.Valid == false)
            {
                Abort("The GC.Alloc marker recorder is unavailable.");
                return;
            }

            if (TryStartPhysicsRecorder() == false)
            {
                Abort("No physics-query counter was resolved through ProfilerRecorderHandle.GetAvailable.");
                return;
            }

            if (HasFlag(MarkerDumpFlag))
            {
                LogAvailableMarkers();
            }

            SceneManager.sceneLoaded += OnSceneLoaded;
        }

        private static bool HasFlag(string flag)
        {
            string[] arguments = Environment.GetCommandLineArgs();

            for (int index = 0; index < arguments.Length; index++)
            {
                if (string.Equals(arguments[index], flag, StringComparison.OrdinalIgnoreCase))
                {
                    return true;
                }
            }

            return false;
        }

        private static void LogAvailableMarkers()
        {
            List<ProfilerRecorderHandle> handles = new List<ProfilerRecorderHandle>();
            ProfilerRecorderHandle.GetAvailable(handles);

            for (int index = 0; index < handles.Count; index++)
            {
                ProfilerRecorderDescription description =
                    ProfilerRecorderHandle.GetDescription(handles[index]);

                if (string.IsNullOrEmpty(description.Name))
                {
                    continue;
                }

                if (description.Category != ProfilerCategory.Physics
                    && description.Name.IndexOf("query", StringComparison.OrdinalIgnoreCase) < 0)
                {
                    continue;
                }

                Debug.Log("[Performance] marker: " + description.Name
                    + " | category: " + description.Category.Name);
            }
        }

        private void OnDestroy()
        {
            SceneManager.sceneLoaded -= OnSceneLoaded;
            DisposeRecorders();
        }

        private void OnSceneLoaded(Scene scene, LoadSceneMode mode)
        {
            if (scene.name != ArenaSceneName)
            {
                return;
            }

            _arenaSceneLoads++;
        }

        private void Update()
        {
            if (_finished || _initialized == false)
            {
                return;
            }

            if (TryResolveSession() == false)
            {
                return;
            }

            if (_runObserved == false)
            {
                if (_session.HasActiveRun == false)
                {
                    return;
                }

                double observedAt = Time.unscaledTimeAsDouble;
                _runObserved = true;
                _activeRunId = _session.ActiveArenaRunId;
                _warmupStartTime = observedAt;
                _scriptedInput.StartRun(observedAt);
                return;
            }

            double current = Time.unscaledTimeAsDouble;

            if (_capturing == false)
            {
                if (current - _warmupStartTime < WarmupSeconds)
                {
                    return;
                }

                _capturing = true;
                _captureStartTime = current;

                // Warm the allocation query once so its first call is not
                // attributed to the capture window.
                GC.GetAllocatedBytesForCurrentThread();
                return;
            }

            long allocationMark = GC.GetAllocatedBytesForCurrentThread();
            bool live = TryGetLiveRun(out ArenaRunAggregate run);

            if (live)
            {
                CaptureFrame(run);
            }

            _harnessAllocatedBytes += GC.GetAllocatedBytesForCurrentThread() - allocationMark;

            if (live == false)
            {
                _captureElapsedSeconds = current - _captureStartTime;
                Finish(endedEarly: true);
                return;
            }

            _captureElapsedSeconds = current - _captureStartTime;

            if (_captureElapsedSeconds >= CaptureSeconds)
            {
                _reportedCaptureSeconds = CaptureSeconds;
                Finish(endedEarly: false);
            }
        }

        private bool TryResolveSession()
        {
            if (_session != null)
            {
                return true;
            }

            if (_sceneScope == null)
            {
                _sceneScope = FindObjectOfType<ArenaSceneLifetimeScope>();

                if (_sceneScope == null)
                {
                    return false;
                }
            }

            var container = _sceneScope.Container;

            if (container == null)
            {
                return false;
            }

            _session = container.Resolve<ArenaRunSession>();
            _repository = container.Resolve<IArenaRunRepository>();

            if (_repository == null)
            {
                Abort("The scene scope resolved no IArenaRunRepository.");
                return false;
            }

            return true;
        }

        private bool TryGetLiveRun(out ArenaRunAggregate run)
        {
            run = null;

            if (_arenaSceneLoads > 1)
            {
                return false;
            }

            if (_session.ActiveArenaRunId.Equals(_activeRunId) == false)
            {
                return false;
            }

            if (_repository.TryGet(_activeRunId, out run) == false)
            {
                return false;
            }

            return run.Status == ArenaRunStatus.Playing;
        }

        private void CaptureFrame(ArenaRunAggregate run)
        {
            FrameTimingManager.CaptureFrameTimings();

            if (FrameTimingManager.GetLatestTimings(1, _frameTimings) > 0)
            {
                double cpuFrameTime = _frameTimings[0].cpuFrameTime;

                if (cpuFrameTime > 0d && _sampleCount < _frameTimeSamples.Length)
                {
                    _frameTimeSamples[_sampleCount] = (float)cpuFrameTime;
                    _sampleCount++;
                }
            }

            _capturedFrames++;

            long allocatedBytes = _gcAllocRecorder.LastValue;

            if (allocatedBytes > 0)
            {
                _totalAllocatedBytes += allocatedBytes;
                _framesWithAllocation++;
            }

            long physicsQueries = 0;

            for (int index = 0; index < _physicsRecorders.Length; index++)
            {
                ProfilerRecorder recorder = _physicsRecorders[index];

                if (recorder.Valid == false || recorder.Count == 0)
                {
                    continue;
                }

                // Capacity is one sample per frame: the wrapped slot holds the
                // frame's aggregated sample, whose Count is the marker's call count.
                physicsQueries += recorder.GetSample(0).Count;
            }

            if (physicsQueries > _peakPhysicsQueries)
            {
                _peakPhysicsQueries = (int)physicsQueries;
            }

            int activeEnemies = run.ActiveEnemyCount;

            if (activeEnemies > _peakActiveEnemies)
            {
                _peakActiveEnemies = activeEnemies;
            }
        }

        private bool TryStartPhysicsRecorder()
        {
            List<ProfilerRecorderHandle> handles = new List<ProfilerRecorderHandle>();
            ProfilerRecorderHandle.GetAvailable(handles);

            ProfilerRecorderHandle[] resolved = new ProfilerRecorderHandle[PhysicsQueryMarkers.Length];
            bool[] present = new bool[PhysicsQueryMarkers.Length];

            for (int handleIndex = 0; handleIndex < handles.Count; handleIndex++)
            {
                ProfilerRecorderDescription description =
                    ProfilerRecorderHandle.GetDescription(handles[handleIndex]);

                if (description.Category != ProfilerCategory.Physics)
                {
                    continue;
                }

                if (string.IsNullOrEmpty(description.Name))
                {
                    continue;
                }

                for (int candidate = 0; candidate < PhysicsQueryMarkers.Length; candidate++)
                {
                    if (present[candidate])
                    {
                        continue;
                    }

                    if (string.Equals(
                            description.Name,
                            PhysicsQueryMarkers[candidate],
                            StringComparison.Ordinal))
                    {
                        resolved[candidate] = handles[handleIndex];
                        present[candidate] = true;
                    }
                }
            }

            List<ProfilerRecorder> recorders = new List<ProfilerRecorder>();
            List<string> names = new List<string>();

            for (int candidate = 0; candidate < PhysicsQueryMarkers.Length; candidate++)
            {
                if (present[candidate] == false)
                {
                    continue;
                }

                ProfilerRecorder recorder = new ProfilerRecorder(
                    resolved[candidate],
                    1,
                    ProfilerRecorderOptions.StartImmediately
                        | ProfilerRecorderOptions.SumAllSamplesInFrame
                        | ProfilerRecorderOptions.WrapAroundWhenCapacityReached);

                if (recorder.Valid == false)
                {
                    continue;
                }

                recorders.Add(recorder);
                names.Add(PhysicsQueryMarkers[candidate]);
            }

            if (recorders.Count == 0)
            {
                return false;
            }

            _physicsRecorders = recorders.ToArray();
            _physicsCounterName = string.Join(" + ", names);

            return true;
        }

        private void Finish(bool endedEarly)
        {
            if (_finished)
            {
                return;
            }

            if (endedEarly == false && _sampleCount == 0)
            {
                Abort("No frame-timing samples were captured.");
                return;
            }

            _finished = true;
            DisposeRecorders();
            SceneManager.sceneLoaded -= OnSceneLoaded;

            if (endedEarly)
            {
                _reportedCaptureSeconds = Math.Round(_captureElapsedSeconds, 3);
            }

            FrameStatistics statistics = SummarizeFrames();

            bool passAverage = endedEarly == false
                && statistics.AverageMs <= AverageFrameTimeLimitMs;
            bool passWorst = endedEarly == false
                && statistics.P99Ms <= P99FrameTimeLimitMs
                && statistics.MaxMs <= MaxFrameTimeLimitMs;
            bool passGcAlloc = endedEarly == false && _totalAllocatedBytes == 0;
            bool passActiveEnemies = endedEarly == false
                && _peakActiveEnemies <= ActiveEnemyLimit;
            bool passPhysicsQueries = endedEarly == false
                && _peakPhysicsQueries <= PhysicsQueryLimit;

            string json = BuildJson(
                statistics,
                passAverage,
                passWorst,
                passGcAlloc,
                passActiveEnemies,
                passPhysicsQueries,
                endedEarly);

            try
            {
                string directory = Path.GetDirectoryName(_outputPath);

                if (string.IsNullOrEmpty(directory) == false)
                {
                    Directory.CreateDirectory(directory);
                }

                File.WriteAllText(_outputPath, json);
                Debug.Log("[Performance] Result written to " + _outputPath);
                Debug.Log("[Performance] peaks: enemies=" + _peakActiveEnemies
                    + " physics=" + _peakPhysicsQueries
                    + " gcBytes=" + _totalAllocatedBytes
                    + " harnessBytes=" + _harnessAllocatedBytes
                    + " frames=" + _capturedFrames);

                if (endedEarly)
                {
                    Environment.Exit(2);
                }
                else
                {
                    Application.Quit();
                }
            }
            catch (Exception exception)
            {
                Debug.LogError("[Performance] Could not write the result file: " + exception);
                Environment.Exit(1);
            }
        }

        private void Abort(string reason)
        {
            if (_finished)
            {
                return;
            }

            _finished = true;
            DisposeRecorders();
            SceneManager.sceneLoaded -= OnSceneLoaded;
            Debug.LogError("[Performance] Measurement aborted: " + reason);
            Environment.Exit(1);
        }

        private void DisposeRecorders()
        {
            if (_recordersDisposed)
            {
                return;
            }

            _recordersDisposed = true;

            if (_gcAllocRecorder.Valid)
            {
                _gcAllocRecorder.Dispose();
            }

            if (_physicsRecorders != null)
            {
                for (int index = 0; index < _physicsRecorders.Length; index++)
                {
                    if (_physicsRecorders[index].Valid)
                    {
                        _physicsRecorders[index].Dispose();
                    }
                }
            }
        }

        private FrameStatistics SummarizeFrames()
        {
            int count = _sampleCount;
            Array.Sort(_frameTimeSamples, 0, count);

            double total = 0d;

            for (int index = 0; index < count; index++)
            {
                total += _frameTimeSamples[index];
            }

            double average = count > 0 ? total / count : 0d;
            double maximum = count > 0 ? _frameTimeSamples[count - 1] : 0d;
            int p99Index = count > 0
                ? Math.Min(count - 1, (int)Math.Ceiling(0.99d * count) - 1)
                : 0;
            double p99 = count > 0 ? _frameTimeSamples[p99Index] : 0d;

            return new FrameStatistics(average, p99, maximum);
        }

        private string BuildJson(
            FrameStatistics statistics,
            bool passAverage,
            bool passWorst,
            bool passGcAlloc,
            bool passActiveEnemies,
            bool passPhysicsQueries,
            bool endedEarly)
        {
            BuildInfo buildInfo = ReadBuildInfo();
            string commit = buildInfo != null ? buildInfo.commit : _commit;
            string unityVersion = buildInfo != null
                ? buildInfo.unityVersion
                : Application.unityVersion;
            string builtAtUtc = buildInfo != null ? buildInfo.builtAtUtc : "unknown";

            StringBuilder json = new StringBuilder(1024);
            json.Append('{');

            json.Append("\"build\":{");
            json.Append("\"commit\":").Append(Quote(commit)).Append(',');
            json.Append("\"unityVersion\":").Append(Quote(unityVersion)).Append(',');
            json.Append("\"builtAtUtc\":").Append(Quote(builtAtUtc));
            json.Append("},");

            json.Append("\"machine\":{");
            json.Append("\"os\":").Append(Quote(SystemInfo.operatingSystem)).Append(',');
            json.Append("\"cpu\":").Append(Quote(SystemInfo.processorType)).Append(',');
            json.Append("\"gpu\":").Append(Quote(SystemInfo.graphicsDeviceName)).Append(',');
            json.Append("\"memoryMb\":").Append(SystemInfo.systemMemorySize);
            json.Append("},");

            json.Append("\"profile\":{");
            json.Append("\"playerHealth\":").Append(_playerHealth).Append(',');
            json.Append("\"regularEnemyHealth\":").Append(_regularEnemyHealth).Append(',');
            json.Append("\"waveEnemies\":").Append(_waveEnemies).Append(',');
            json.Append("\"spawnIntervalSeconds\":")
                .Append(Number(_spawnIntervalSeconds, "0.###"));
            json.Append("},");

            json.Append("\"window\":{");
            json.Append("\"warmupSeconds\":").Append(Number(WarmupSeconds, "0")).Append(',');
            json.Append("\"captureSeconds\":")
                .Append(Number(_reportedCaptureSeconds, "0.###")).Append(',');
            json.Append("\"capturedFrames\":").Append(_capturedFrames).Append(',');
            json.Append("\"runEndedEarly\":").Append(endedEarly ? "true" : "false");
            json.Append("},");

            json.Append("\"frameTime\":{");
            json.Append("\"averageMs\":").Append(Number(statistics.AverageMs, "0.###")).Append(',');
            json.Append("\"p99Ms\":").Append(Number(statistics.P99Ms, "0.###")).Append(',');
            json.Append("\"maxMs\":").Append(Number(statistics.MaxMs, "0.###")).Append(',');
            json.Append("\"source\":\"FrameTimingManager.cpuFrameTime\"");
            json.Append("},");

            json.Append("\"gcAlloc\":{");
            json.Append("\"totalBytes\":").Append(_totalAllocatedBytes).Append(',');
            json.Append("\"framesWithAllocation\":").Append(_framesWithAllocation);
            json.Append("},");

            json.Append("\"activeEnemies\":{");
            json.Append("\"peak\":").Append(_peakActiveEnemies);
            json.Append("},");

            json.Append("\"physicsQueries\":{");
            json.Append("\"peakPerFrame\":").Append(_peakPhysicsQueries).Append(',');
            json.Append("\"counter\":").Append(Quote(_physicsCounterName));
            json.Append("},");

            json.Append("\"pass\":{");
            json.Append("\"averageFrameTime\":").Append(passAverage ? "true" : "false").Append(',');
            json.Append("\"worstFrameTime\":").Append(passWorst ? "true" : "false").Append(',');
            json.Append("\"gcAlloc\":").Append(passGcAlloc ? "true" : "false").Append(',');
            json.Append("\"activeEnemies\":").Append(passActiveEnemies ? "true" : "false").Append(',');
            json.Append("\"physicsQueries\":").Append(passPhysicsQueries ? "true" : "false");
            json.Append('}');

            json.Append('}');

            return json.ToString();
        }

        private static BuildInfo ReadBuildInfo()
        {
            try
            {
                string directory = Path.GetDirectoryName(Application.dataPath);

                if (string.IsNullOrEmpty(directory))
                {
                    return null;
                }

                string path = Path.Combine(directory, "build-info.json");

                if (File.Exists(path) == false)
                {
                    return null;
                }

                BuildInfo info = JsonUtility.FromJson<BuildInfo>(File.ReadAllText(path));

                return info == null || string.IsNullOrWhiteSpace(info.commit) ? null : info;
            }
            catch (Exception)
            {
                return null;
            }
        }

        private static string Number(double value, string format)
        {
            return value.ToString(format, CultureInfo.InvariantCulture);
        }

        private static string Quote(string value)
        {
            StringBuilder builder = new StringBuilder((value ?? string.Empty).Length + 2);
            builder.Append('"');

            foreach (char character in value ?? string.Empty)
            {
                switch (character)
                {
                    case '"':
                        builder.Append("\\\"");
                        break;
                    case '\\':
                        builder.Append("\\\\");
                        break;
                    case '\n':
                        builder.Append("\\n");
                        break;
                    case '\r':
                        builder.Append("\\r");
                        break;
                    case '\t':
                        builder.Append("\\t");
                        break;
                    default:
                        builder.Append(character);
                        break;
                }
            }

            builder.Append('"');

            return builder.ToString();
        }

        [Serializable]
        private sealed class BuildInfo
        {
            public string commit;
            public string unityVersion;
            public string builtAtUtc;
        }

        private readonly struct FrameStatistics
        {
            public FrameStatistics(double averageMs, double p99Ms, double maxMs)
            {
                AverageMs = averageMs;
                P99Ms = p99Ms;
                MaxMs = maxMs;
            }

            public double AverageMs { get; }
            public double P99Ms { get; }
            public double MaxMs { get; }
        }
    }
}
