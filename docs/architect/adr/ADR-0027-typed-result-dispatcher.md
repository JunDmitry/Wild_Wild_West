# ADR-0027: Dispatch the tick result through typed ordered handlers

- **Status:** Accepted
- **Date:** 2026-10-25
- **Authors:** architect-role
- **Reviewers:** none
- **Related ADRs:** [ADR-0005](ADR-0005-domain-events-and-application-notifications.md), [ADR-0020](ADR-0020-presentation-domain-read-model-values.md), [ADR-0022](ADR-0022-application-notifications-for-one-shot-facts.md), [ADR-0024](ADR-0024-application-owned-hud-model.md)

## Context

ADR-0022 introduced the notification seam for the victory fact: the tick maps
Domain Events to Application notifications and publishes them, one at a time,
through `IArenaRunNotificationSink`. It deliberately left the typed dispatcher
of delivery stage T-5 open and said the sink could be replaced by it.

The sink cannot carry T-5's pipeline. Its stages begin with snapshot
publication and entity view synchronization, which need the tick result — the
snapshot — not an individual notification. A sink-shaped dispatcher would have
to grow a second, result-shaped entry point later, and every consumer would
carry both contracts meanwhile.

The seam is also the wrong failure boundary. A sink exception currently fails
a tick whose aggregate work is already committed, so one broken view prevents
the remaining views from receiving facts for that tick and forces every caller
to handle the partial-result path for successful gameplay.

## Decision

We will replace the notification sink with a typed dispatcher that receives the
tick result and runs six ordered pipeline stages over it.

- `ArenaRunTickResult` carries the tick's Application notifications
  (`Notifications`), mapped once by `ArenaRunNotificationMapper` in its
  constructor from the already-validated Domain Events. The result remains the
  complete description of the tick: snapshot, HUD model, Domain Events,
  executed stages, notifications.
- `IArenaRunDispatch` (`Application/Dispatch`) is the outbound seam:
  `Dispatch(ArenaRunTickResult)`. `ArenaRunTickService` calls it after the
  coordinator builds the result and before returning the result. It does not
  dispatch notifications individually, and the result is never rebuilt.
- `IArenaRunNotificationSink` and `ArenaRunNotificationPublisher` are removed.
  There is no adapter and no deprecation window: the tick's outbound contract
  is the dispatcher.
- `ArenaRunDispatchStage` names the six plan stages in order: snapshot
  publication, entity view synchronization, gameplay visual, HUD, audio and
  visual effects, analytics and diagnostics. No call site identifies a stage by
  string.
- Stage 1 and 2 handlers implement `IArenaRunSnapshotHandler` and receive
  `ArenaRunTickResult.Snapshot`. Stages 3–6 handlers implement
  `IArenaRunNotificationHandler<TNotification>`; a handler registered for a
  base notification interface receives every assignable notification, which
  lets an analytics handler observe the whole vocabulary without a switch.
- Ordering is deterministic: stages run in declaration order; snapshot handlers
  run in registration order within their stage; a notification stage delivers
  the tick's notifications in publication order and, for each notification,
  its matching handlers in registration order. The order is therefore
  independent of handler execution and stable across runs.
- `ArenaRunResultDispatcher` is the Application implementation. Registration
  closes at the first dispatch; a handler registered twice for the same stage
  and payload type is rejected; a snapshot handler registered for a
  notification stage (or the reverse) is rejected.
- `IArenaRunDispatchDiagnostics` is an optional port. Every handler call is
  guarded: a handler exception is reported as an `ArenaRunDispatchFailure`
  (stage, handler name, payload type, exception), the remaining handlers in
  that stage still run, the later stages still run, and the tick returns
  normally. A diagnostics callback that itself throws is ignored; diagnostics
  never change gameplay.
- A failure of the dispatcher itself (not a handler) still propagates from
  `ExecuteTick` as `ArenaRunTickFailedException` carrying the built result, so
  the existing partial-result contract is unchanged for programming errors.
- Dispatch is single-threaded and not re-entrant. A dispatch on one result
  completes before the next tick starts.

## Rationale

- The tick result is already the one object that describes a tick; adding the
  mapped notifications completes it instead of creating a second channel that
  has to be ordered against the first.
- Result-driven dispatch makes the plan's six stages implementable as written,
  with no stage that has no input.
- Replacing the sink now costs a small migration: one Application service, one
  Application port, one Presentation presenter, and two test fixtures. Keeping
  it would cost a second contract and an adapter that must later be retired.
- Isolating handler failures matches the layering: a view error must not
  roll back or block a committed gameplay tick, and the tick result — the only
  thing the caller needs — must survive it. `ArenaRunDispatchDiagnostics` keeps
  the failure observable without throwing presentation errors into Application
  flow.
- Registration-time validation moves wiring mistakes to composition time,
  before the first tick, instead of to the first notification that happens to
  reach a broken handler.

## Consequences

### Positive

- One outbound contract for the tick; no sink/publisher pair to maintain.
- The six pipeline stages exist as ordered contracts and are testable without
  Unity.
- Handler failures are visible through diagnostics and cannot fail a tick.
- Snapshot and entity-view synchronization precede every notification stage by
  construction, so a handler always sees the snapshot that the notifications
  belong to.
- Application still references no Presentation type: handlers are registered
  from composition, and the Application only knows the interfaces.

### Negative

- The tick result gains a `Notifications` field and maps it on construction;
  every result now pays the mapping cost, including results nobody dispatches.
- `ArenaRunTickServiceDependencies` changes its required port, so every
  construction site (Application tests, PlayMode harness, future
  CompositionRoot) must provide a dispatcher.
- The Unity PlayMode assembly is red until the coder migrates the presenter and
  the harness (handoff `t5-dispatcher`); the standalone Application and Domain
  suites stay green.
- A handler can no longer observe the pre-tick snapshot through its own
  argument. Presentation keeps the previously published snapshot itself when a
  notification needs a position for an enemy the tick removed (see the tick
  contract).

### Neutral / Risks

- Ordering is deterministic but not commutative: a HUD handler that assumes
  it runs after a gameplay handler must be registered in the later stage, not
  merely later.
- `ArenaRunTickResult` now depends on `ArenaRunNotificationMapper`; both are
  Application types, so the layer rule is unaffected.
- If a future stage needs a different input, it must be added to
  `ArenaRunDispatchStage` in the middle of the order, which shifts the
  declaration values. The enum is not serialized, so no persisted data is
  affected.
- The dispatcher is not thread-safe by design; it is called from the Unity
  main thread only, like the tick service.

## Alternatives

1. **Dispatcher implements the sink** — `ArenaRunNotificationDispatcher :
   IArenaRunNotificationSink`, tick service unchanged.
   - Why not chosen: the sink cannot see the result, so stages 1–2 would remain
     outside it and a result-shaped entry point would be added later; two
     contracts for one pipeline.
2. **Keep the sink as the last stage of the dispatcher** — a result dispatcher
   that forwards notifications to the existing sink.
   - Why not chosen: the sink's fail-fast policy would survive inside the
     dispatcher, and the untyped `Publish` would remain the only path to
     Presentation; the typed handler contracts would be optional rather than
     the seam.
3. **Notification-only dispatcher with stages 3–6** — snapshot and entity
   stages stay with the presenters until T-8.
   - Why not chosen: the plan defines six stages, and T-8's tick driver would
     have to add stages 1–2 and re-order their call sites anyway.

## Compliance with paradigm

- ADR-0005: Domain Events remain the source of one-shot facts; notifications
  stay their Application representation. The Domain is unchanged.
- ADR-0007: every new type lives in `Game.Arena.Application` and uses only
  Application and Domain types; no Unity type enters the layer.
- ADR-0020: Presentation handlers receive `ArenaRunSnapshot` and Application
  notifications, both of which they already consume.
- ADR-0022: the notification vocabulary and the single mapping place are kept;
  its sink-based dispatch seam and its sink-failure policy are superseded by
  this ADR.
- ADR-0024: the HUD model is unchanged and available to stage-3 handlers
  through the result; HUD handlers still do not format values.
- `docs/architect/paradigm.md`: no deviation; no exception is added.

## Migration plan

1. Record this ADR and mark ADR-0022's dispatch seam superseded.
2. Add `Notifications` to `ArenaRunTickResult`, mapped by
   `ArenaRunNotificationMapper`.
3. Add the dispatch contracts and `ArenaRunResultDispatcher`.
4. Replace the sink in `ArenaRunTickServiceDependencies` and
   `ArenaRunTickService` with `IArenaRunDispatch`; delete the sink port and the
   publisher.
5. Migrate the Application tests and add dispatcher coverage (routing, stage
   order, registration order, no-handler, duplicate and late registration,
   isolated handler failure, diagnostics reporting).
6. Update `application-tick-contract.md` and orientation; run the standalone
   suites.
7. Hand the Presentation handler extraction, registration, and PlayMode
   migration to `coder-role` (`docs/coder/architect-inbox/t5-dispatcher.md`).

Steps 1–6 leave the standalone build and tests green. The Unity test assembly
is red from step 4 until the coder finishes step 7, which is stated in the
handoff.

## References

- `docs/coder/delivery-plan.md` § T-5
- `docs/architect/architecture/arena-combat/application-tick-contract.md`
- `Assets/_Project/Contexts/Arena/Application/Ticks/ArenaRunTickService.cs`
- `Assets/_Project/Contexts/Arena/Application/Ticks/ArenaRunTickResult.cs`
- `Assets/_Project/Contexts/Arena/Application/Notifications/ArenaRunNotificationMapper.cs`
- `Assets/_Project/Contexts/Arena/Presentation/Notifications/ArenaRunNotificationPresenter.cs`
- `docs/architect/adr/ADR-0022-application-notifications-for-one-shot-facts.md`

## Amendment 2026-10-25 — snapshot and HUD handlers at every stage

T-8.2 had to register snapshot-driven gameplay-visual and HUD presenters, and no
handler could see `ArenaRunTickResult.Hud`. The stage/payload split in the
Decision above is narrowed as follows; everything else stands.

- Every stage accepts `IArenaRunSnapshotHandler`. The stages 1–2 restriction is
  removed.
- A new `IArenaRunHudHandler` (`Application/Dispatch`) receives both the result's
  snapshot and its HUD model, and is accepted at every stage.
- Within a stage the dispatcher runs snapshot handlers first, then HUD handlers,
  then notification handlers, each in registration order. Notification handlers
  remain restricted to stages 3–6, so snapshot and entity-view work still
  precedes every notification stage.
- Registration closing, duplicate rejection, and per-handler failure isolation
  are unchanged. A HUD handler failure is reported with
  `typeof(ArenaRunHudSnapshot)` as its payload type.
- The Compliance section's claim that the HUD model is "available to stage-3
  handlers through the result" is corrected: the model reaches handlers only
  through `IArenaRunHudHandler`.
