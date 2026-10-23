# ADR-0025: Derive the defeat window from published run state

- **Status:** Accepted
- **Date:** 2026-10-23
- **Authors:** architect-role
- **Reviewers:** none
- **Related ADRs:** [ADR-0020](ADR-0020-presentation-domain-read-model-values.md), [ADR-0021](ADR-0021-automatic-defeat-restart-in-application-tick.md), [ADR-0022](ADR-0022-application-notifications-for-one-shot-facts.md), [ADR-0024](ADR-0024-application-owned-hud-model.md)

## Context

F-010b adds R6/R7 and A6/A7: a defeated run shows a defeat message before the
automatic restart, and the message names the wave the player had reached. The
design hands off screen `S-003` — a full-screen defeat statement and the wave
reached, with no control, living for the restart delay.

The record still asserts the opposite in two places. ADR-0021 § Context and its
out-of-scope line ("a defeat screen or message") were written when F-010 forbade
one; `application-tick-contract.md` § Defeat and Restart says "No defeat window,
message, confirmation, or manual restart exists in the contract." The analyst
asked which side is right and requires the ADR and the spec to agree.

What the published state already carries:

- The run status. `ArenaRunSnapshot.Status == ArenaRunStatus.Defeat` holds for
  exactly the window's lifetime: from the defeat transition until
  `DefeatRestartStage` replaces the run with a fresh `Playing` one.
- `ArenaRunHudSnapshot.IsDefeated` (ADR-0024), derived from that status.
- The wave reached. `WaveSnapshot.Number` is frozen with the defeated run and
  is published on every tick of the delay.

What it does not carry: the total wave count. `WaveStateSnapshot` and
`ArenaRunSnapshot` expose the current `WaveNumber` only, so the design's
"wave 2 of 3" cannot be rendered from published state today (F-008's wave
indicator needs the same total).

## Decision

The defeat window is a Presentation surface over published run state. No
Application notification and no defeat-specific channel is added.

- Presentation renders `S-003` while the published HUD reports the run defeated
  and stops rendering it when the restart publishes the fresh run. It reads the
  wave reached from the published wave state. The window needs no event and no
  edge detection: the defeated state is the window's lifetime.
- The Domain publishes the total wave count in its run state read model.
  `ArenaRun` already holds its `WaveCatalog`; `ArenaRunStateSnapshot` gains
  `WaveTotal`, filled from `WaveCatalog.Last` in the single place the aggregate
  builds a snapshot. `ArenaRunSnapshot` carries the value through the
  Application mapper, and `ArenaRunHudSnapshot` exposes `WaveCurrent`,
  `WaveTotal`, and `WaveText` alongside the other presentation-ready values.
- The window's copy stays in Presentation, as the `S-002` victory window's copy
  does. The labels and numbers the copy and the accessibility announcement use
  come from the HUD model (ADR-0024).
- This ADR supersedes only ADR-0021's out-of-scope assertion about a defeat
  screen or message. ADR-0021's decision — the restart is owned by the
  Application tick — stands unchanged.

Out of scope: a defeat notification, Application-owned defeat copy, a score or
statistics summary, a manual restart or restart control, and save/load.

## Rationale

- Defeat is continuous state, and the window's lifetime is exactly that state.
  ADR-0022 reserves notifications for one-shot facts a snapshot cannot carry and
  forbids using them as a second state channel. The Playing → Defeat transition
  is a fact, but the window is defined by the state that follows it.
- A view that polls `IsDefeated` invents no edge detection: the same value that
  shows the window hides it when the restart publishes a new run. A notification
  would signal the start of the window but not its end, so state would still be
  needed.
- The wave total is part of the run's own state: the aggregate that owns the
  wave catalog is the only component that cannot be told the wrong total.
  Publishing it from the Domain snapshot is a read-model addition, not gameplay
  orchestration in the view, and it keeps the mapper's signature unchanged.
- Putting the total in the read model keeps the view from hardcoding "three" and
  serves F-008's wave indicator with one contract.
- The alternative — deriving the window from a new notification — would need
  ADR-0022 weakened for a surface whose lifetime a snapshot already defines.

## Consequences

### Positive

- The ADR, the product spec, and the tick contract agree: the restart is
  Application-owned; the message is a Presentation reaction to published state.
- `S-003` is testable from the tick result alone, and `S-002`/`S-003` stay
  distinct channels (notification vs state) for the right reasons.
- The wave total is published once and reusable by every wave readout.

### Negative

- `ArenaRunStateSnapshot` and `ArenaRunSnapshot` each gain a member; the Domain
  and Application mappers map one more value. The aggregate builds the state
  snapshot in one place, so no construction site outside the Domain changes.
- The HUD model grows three members, and the mapper maps one more value.
- The window still needs Presentation work (the coder's part); this ADR only
  settles the data and channel.

### Neutral / Risks

- A tick that restarts a defeated run reports the new run, so a consumer that
  misses the intermediate frames can miss the window. Presentation reacts to
  each tick result; this is the existing run-change behaviour.
- The total is read from the aggregate's catalog at snapshot time, so a run
  built with a different catalog reports its own total. No session-level
  wiring is required.

## Alternatives

1. **Presentation-only with no contract change.** The window reads
   `Snapshot.Wave.Number` and cannot show the total.
   - Why not chosen: the design's FL-049 and the accessibility announcement
     require "the first wave of three"; the design would need a rework, and
     F-008 would face the same gap later.
2. **A defeat notification through the ADR-0022 sink.**
   - Why not chosen: it duplicates a state the snapshot already publishes, ends
     up combined with state for the window's lifetime, and weakens the
     one-shot-fact rule for no gain.
3. **Supply the total from the Application's creation parameters.** The
   session already holds `ArenaRunCreationParameters.WaveCatalog` and could
   pass `WaveCatalog.Last` into the mapper.
   - Why not chosen: it pushes a second parameter through
     `ArenaRunSnapshotMapper.Map`, every mapper call site, and every test
     fixture, and it lets a caller supply a total that disagrees with the run.
     The aggregate already owns the catalog and builds the snapshot in one
     place, so carrying the value there is smaller and self-consistent.

## Compliance with paradigm

- ADR-0007: the new HUD/read-model values are plain data in `Game.Arena.Application`; no Unity type is introduced.
- ADR-0020: Presentation reads published snapshot/HUD values; the wave total joins them.
- ADR-0021: the restart decision is unchanged; only the out-of-scope assertion is superseded.
- ADR-0022: no notification is added for state.
- ADR-0024: the HUD model stays the presentation-ready source; the wave readout joins the health and weapon readouts.
- `docs/architect/paradigm.md`: no deviation; no exception is added.

## Migration plan

1. Add `WaveTotal` to `ArenaRunStateSnapshot`, filled from `WaveCatalog.Last`
   in `ArenaRun.CreateSnapshot`.
2. Carry it into `ArenaRunSnapshot` through `ArenaRunSnapshotMapper`.
3. Add `WaveCurrent`, `WaveTotal`, and `WaveText` to `ArenaRunHudSnapshot` and
   map them.
4. Update the Domain and Application tests; build and run the suites.
5. Correct `application-tick-contract.md` § Defeat and Restart and record the
   published total in § Tick Result.
6. Hand the `S-003` presentation work to `coder-role`.

Each step leaves the standalone build and tests green.

## References

- `docs/architect/analyst-inbox/F-010b.md` — R6/R7, A6/A7, BR-121/122/123
- `docs/architect/design-inbox/F-010.md` — `S-003`, FL-048…FL-051
- `docs/product/requirements/F-010-defeat-and-restart.md`
- `docs/architect/architecture/arena-combat/application-tick-contract.md`
- `Assets/_Project/Contexts/Arena/Application/ReadModels/ArenaRunSnapshot.cs`
- `Assets/_Project/Contexts/Arena/Application/Hud/ArenaRunHudSnapshot.cs`
- `Assets/_Project/Contexts/Arena/Application/Sessions/ArenaRunSession.cs`
- `Assets/_Project/Contexts/Arena/Domain/Configuration/WaveCatalog.cs`
