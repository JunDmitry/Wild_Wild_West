# ADR-0021: Automatic defeat restart is an Application tick concern

- **Status:** Accepted
- **Date:** 2026-10-22
- **Authors:** architect-role
- **Reviewers:** none
- **Related ADRs:** [ADR-0003](ADR-0003-arena-run-aggregate-and-repository.md), [ADR-0005](ADR-0005-domain-events-and-application-notifications.md), [ADR-0013](ADR-0013-stage-attacks-into-start-and-impact.md), [ADR-0015](ADR-0015-standalone-dotnet-build-for-arena-core.md)

## Context

Feature F-010 requires a defeated run to restart by itself: no defeat window,
no message, no confirmation, and no player input (BR-017). The restarted run
begins at the first Wave with the player at full Health and the Firearm
equipped (BR-027), and carries nothing from the failed attempt (BR-028).

The pieces that already exist:

- `ArenaRun` transitions to `ArenaRunStatus.Defeat` when the player's Health
  reaches zero (BR-015, BR-101). A terminal run is never reset (BR-016).
- `ArenaRunSession.RestartDefeatedRun()` allocates fresh `ArenaRunId` and
  `PlayerId`, builds a new run through `ArenaRunFactory.Start` — the same path
  as the initial run, so Wave one, full Health, and the Firearm come for free —
  adds it, removes the defeated run, and returns a snapshot at
  `AggregateRevision.Initial` in `Playing`.

What did not exist: any production caller. `RestartDefeatedRun()` was reachable
only from tests. The tick observed a defeated run only by returning early:
`ArenaRunTickCoordinator` stops as soon as `run.Status != ArenaRunStatus.Playing`
and the time stage refuses to advance a non-`Playing` run. Nothing accumulated
a delay, and no tuning value described one. The delay is a declared tuning
value (analyst, F-010).

## Decision

The automatic defeat restart is owned by the Application tick.

- A new port `IRunRestartDelayPolicy` (in `Game.Arena.Application.Restart`)
  decides, from the elapsed tick delta, whether the restart is due.
  `FixedIntervalRunRestartDelayPolicy(GameDuration)` is the implementation,
  accumulating game time and reporting due once the configured delay has
  elapsed. It mirrors the existing `IEnemySpawnPacingPolicy` pattern.
- A new tick stage `DefeatRestartStage` runs in `ArenaRunTickCoordinator` after
  the clock delta is read. It acts only while the active run is in `Defeat`:
  it records `ArenaRunTickStage.DefeatRestart`, feeds the delta to the policy,
  and, when the policy reports due, calls
  `ArenaRunSession.RestartDefeatedRun()`.
- When a restart happens, the tick builds its result from the **new** run
  (fresh id, `AggregateRevision.Initial`, `Playing`) and ends for that frame;
  no player input is read and no phase runs. The restarted run is ticked
  normally from the next tick onward.
- `ArenaRunTickServiceDependencies` carries the policy; `ArenaRunTickService`
  resets it together with the spawn pacing policy whenever the active run id
  changes — the existing branch that already exists for a new run.
- The delay is supplied by composition as a `GameDuration`. No Unity asset,
  scriptable object, or scene reference is introduced here; the CompositionRoot
  (a stub today) will supply the value when it is wired.

The Domain is unchanged. BR-016 makes a restart a new aggregate, not a
mutation of a terminal one, so no defeat timer enters `ArenaRun`.

Out of scope: a defeat screen or message, a score summary, manual restart, and
save/load — all explicitly outside F-010.

## Rationale

- BR-017 is a gameplay orchestration rule, not a rendering rule. The
  Application already owns the run lifecycle (`StartInitialRun`,
  `RestartDefeatedRun`) and the per-run policy reset; the timer belongs there.
- Putting the timer in the view would make BR-017 verifiable only in PlayMode
  and would give the scene authority over the run lifecycle.
- Putting the timer in the aggregate would contradict BR-016 and add a tuning
  value and a wall-clock concept to a terminal Domain object.
- The tick delta is already the game-time source for every other timed
  behaviour, so the delay is deterministic and testable with a fixed clock.
- The run-id-change branch in `ArenaRunTickService` already exists exactly to
  reset per-run policies; a restart reuses it instead of adding a second
  lifecycle hook.

## Consequences

### Positive

- BR-017, BR-027 and BR-028 are enforced in the standalone suite with no Unity.
- The view observes a new `ArenaRunId` and a `Playing` snapshot; it needs no
  restart logic of its own.
- Adding a delay tuning value is a constructor argument, not a Domain change.
- `ArenaRunTickStage.DefeatRestart` makes the event visible to consumers
  through the existing result vocabulary.

### Negative

- A tick can now end with a different `ArenaRunId`, `FinalRevision`, and
  snapshot than it started with. Consumers that assume a stable run id across
  ticks must read the id from the result (they already do).
- `ArenaRunTickServiceDependencies` gains a required dependency; every
  construction site must pass a policy.
- The restart frame does not consume player input and runs no phases.

### Neutral / Risks

- The delay is game time from `IGameClock`, not the defeated run's frozen
  `Time`; a paused or zero-delta clock also stalls the restart, which is the
  intended meaning of "the delay is a tuning value".
- A policy configured with a very short delay restarts on the first tick after
  defeat; a very long one behaves like a stop. Both are valid tunings.

## Alternatives

1. **Presentation observes `Defeat` and restarts on its own timer** — no
   Application change, the scene can add a defeat beat.
   - Why not chosen: gameplay orchestration in the view layer; BR-017 becomes
     untestable outside PlayMode; the run lifecycle would live where there is
     no aggregate access.
2. **`ArenaRun` accumulates defeat time and exposes "restart due"** — the
   delay sits next to the defeat edge.
   - Why not chosen: a terminal aggregate would carry a temporal policy and a
     tuning value, and BR-016 makes the restart a new aggregate, not a Domain
     action.
3. **Restart from `ArenaRunTickService` between ticks, without a new stage** —
   fewer moving parts in the coordinator.
   - Why not chosen: the stage vocabulary (`ArenaRunTickStage`) is how the tick
     reports what ran; hiding the restart outside the pipeline would make the
     result unable to say a restart occurred, and would split per-run policy
     handling across two places.

## Compliance with paradigm

- ADR-0007: the new port and policy live in `Game.Arena.Application` and use
  only Domain value types (`GameDuration`); no Unity type is introduced.
- ADR-0015: all new behaviour is covered by the standalone Application suite.
- ADR-0003/BR-016: the Domain aggregate is untouched; identity allocation and
  repository swap stay in `ArenaRunSession`.
- ADR-0013: the stage follows the existing pipeline pattern; it does not touch
  the attack start/impact split.
- `docs/architect/paradigm.md`: no deviation; no exception is added.

## Migration plan

1. Add `IRunRestartDelayPolicy` and `FixedIntervalRunRestartDelayPolicy`.
2. Add `ArenaRunTickStage.DefeatRestart`.
3. Add `DefeatRestartStage`; run it in `ArenaRunTickCoordinator` after the
   clock delta is read, returning the new run's result when a restart occurs.
4. Add the policy to `ArenaRunTickServiceDependencies`; reset it with the
   pacing policy on an active-run id change.
5. Update `application-tick-contract.md` with § Defeat and Restart.
6. Extend Application tests (policy, stage, coordinator restart scenarios) and
   refresh the PlayMode harness construction.
7. Build and run the standalone suites.

Each step leaves the standalone build and tests green.

## References

- `docs/architect/analyst-inbox/F-010.md` — US-039, US-040, US-041, BR-015,
  BR-016, BR-017, BR-027, BR-028, BR-051
- `docs/architect/architecture/arena-combat/application-tick-contract.md`
- `Assets/_Project/Contexts/Arena/Application/Sessions/ArenaRunSession.cs`
- `Assets/_Project/Contexts/Arena/Application/Ticks/ArenaRunTickCoordinator.cs`
- `Assets/_Project/Contexts/Arena/Application/Spawning/FixedIntervalSpawnPacingPolicy.cs`
