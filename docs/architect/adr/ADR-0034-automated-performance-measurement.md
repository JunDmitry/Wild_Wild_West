# ADR-0034: Automated performance measurement for the production cut-over

- **Status:** Accepted
- **Date:** 2026-10-26
- **Authors:** architect-role
- **Reviewers:** user (approved the `t9-performance-measurement` plan on 2026-10-26)
- **Related ADRs:** [ADR-0033](ADR-0033-performance-thresholds-for-cut-over.md), [ADR-0030](ADR-0030-composition-root-scopes.md), [ADR-0031](ADR-0031-scene-binder-and-frame-driver.md), [ADR-0032](ADR-0032-bootstrap-scene-load-and-initial-run.md), [ADR-0007](ADR-0007-unity-isolation-and-layer-dependencies.md)

## Context

ADR-0033 fixes five measurable thresholds and a player-build measurement procedure, and makes the measurement the last open T-9 entry gate. It also assumes a person with the Unity Profiler: nothing in the repository captures frame time, GC allocations, physics queries, or the active-enemy peak, and there is no player-build entry point. A manual 60 s run depends on human cadence, two clean repeats, and hand transcription of P99 and worst-frame values — exactly the evidence a cut-over decision should not rest on.

The composition already exposes two seams that the T-8.6 smoke fixture drives through the real player graph: `BootstrapLifetimeScope.Configuration` supplies the process-lifetime catalogue, and `ArenaSceneBinder.SceneObjectsOverride` supplies the scene seam (camera, player, input source, enemy transforms). Both reach the production-built scope. The measurement harness can therefore measure the shipped scene stack without changing gameplay or adding a production seam.

The 10 % legacy comparison cannot be run: every `m_Script` GUID in every scene was mapped to `Assets/_Project/Core/**`, and no scene references any legacy script. The legacy stack survives only as frozen model/rules assemblies and characterization tests, so there is no runnable legacy baseline.

## Decision

We will add a development-only measurement harness that builds and runs the real Arena player stack, injects a measurement configuration and a scripted input source through the two already-proven composition seams, captures the five ADR-0033 metrics, and writes one JSON result file per run. The harness is compiled only under the `PERF_MEASUREMENT` scripting define and activates only when the player is started with `-perfmeasure`; no production assembly references it.

The measurement player's scene list is `ArenaScene` only. `BootstrapLifetimeScope.Awake` builds the process graph (VContainer `autoRun`), so a `sceneLoaded` callback is too late to configure it; the harness therefore creates the persistent `BootstrapLifetimeScope` in code before the scene loads, sets `Configuration` and `LoadArenaOnStart = false` while the object is inactive, and activates it. The scene binder's typed parent reference resolves that scope, and the `sceneLoaded` callback sets `SceneObjectsOverride` on the binder after its `Awake` and before its `Start`.

Concrete consequences:

- **Modules affected:** a new `Game.Performance` runtime assembly (define-constrained, leaf above CompositionRoot) and a new `Game.Performance.Editor` assembly holding an `-executeMethod` build script. `ArenaSceneBinder` gains a `CreateSceneObjects()` method so `BuildScope` and the harness share one construction path; Domain, Application, Infrastructure, and Presentation are untouched.
- **Measurement profile:** the shipped arena, weapon catalogue, enemy behaviour, wave size, spawn interval, and restart delay; player and regular-enemy health raised so the 55 s sample can end neither in defeat nor in victory. Health changes no per-frame cost the thresholds measure, and the profile is recorded with every result.
- **Capture:** `FrameTimingManager` for average and worst frame time, `ProfilerRecorder` for per-frame GC allocation and physics queries, `ArenaRun.ActiveEnemyCount` polled through the session and repository for the enemy peak, and `SystemInfo` plus the build commit for the run identity.
- **Bootstrap deviation:** the measurement player does not load `BootstrapScene`. The harness creates the same persistent root scope from the same `ArenaRegistrations` before the Arena scene loads. The measured Arena graph, scene, and tick are identical; the bootstrap scene-load edge is not part of the performance measurement.
- **Legacy comparison:** recorded **not applicable** — no runnable legacy scene exists (scene-GUID evidence above). The absolute ADR-0033 thresholds stand alone.
- **New dependencies:** none. Both metrics APIs are part of the Unity engine.
- **Out of scope:** changing the ADR-0033 thresholds, automated CI regression runs, instrumenting the frozen legacy stack, per-platform thresholds, and memory footprint beyond per-frame allocations.

## Options considered

### Option A — Manual Profiler runbook (ADR-0033 as written)
- Pros: no new code, no new assembly, cheapest to author.
- Cons: P99 and worst-frame values are read by eye; cadence and repeatability depend on a person; results arrive as transcription, not artifacts.
- Why not chosen: the gate is a cut-over decision; it should rest on machine-captured evidence that can be re-run.

### Option B — Development-only measurement harness (chosen)
- Pros: exact capture of all five metrics, two identical clean runs, a repeatable artifact, and a reference procedure for post-cut-over regressions; uses seams the smoke fixture already proves.
- Cons: a new leaf assembly, a build script, and a coder implementation task; the harness itself must be allocation-free after warm-up.
- Why chosen: it turns the last T-9 gate into a repeatable measurement rather than a hand-read.

### Option C — Unity Profiler command-line capture and analysis
- Pros: keeps the built-in Profiler as the single source.
- Cons: binary Profiler capture needs an external analyzer and still cannot derive the enemy peak or the scripted cadence without custom code.
- Why not chosen: more moving parts for the same five numbers.

## Consequences

### Positive

- T-9's last entry gate becomes measurable on demand, with a JSON artifact per run.
- The five metrics map onto the architecture's own boundaries: frame time to the tick and dispatcher, GC allocation to the Application path, enemy peak to the Domain aggregate, physics queries to the Infrastructure adapters.
- No production assembly, scene, prefab, or package changes beyond one composition accessor.

### Negative / trade-offs

- The harness is measurement-only tooling that must itself be trusted to be allocation-free; an abort or a missing counter is a blocker, not a silent zero.
- The measurement profile deviates from the shipped player and enemy health; results are only comparable to runs using the same profile.
- The physics-query counter name must be resolved at runtime; if Unity 2022.3.46f1 exposes no physics-query counter, the harness stops and reports rather than substituting a marker.

### Neutral

- Measurement runs are manual invocations of a built player, not a CI job.
- The harness is not shipped: without `PERF_MEASUREMENT` the assembly does not compile, and without `-perfmeasure` it does nothing.

## Compliance with paradigm

- The runtime layer graph is unchanged: Domain and Application still reference no Unity type (ADR-0007), and no production assembly references the measurement assembly.
- `Game.Performance` is a development-only leaf that references CompositionRoot, Infrastructure, Application, and Domain. It is recorded as an explicit paradigm exception because it sits outside the shipping layer graph.
- The measurement host locates scene components by type only inside the define-gated assembly; production composition still resolves no service through a global search (ADR-0031).

## Migration plan

1. Extract `ArenaSceneBinder.CreateSceneObjects()` and use it from `BuildScope`; the existing `ArenaSceneBinderTests` and `ArenaSandboxSmokeTests` must stay green.
2. Add `Game.Performance` (runtime, `defineConstraints: PERF_MEASUREMENT`) and `Game.Performance.Editor` with the build script, measurement host, scripted input source, and probe.
3. Build the measurement player and run two clean sessions; record the results in `docs/architect/migration/performance-measurement.md`.
4. T-9 proceeds only when every metric is within threshold; a failed metric becomes an optimisation task before cut-over.

Each step leaves the build green.

## Amendment — 2026-10-28: metric 3's scope and the attribution oracle

This amendment settles the question raised by `docs/architect/coder-inbox/c6.md`. It changes no code, no threshold value, and no measurement profile.

- **Metric 3 stays global.** Per-frame managed allocation remains **0 B after warm-up**, measured by the single `GC.Alloc` recorder summed over the whole capture window (ADR-0033). It is not narrowed to "the game's own scope", and no global allocation budget is introduced. ADR-0033 permits revising a threshold only when it is demonstrably wrong for a platform reason; that demonstration does not exist.
- **The measurement player is the only accepted attribution oracle.** Editor-side `GC.GetAllocatedBytesForCurrentThread()` readings are not evidence of player behaviour: the same method read 0 B for a `HashSet.UnionWith` path that allocated roughly 190 B/frame in the player build (measured at `2072667`). An editor reading of 0 B therefore neither attributes nor exonerates a site.
- **Attribution is player-side.** A residual allocation is attributed by method-level A/B in temporary player builds (reverted before the acceptance runs), or by an allocation call-stack capture (`Profiler.enableAllocationCallstacks` with `-profiler-enable -profiler-log-file`) analysed in the Profiler window. Neither changes the harness's five metrics or its capture protocol.
- **A metric-3 revision needs capture evidence.** Only a call-stack capture showing an allocation with no game-code call site justifies a superseding ADR, and that ADR carries the capture as its evidence.

## References

- `docs/architect/adr/ADR-0033-performance-thresholds-for-cut-over.md`
- `docs/architect/migration/performance-measurement.md`
- `docs/architect/migration/criteria-for-switching-production-flow.md` § Performance
- `Assets/_Project/CompositionRoot/BootstrapLifetimeScope.cs`
- `Assets/_Project/CompositionRoot/ArenaSceneBinder.cs`
- `Assets/_Project/CompositionRoot/Tests/GameCompositionRootPlayMode/ArenaSandboxSmokeTests.cs`
