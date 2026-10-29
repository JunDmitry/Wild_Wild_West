# Performance measurement for the production cut-over

- **Authority:** [ADR-0033](../adr/ADR-0033-performance-thresholds-for-cut-over.md) fixes the thresholds and the player-build procedure; [ADR-0034](../adr/ADR-0034-automated-performance-measurement.md) fixes the harness and the measurement profile.
- **Status:** measured 2026-10-27; corrected baseline on build `00903f4` (runs 3–4) reports ~6.1–6.3 KB/frame. 3 of 5 thresholds pass; GC allocation and physics queries fail. T-9 remains blocked.
- **Owner:** `architect-role`; the harness is implemented by `coder-role`.

## Scope

The measurement runs the **target** Arena stack in a Windows player build. The 10 % legacy comparison is recorded **not applicable**: no scene in the repository references any `Game.Core.*` script (every scene's `m_Script` GUIDs were mapped to `Assets/_Project/Core/**`, zero hits), so no legacy gameplay scene is runnable. The absolute thresholds stand alone, as ADR-0033's not-applicable clause allows.

## Metrics and pass criteria

| # | Metric | Threshold | Capture |
|---|---|---|---|
| 1 | Average frame time | ≤ 16.7 ms | `FrameTimingManager` `cpuFrameTime`, mean over the capture window |
| 2 | Worst frame time | P99 ≤ 33.3 ms and max ≤ 50 ms | Same samples, sorted after capture |
| 3a | Game-code allocation | 0 B after warm-up | Allocation call-stack capture with a positive control; no game-code frame is the allocating site (ADR-0037) |
| 3b | Total managed allocation per frame | ≤ 250 B after warm-up | `ProfilerRecorder` marker `GC.Alloc`, sum over the capture window divided by captured frames (ADR-0037) |
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
  "gcAlloc":         { "totalBytes": 0, "framesWithAllocation": 0,
                       "bytesPerFrame": 0, "baselineBytesPerFrame": 250 },
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
- The probe allocates inside the capture window. The capture path (`Update`, `CaptureFrame`, `Finish`) is allocation-free by inspection; `GC.GetAllocatedBytesForCurrentThread()` is **not supported in a 2022.3.46f1 player build** (a deliberate 4 KiB allocation produces no delta), so that API cannot be used as the check.

## Measurement-instrument notes

- The `GC.Alloc` recorder must be started with `ProfilerRecorderOptions.SumAllSamplesInFrame | ProfilerRecorderOptions.WrapAroundWhenCapacityReached`. Without wrap-around, a capacity-1 recorder captures one sample at startup and repeats it for the whole run. Runs recorded before 2026-10-27 with that defect are invalid for metric 3 and are annotated as such below.
- The physics counter is the sum of the three named markers (metric 5); no single `Physics.Query` counter exists in this player.
- **Allocation attribution is player-side only.** Editor `GC.GetAllocatedBytesForCurrentThread()` readings are not evidence of player behaviour — the same method read 0 B for a `UnionWith` path that allocated ~190 B/frame in the player — so attribute a residual with method-level A/B in temporary player builds or an allocation call-stack capture. Metric 3 is the two-part rule of ADR-0037: **3a** game-code allocation 0 B/frame (capture with a positive control), **3b** total global allocation ≤ 250 B/frame against the recorded baseline.
- **A capture needs a positive control.** A missing managed frame does not prove a missing game call — player symbols can be stripped and inlined methods collapse out of a stack. A 3a capture is valid only when a deliberate allocation from game code resolves to a game frame in the same capture, and it must show no other game-code allocating frame.

## Recording results

Append one row per run; never edit a recorded row. A rerun after a code change is a new row.

| Run | Build commit | Date (UTC) | Avg ms | P99 ms | Max ms | GC B/frame | Enemy peak | Physics peak | Verdict |
|---|---|---|---|---|---|---|---|---|---|
| 1 | `38f856e` | 2026-10-26T23:37:52Z | 16.666 | 19.903 | 23.203 | 35 200 † | 10 | 12 | fail (GC, physics) |
| 2 | `38f856e` | 2026-10-26T23:39:08Z | 16.666 | 19.593 | 23.727 | 31 200 † | 10 | 12 | fail (GC, physics) |
| 3 | `00903f4` | 2026-10-27T00:19:15Z | 16.666 | 19.635 | 23.229 | 6 087 | 10 | 12 | fail (GC, physics) |
| 4 | `00903f4` | 2026-10-27T00:20:19Z | 16.667 | 19.823 | 23.389 | 6 277 | 10 | 12 | fail (GC, physics) |
| 5 | `0d946e2` | 2026-10-28T14:33:43Z | 16.666 | 22.323 | 26.320 | 525 | 10 | 12 | fail (GC, physics) |
| 6 | `0d946e2` | 2026-10-28T14:34:47Z | 16.666 | 21.930 | 25.037 | 587 | 10 | 12 | fail (GC, physics) |
| 7 | `2bd8fed` | 2026-10-29 | — ‡ | — ‡ | — ‡ | 241.9 ‡ | 10 | 12 | metric 3b baseline |
| 8 | `2bd8fed` | 2026-10-29 | — ‡ | — ‡ | — ‡ | 235.3 ‡ | 10 | 12 | metric 3b baseline |

† **Invalid for metric 3.** Runs 1–2 used the capacity-1 `GC.Alloc` recorder without wrap-around, which froze its first sample, so the GC column repeats one startup value. They are left unedited as the audit record; runs 3–4 are the corrected baseline (recorder fixed in `00903f4`). Their frame-time, enemy, and physics columns are valid.

‡ **Runs 7–8 are the `t9-gc-residual-b` acceptance runs**, taken at `2bd8fed` (instrumentation reverted; `Assets/` identical to `561ff78`). The report records the GC column and states the frame-time, enemy, and physics columns as unchanged from runs 5–6; the raw `run-*.json` files are not committed, so the frame-time cells are left blank rather than transcribed. Both runs allocated on nearly every captured frame (2 651/3 299 and 2 668/3 301). Under ADR-0037 they are the **metric 3b baseline**: 241.9 and 235.3 B/frame, threshold 250 B/frame.

- **Machine:** Windows 10 (10.0.19045) 64-bit, Intel Core i5-10500H @ 2.50 GHz, NVIDIA GeForce GTX 1650 Max-Q, 24 393 MB.
- **Window:** both runs captured 3 301 frames over 55 s, `runEndedEarly = false`.
- **Harness attribution:** `GC.GetAllocatedBytesForCurrentThread()` is unsupported in this player (a deliberate 4 KiB allocation produces no delta), so harness allocation is established by inspection: the capture path allocates nothing.
- **GC allocation:** every captured frame allocates in both corrected runs, at 6 087 B/frame (run 3) and 6 277 B/frame (run 4). The allocation sits in the per-tick path — the domain/application snapshot graph, the per-frame enemy movement batch and its domain events, and presenter collections — not in TextMeshPro text assignment (a change-driven HUD spike lowered the total by only ~2 %).
- **GC allocation after ADR-0035/0036 (runs 5–6):** the pooled tick path is implemented (commits `d5f9fa4`…`acd1dd7`) and the editor allocation fixture measures 0 B over 300 steady ticks and a ten-event frame, but the player build still allocates on 3 253 and 3 264 of 3 301 captured frames — 525 and 587 B/frame, about 91 % below runs 3–4. The residual therefore lies outside the Application/Domain tick path the fixture exercises. The implementation report names `HitConfirmationPresenter` and `EnemyAttackCuePresenter` (view instantiation on events, lazily allocated lists) as the unattributed candidates; per-frame HUD/string work is a second. Attribution is the next step; metric 3 is not met.
- **GC allocation after ADR-0037 (runs 7–8):** attribution is complete. Disabling the tick leaves 0.1 B/frame, skipping the player movement stage 31.1, and an allocation call-stack capture bottoms at `VContainerUpdate/UpdateFunction.Invoke()` with no game-code frame — the residual tracks motion of the rendered entities (report `docs/architect/coder-inbox/t9-gc-residual-b.md`). ADR-0037 replaces metric 3 with 3a (game-code 0 B, capture with a positive control) and 3b (total ≤ 250 B/frame). Runs 7–8 are the 3b baseline; the 3a capture is still to be taken.
- **Preliminary run (not recorded):** the implementing coder's run of the same code produced 31.1 KB/frame and physics peak 12. Its `build-info.json` recorded `-perfcommit e633b4f`, the specification commit rather than the code commit, so it is not attributable and is not entered as a results row.

**Legacy comparison:** not applicable — no runnable legacy scene exists (scene-GUID evidence in § Scope).
