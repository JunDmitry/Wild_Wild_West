# ADR-0022: Publish one-shot facts through an Application notification sink

- **Status:** Accepted (dispatch seam and failure policy superseded by [ADR-0027](ADR-0027-typed-result-dispatcher.md))
- **Date:** 2026-10-22
- **Authors:** architect-role
- **Reviewers:** none
- **Related ADRs:** [ADR-0005](ADR-0005-domain-events-and-application-notifications.md), [ADR-0013](ADR-0013-stage-attacks-into-start-and-impact.md), [ADR-0021](ADR-0021-automatic-defeat-restart-in-application-tick.md)

## Context

Feature F-011 adds BR-118: when an ArenaRun reaches Victory, a Victory window
stating that the run was won is shown to the player. The Domain already
transitions the run (`ArenaRunVictorious` in the same revision as
`WaveCompleted`) and already stops the arena: every gameplay entry point is
guarded by `Status == Playing`, so BR-117 (no Attack after Victory), no
movement, no spawn, no new Wave, and "never Defeat again" all hold.

What does not exist is a way for the view to learn that the run was won. The
tick result carries `ArenaRunSnapshot.Status == Victory` and the events, but
the Presentation is a stub, notification mapping (T-4.4) and the typed
dispatcher (T-5) are deferred, and the delivery plan records that continuous
state is rendered from snapshots while one-shot facts are delivered through
notifications.

The decision taken for F-011 is to introduce the notification seam now for the
victory fact rather than leave the window to view-side edge detection.

## Decision

The Application owns the mapping from one-shot Domain events to Application
notifications and publishes them through a sink port at the end of a tick.

- `IArenaRunNotification` is the marker for Application notifications in
  `Game.Arena.Application.Notifications`.
- `ArenaRunVictoryNotification` is the first notification: it carries the
  `ArenaRunId` and the `AggregateRevision` of the victory transition. It states
  the fact, not the UI; the Victory window is the Presentation's reaction.
- `ArenaRunNotificationMapper` maps `ArenaRunVictorious` to the notification
  and ignores every other event. It is the single place the mapping lives, so
  T-4.4 extends it instead of duplicating it.
- `IArenaRunNotificationSink` (`Application/Ports`) is implemented by
  presentation or composition. The tick publishes each mapped notification
  after it has built `ArenaRunTickResult` and before returning it.
- A sink exception fails the tick with the existing
  `ArenaRunTickFailedException`, carrying the already-built result. Notifications
  already published in that tick are not rolled back and the remaining ones are
  not published. There is no retry.
- Continuous state stays in the snapshot; notifications are not a second
  snapshot channel.
- The tick result keeps its Domain events. Notifications are additive, so the
  view can be driven by the sink while existing consumers are unchanged.

Scope is the victory fact only. The rest of the T-4.3 vocabulary (damage,
wave transitions, spawns) and the T-5 dispatcher pipeline are not defined here;
when they land, they extend the same mapper and may replace the single sink
with the typed handler pipeline.

## Rationale

- BR-118 describes a one-shot fact. Snapshots are the wrong channel: a view
  polling the status must invent edge detection and can miss or repeat the
  transition.
- Mapping a Domain event to a notification is an Application responsibility in
  this project's layering (ADR-0005; delivery plan T-4.4). Putting the edge
  detection in the view would move a use-case concern into Presentation.
- Publishing from the tick keeps ordering: the notification is produced by the
  same result that reports the victory, so the view never sees a win before the
  snapshot that describes it.
- One sink method keeps the seam small; T-5 can keep this port as the final
  stage of the dispatcher.

## Consequences

### Positive

- The Victory window has a push signal with exactly one producer.
- The victory transition cannot be repeated: `ArenaRunVictorious` fires once,
  and a restarted run is a new aggregate.
- The mapping is unit-testable in the standalone suite.
- T-4.4/T-5 inherit a concrete, used seam instead of designing one from nothing.

### Negative

- `ArenaRunTickServiceDependencies` gains a required sink; every construction
  site (tests, PlayMode harness, future CompositionRoot) must provide one.
- The tick can now fail after the aggregate work is complete because a
  presentation sink threw. Callers must handle the partial-result path even for
  successful gameplay.
- The notification vocabulary is partial until T-4.3 is finished.

### Neutral / Risks

- A sink that publishes twice or shows the window twice is a view bug; the
  Application guarantees one notification per victory event.
- If the sink re-enters the tick service, ordering is undefined; sinks must only
  record or schedule presentation work.
- The mapping iterates Domain events, so its cost is linear in events per tick;
  victory-only mapping makes it negligible.

## Alternatives

1. **Presentation detects the Playing → Victory edge in the tick result** — no
   new Application vocabulary.
   - Why not chosen: the one-shot rule becomes an untestable view
     responsibility, and a later T-5 dispatcher would duplicate the signal.
2. **Model the announcement in the aggregate** (`VictoryAnnounced` state or a
   flag) so it cannot be repeated.
   - Why not chosen: BR-118 is about what the player sees, not about run state;
     a terminal Domain object would carry UI lifetime, and BR-012/BR-013 already
     define the lifecycle.
3. **Full T-4.3/T-5 dispatcher now** — define the whole notification vocabulary
   and typed handler pipeline.
   - Why not chosen: too much design ahead of need; the feature requires one
     fact, and the plan already sequences T-4 and T-5.

## Compliance with paradigm

- ADR-0005: Domain events remain the source of one-shot facts; notifications are
  their Application representation. The Domain is unchanged.
- ADR-0007: the port and notification types live in `Game.Arena.Application`
  and use only Domain value types (`ArenaRunId`, `AggregateRevision`).
- ADR-0021: notification dispatch follows the tick's result, so a restart tick
  still returns its new run and publishes only the facts it produced.
- `docs/architect/paradigm.md`: no deviation; no exception is added.

## Migration plan

1. Add `IArenaRunNotification`, `ArenaRunVictoryNotification`, and the mapper.
2. Add `IArenaRunNotificationSink` and the dispatcher.
3. Add the sink to `ArenaRunTickServiceDependencies`; dispatch after the result
   is built; wrap sink failures.
4. Update `application-tick-contract.md` with § Victory and Terminal Runs.
5. Extend Application tests (mapper, dispatcher, end-to-end victory) and refresh
   construction sites.
6. Build and run the standalone suites.

Each step leaves the standalone build and tests green.

## References

- `docs/architect/analyst-inbox/F-011.md` — US-042, US-043, BR-117, BR-118
- `docs/architect/architecture/arena-combat/application-tick-contract.md`
- `docs/coder/delivery-plan.md` — T-4.3, T-4.4, T-5
- `Assets/_Project/Contexts/Arena/Domain/Aggregates/ArenaRun/ArenaRun.cs`
- `Assets/_Project/Contexts/Arena/Domain/Events/ArenaRunVictorious.cs`
