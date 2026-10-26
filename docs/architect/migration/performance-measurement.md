# Performance measurement for the production cut-over

- **Authority:** [ADR-0033](../adr/ADR-0033-performance-thresholds-for-cut-over.md) fixes the thresholds and the player-build procedure; [ADR-0034](../adr/ADR-0034-automated-performance-measurement.md) fixes the harness and the measurement profile.
- **Status:** measured 2026-10-27 on build `38f856e` (two clean runs); 3 of 5 thresholds pass, GC allocation and physics queries fail. T-9 remains blocked.
- **Owner:** `architect-role`; the harness is implemented by `coder-role`.

## Scope

The measurement runs the **target** Arena stack in a Windows player build. The 10 % legacy comparison is recorded **not applicable**: no scene in the repository references any `Game.Core.*` script (every scene's `m_Script` GUIDs were mapped to `Assets/_Project/Core/**`, zero hits), so no legacy gameplay scene is runnable. The absolute thresholds stand alone, as ADR-0033's not-applicable clause allows.

## Metrics and pass criteria

| # | Metric | Threshold | Capture |
|---|---|---|---|
| 1 | Average frame time | ≤ 16.7 ms | `FrameTimingManager` `cpuFrameTime`, mean over the capture window |
| 2 | Worst frame time | P99 ≤ 33.3 ms and max ≤ 50 ms | Same samples, sorted after capture |
| 3 | Managed allocation per frame | 0 B after warm-up | `ProfilerRecorder` marker `GC.Alloc`, sum over the capture window |
| 4 | Maximum active enemies | ≤ 24 | `ArenaRun.ActiveEnemyCount` polled every capture frame; peak |
| 5 | Physics queries per frame | ≤ 5 | Sum of the Physics-category markers `Physics.SphereCastNonAlloc`, `Physics.RaycastNonAlloc`, and `Physics.OverlapSphereNonAlloc`; peak per frame |

Every metric must hold in **both** runs. A failed metric is an optimisation task, not a waiver.

No single `Physics.Query` counter is exposed in a 2022.3.46f1 player, so the counter ADR-0033 names is resolved as the sum of the three query markers the game actually calls. The ≤ 5 threshold is unchanged.

## Measurement profile

The shipped `ArenaGameConfiguration.CreateDefault()` is used unchanged except for two health values:

| Item | Shipped | Measurement |
|---|---|---|
| Arena, bounds, spawn radius | shipped | unchanged |
| Player position, speed, radius | shipped | unchanged |
| Player health | 100 | **1 000 000** |
| Weapon catalogue | shipped | unchanged |
| Regular enemy definition | shipped | health **1 000 000**, every other field unchanged |
| Boss enemy definition | shipped | health **1 000 001** — `EnemyCatalog` requires the boss to strictly exceed every regular enemy; the value is unused by this profile, every other field unchanged |
| Waves | one wave, 10 regular | unchanged |
| Spawn interval | 2 s | unchanged |
| Restart delay | 3 s | unchanged |

Raised health prevents a defeat or a victory inside the 60 s window while leaving the per-frame load (movement, targeting, attacks, damage application, rendering, physics queries) unchanged. Enemy deaths and the victory/defeat windows are therefore not exercised by this measurement; they are covered functionally by the T-8 suites.

## Harness shape

- `Game.Performance` — runtime assembly, `defineConstraints: ["PERF_MEASUREMENT"]`, leaf above `Game.CompositionRoot`; compiled only into the measurement player.
- `Game.Performance.Editor` — Editor-only assembly with the `-executeMethod` build entry point.
- `PerformanceMeasurementHost` — `[RuntimeInitializeOnLoadMethod(RuntimeInitializeLoadType.BeforeSceneLoad)]`; returns immediately unless `-perfmeasure` is present.
- `ScriptedPlayerInputSource` — implements `IPlayerInputSource`; deterministic cadence below.
- `PerformanceProbe` — captures the five metrics and writes one JSON file; allocation-free during the capture window.

## Injection sequence

1. `BeforeSceneLoad`: parse `-perfmeasure`, `-perfout`, `-perfcommit`.
2. Create a GameObject, **deactivate it**, add `BootstrapLifetimeScope`, set `Configuration` to the measurement profile and `LoadArenaOnStart = false`, mark it `DontDestroyOnLoad`, then activate it. `BootstrapLifetimeScope.Awake` builds the process graph only on activation, so the configuration is in place first.
3. The measurement player's scene list is `ArenaScene` only. Its `ArenaSceneBinder.Awake` sets the typed parent reference to the scope from step 2.
4. `SceneManager.sceneLoaded` for `ArenaScene` (after Awake/OnEnable, before `Start`): obtain `binder.CreateSceneObjects()` (the production construction path), build an `ArenaSceneObjects` with the same camera, player, and enemy transforms but the `ScriptedPlayerInputSource`, and assign it to `binder.SceneObjectsOverride`.
5. `ArenaSceneBinder.Start` builds the scene scope with the override; `ArenaRunStartupDriver` starts the run; `ArenaTickDriver` drives the tick.
6. The probe polls `ArenaSceneLifetimeScope.Container` until it is built, resolves `ArenaRunSession` and `IArenaRunRepository`, then starts the warm-up clock when `HasActiveRun` becomes true.

## Scripted input cadence

`elapsed` is unscaled wall time since the run became active.

- **Movement** — two-second phases cycling forward-right, forward-left, back-left, back-right: `(vertical, horizontal)` = `(1, 0.5)`, `(1, -0.5)`, `(-1, -0.5)`, `(-1, 0.5)`, repeating.
- **Aim** — the view direction rotates 36° per second around Y, starting at forward.
- **Attack** — one attack request on the first frame of every whole second.
- **Weapon switch** — on the first frame of every second where `second % 5 == 0`, request a switch and an attack in the same frame (the tick applies switching before the attack, so this is a melee attack); on the first frame of `second % 5 == 1`, request a switch back to ranged and attack. Every fifth second therefore carries a melee attack, as ADR-0033 requires.

The cadence needs no enemy lookup: misses still execute the targeting resolver, the physics query, and the attack-impact path the thresholds measure.

## Capture protocol

1. Warm-up: 5 s after the run becomes active; samples in this window are discarded.
2. Capture: the next 55 s, ending at 60 s total. Sample every frame.
3. End: write the JSON result, flush, `Application.Quit()`.
4. Repeat on a clean process; both runs use the same build and profile.

## Result file schema

```json
{
  "build":   { "commit": "<sha>", "unityVersion": "<version>", "builtAtUtc": "<iso>" },
  "machine": { "os": "<SystemInfo.operatingSystem>", "cpu": "<processorType>",
               "gpu": "<graphicsDeviceName>", "memoryMb": 0 },
  "profile": { "playerHealth": 1000000, "regularEnemyHealth": 1000000,
               "waveEnemies": 10, "spawnIntervalSeconds": 2 },
  "window":  { "warmupSeconds": 5, "captureSeconds": 55, "capturedFrames": 0,
               "runEndedEarly": false },
  "frameTime":       { "averageMs": 0, "p99Ms": 0, "maxMs": 0,
                       "source": "FrameTimingManager.cpuFrameTime" },
  "gcAlloc":         { "totalBytes": 0, "framesWithAllocation": 0 },
  "activeEnemies":   { "peak": 0 },
  "physicsQueries":  { "peakPerFrame": 0, "counter": "<resolved marker name>" },
  "pass": { "averageFrameTime": false, "worstFrameTime": false, "gcAlloc": false,
            "activeEnemies": false, "physicsQueries": false }
}
```

## Build and run

Build (from the repository root):

```
I:\Programs\UnityEditors\2022.3.46f1\Editor\Unity.exe -batchmode -quit ^
  -projectPath I:\Code_Source\UnityProjects\Wild_Wild_West ^
  -executeMethod Game.Performance.Editor.PerformancePlayerBuilder.Build ^
  -perfcommit <sha> ^
  -logFile I:\Code_Source\UnityProjects\Wild_Wild_West\perf-build.log
```

The builder writes `Builds\Performance\` (player plus `build-info.json`), sets `PERF_MEASUREMENT` for the Standalone group for the duration of the build, and restores the previous defines afterwards. `Builds/` is not committed.

Run each session (separate clean process):

```
Builds\Performance\WildWildWest-Performance.exe -perfmeasure ^
  -perfout <absolute path>\run1.json -perfcommit <sha> ^
  -screen-width 1920 -screen-height 1080 -screen-fullscreen 0
```

## Abort conditions

An aborted run is not a pass. Stop and report to the architect; do not substitute a marker or a value.

- The physics counter cannot be resolved through `ProfilerRecorderHandle.GetAvailable` for the player build.
- The run reaches `Victory` or `Defeat`, or the scene reloads, inside the 60 s window.
- Fewer than 55 s of frames are captured, or the process exits non-zero.
- The probe itself allocates during the capture window (prove the harness allocation-free before judging metric 3).

## Recording results

Append one row per run; never edit a recorded row. A rerun after a code change is a new row.

| Run | Build commit | Date (UTC) | Avg ms | P99 ms | Max ms | GC B/frame | Enemy peak | Physics peak | Verdict |
|---|---|---|---|---|---|---|---|---|---|
| 1 | `38f856e` | 2026-10-26T23:37:52Z | 16.666 | 19.903 | 23.203 | 35 200 | 10 | 12 | fail (GC, physics) |
| 2 | `38f856e` | 2026-10-26T23:39:08Z | 16.666 | 19.593 | 23.727 | 31 200 | 10 | 12 | fail (GC, physics) |

- **Machine:** Windows 10 (10.0.19045) 64-bit, Intel Core i5-10500H @ 2.50 GHz, NVIDIA GeForce GTX 1650 Max-Q, 24 393 MB.
- **Window:** both runs captured 3 301 frames over 55 s, `runEndedEarly = false`.
- **Harness attribution:** `GC.GetAllocatedBytesForCurrentThread()` delta over each capture block is 0 B in both runs, so the recorded GC total is game-side, not harness-side.
- **GC allocation:** every captured frame allocates in both runs (3 301 / 3 301), at a near-constant per-frame amount — 35 200 B in run 1, 31 200 B in run 2.
- **Preliminary run (not recorded):** the implementing coder's run of the same code produced 31.1 KB/frame and physics peak 12. Its `build-info.json` recorded `-perfcommit e633b4f`, the specification commit rather than the code commit, so it is not attributable and is not entered as a results row.

**Legacy comparison:** not applicable — no runnable legacy scene exists (scene-GUID evidence in § Scope).
