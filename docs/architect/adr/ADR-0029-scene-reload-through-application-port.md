# ADR-0029: Request an Arena scene reload through an Application port at the restart edge

- **Status:** Accepted
- **Date:** 2026-10-25
- **Authors:** architect-role
- **Reviewers:** none
- **Related ADRs:** [ADR-0007](ADR-0007-unity-isolation-and-layer-dependencies.md), [ADR-0014](ADR-0014-use-vcontainer-for-dependency-injection.md), [ADR-0021](ADR-0021-automatic-defeat-restart-in-application-tick.md), [ADR-0025](ADR-0025-derive-defeat-window-from-published-state.md), [ADR-0027](ADR-0027-typed-result-dispatcher.md)

## Context

The delivery plan's T-6.9 requires "an Application port for scene reload (defeat
delay and restart) implemented with `SceneManager`; a PlayMode test proves a
defeated run triggers the configured reload exactly once". Today nothing in the
project touches `SceneManager`, and `Game.CompositionRoot` is still an assembly
marker, so T-8 will build the two-scene lifecycle (BootstrapScene plus a
reloadable ArenaScene) that ADR-0014 describes.

The restart itself is already delivered. ADR-0021 and the tick contract
(§ Defeat and Restart) make the tick the owner of the defeat delay:
`DefeatRestartStage` feeds the tick delta to `IRunRestartDelayPolicy` and, when
it reports due, calls `ArenaRunSession.RestartDefeatedRun()`, which allocates
fresh identities, adds a new `ArenaRun`, and removes the defeated one. The
restarted run exists in the repository before the tick returns.

What is missing is the Unity half: reloading the ArenaScene so per-scene
adapters and views are rebuilt (ADR-0014). The tick contract currently states
that Application "does not ... reload scenes", which is true in the physical
sense — Application cannot call `SceneManager` — but says nothing about how the
request reaches Unity.

The material question is *where the request originates*. Three candidates
exist: at the restart edge inside the tick, from a restart notification through
the typed dispatcher (ADR-0027), or from a future scene driver that watches the
active run id change.

## Decision

We will declare `IArenaSceneReloader` in `Game.Arena.Application.Ports` with a
single method, `void ReloadArenaScene()`, and call it from `DefeatRestartStage`
immediately after `ArenaRunSession.RestartDefeatedRun()` reports a restart.

- The port is a command, not a fact. Notifications stay reserved for one-shot
  facts about committed state (ADR-0022, ADR-0027); "reload the scene" is an
  instruction the host must carry out.
- Application still references no Unity type: it names an interface it owns.
  Infrastructure supplies `UnityArenaSceneReloader`, which calls
  `SceneManager.LoadScene` with the scene name composition configures — the
  same adapter shape as every other T-6 port.
- Exactly once is structural. The call sits on the single edge where a defeated
  run becomes a live one, after the restart outcome is confirmed. A tick that
  does not restart a run cannot reload; a restarting tick reloads once. The
  tick service already resets the restart delay policy when the active run id
  changes, so the next tick sees a `Playing` run and the stage returns early.
- The reload request precedes the tick's result dispatch in program order but
  Unity applies `LoadScene` at the end of the frame, so the tick still
  dispatches its result and the restarted run's snapshot is published exactly
  as it is today. The scene then reloads under the new state.
- The reloaded scene binds to the already-restarted run. `ArenaRunSession`
  holds it, and T-8's scene entry must present the active run rather than start
  a second one. The delivery plan's T-8.4 phrasing ("the reloaded scene starts
  a new run") is read as "the reloaded scene presents the restarted run", and
  its wording is corrected alongside this ADR.

## Rationale

- The restart edge is the only place that knows a restart has just happened. A
  driver that watches ids must diff state it did not create, and would also
  fire for the initial run unless specially guarded.
- A notification would put a command on a fact channel, and the dispatch seam
  has no production composition until T-8 — the exactly-once property could not
  be proven in T-6.9 as the delivery plan requires.
- The port keeps ADR-0007 intact: the only code that knows about scenes is
  Infrastructure, and the only code that knows about both sides is the
  composition root.

## Consequences

### Positive

- T-6.9 is provable now: a recording port plus the real tick shows exactly one
  reload for a defeated run, and zero for a run that never restarts.
- T-8 wiring is one registration and two subscriptions (reloader, restart delay
  value) rather than a state-diffing driver.
- No new notification type, no change to the dispatcher, no change to the
  Domain.

### Negative

- The tick contract's sentence "It does not ... reload scenes" must be reworded
  to "it requests a scene reload through a port".
- `ArenaRunTickServiceDependencies` gains a required constructor argument, so
  every construction site — including tests — must supply it. A missing
  registration in T-8 fails loudly at composition rather than silently
  disabling reloads.
- Application code now names a scene-shaped concept (`ReloadArenaScene`), even
  though it remains Unity-free. That is the cost of an honest port.

### Neutral / Risks

- `UnityArenaSceneReloader` must reject an empty scene name at construction:
  composition supplies the name, and a missing one is a configuration error.
- A test must not let the real adapter reload the running test scene; the
  exactly-once proof uses a recording fake. The adapter's own contract
  (configuration validation, delegation to `SceneManager`) is covered without
  performing a destructive load.
- A reload requested while a tick throws later in the same frame would still
  reload; the tick's failure policy is unchanged and out of scope here.

## Alternatives

1. **Restart notification with an Infrastructure handler** — Application adds
   `ArenaRunRestartedNotification` to the tick result; an Infrastructure handler
   registered at the dispatch seam reloads the scene.
   - Why not chosen: it is a command wearing a fact's clothes; the restart fact
     does not exist today; and the exactly-once proof would depend on dispatch
     registration that has no production composition before T-8.
2. **Scene driver observes the active run id** — Application declares the port
   but nothing calls it until T-8; the scene driver reloads when
   `ArenaRunSession.ActiveArenaRunId` changes.
   - Why not chosen: the port is dead code now, so T-6.9's exit criterion cannot
     be met, and the driver must distinguish the initial run from a restart.

## Compliance with paradigm

- ADR-0007: the port lives in Application; the `SceneManager` call lives in
  Infrastructure; no assembly reference is added in either direction.
- ADR-0014: the reload re-creates the ArenaScene scope as designed; the root
  scope and the session survive it.
- ADR-0021: the tick still owns the restart; this ADR only lets it announce the
  consequence to the host.
- ADR-0025: the defeat window still derives from published run state; the
  reload does not add a control or a confirmation.
- `docs/architect/paradigm.md`: no deviation.

## Migration plan

1. Record this ADR and index it.
2. Add `IArenaSceneReloader` and the `DefeatRestartStage` call; plumb it through
   `ArenaRunTickServiceDependencies`.
3. Update the tick contract and the T-8.4 wording.
4. Hand `UnityArenaSceneReloader` and the PlayMode exactly-once fixture to
   `coder-role`.
5. T-8 composes the adapter with the ArenaScene name and binds the scene entry
   to the active run.

## References

- `docs/coder/delivery-plan.md` § T-6.9, § T-8.4
- `docs/architect/architecture/arena-combat/application-tick-contract.md` § Defeat and Restart
- `Assets/_Project/Contexts/Arena/Application/Ticks/Stages/DefeatRestartStage.cs`
- `Assets/_Project/Contexts/Arena/Application/Sessions/ArenaRunSession.cs`
- `Assets/_Project/Contexts/Arena/Application/Ticks/ArenaRunTickServiceDependencies.cs`
