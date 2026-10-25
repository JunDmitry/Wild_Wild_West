# ADR-0028: Register live enemy colliders from view lifecycle callbacks

- **Status:** Accepted
- **Date:** 2026-10-25
- **Authors:** architect-role
- **Reviewers:** none
- **Related ADRs:** [ADR-0007](ADR-0007-unity-isolation-and-layer-dependencies.md), [ADR-0018](ADR-0018-playmode-test-assembly.md), [ADR-0020](ADR-0020-presentation-domain-read-model-values.md)

## Context

`UnityPlayerAttackTargetingResolver` turns physics hits into `EnemyId` values through
`IEnemyColliderRegistry`, an Infrastructure lookup contract introduced with T-6.4
(`bool TryGetEnemyId(Collider, out EnemyId)`). The contract has no production
implementation: every PlayMode fixture supplies a fake whose collider map the test
builds by hand, so nothing in the game keeps that map current.

The colliders themselves exist only on live enemy views. `EnemyViewRegistry` in
Presentation creates a view when an enemy becomes renderable and destroys it when the
enemy leaves the snapshot or the run changes; it owns the complete view lifetime. But
Presentation's assembly references Application and Domain only, and Infrastructure's
references Application and Domain only, so neither side may name the other. A collider
cannot cross an Application port either: Application may not reference `UnityEngine`.

The delivery plan makes the gap explicit: T-6.8 requires "an `IEnemyColliderRegistry`
implementation populated from live enemy views; an unknown collider resolves to
`EnemyId.None`; a PlayMode test covers registration, unregistration, and lookup". T-8
composition is where the two layers will meet.

## Decision

We will expose the existing view lifetime as two Presentation callbacks and let the
composition root subscribe an Infrastructure registry to them.

- `EnemyView` exposes `TargetCollider` — a serialized `Collider` with a
  `GetComponent<Collider>()` fallback — so a prefab decides which collider represents
  the enemy.
- `EnemyViewRegistry` raises `ViewCreated(EnemyId, Collider)` when it instantiates and
  registers a new view, and `ViewDestroyed(EnemyId)` immediately before it destroys a
  view in `RemoveUnpresented` and `ClearAll`. The callbacks follow the same lifetime the
  registry already manages; no polling and no object search are introduced.
- `EnemyViewRegistry` raises `ViewCreated` only when the view exposes a collider. A
  collider-less view is therefore not targetable, which is a legitimate degraded state,
  not an error. One collider per enemy view is the contract; multi-collider views are out
  of scope.
- `IEnemyColliderRegistration` (`Infrastructure/Targeting`) is the registration surface:
  `Register(EnemyId, Collider)` and `Unregister(EnemyId)`. `EnemyColliderRegistry`
  implements it together with `IEnemyColliderRegistry`, keeping both directions of the
  map (collider → enemy, enemy → collider). `TryGetEnemyId` returns `false` and
  `EnemyId.None` for an unknown, removed, or destroyed collider.
- The composition root (T-8) is the only place that subscribes the callbacks:
  `ViewCreated += registry.Register` and `ViewDestroyed += registry.Unregister`. The
  PlayMode fixture subscribes the same pair directly, proving the seam without T-8.
- Presentation and Infrastructure gain no new assembly reference. `System.Action` and
  `UnityEngine.Collider` are available to Presentation already; `EnemyId` is a Domain
  value type available to both.

## Rationale

- The registry is a Unity-physics detail consumed only by an Infrastructure adapter. It
  does not belong in Application, and Application has nothing Unity-free to say about a
  collider.
- `EnemyViewRegistry` already knows exactly when a view appears and disappears. Reusing
  that lifetime keeps registration and rendering in step, so a defeated or out-of-bounds
  enemy stops being targetable at the same moment its view dies.
- The composition root is the designed meeting point for layers that may not reference
  each other (ADR-0007, ADR-0014). Putting the subscription there adds no new edge to
  the dependency graph.
- Deterministic unregistration on `Destroy` is testable with a PlayMode fixture: create
  a view, look it up, remove the enemy, look it up again.

## Consequences

### Positive

- `IEnemyColliderRegistry` finally has a production implementation and the resolver can
  be composed with it in T-8.
- Registration follows view lifetime exactly; no sweep, no stale hidden views.
- Layer rules are untouched: Presentation still references Application and Domain only,
  Infrastructure still references Application and Domain only.
- The PlayMode fixture exercises the real seam, not a hand-built fake.

### Negative

- `EnemyViewRegistry`'s public surface grows two events that exist for an Infrastructure
  consumer; Presentation does not use them itself.
- Production wiring does not exist until T-8. Until then the seam is exercised only by
  the fixture, and an unsubscribed `EnemyViewRegistry` yields an empty registry (events
  are null by default).
- A prefab without a collider silently produces no target instead of failing loudly.

### Neutral / Risks

- The registry keeps two maps (collider → enemy, enemy → collider); `Unregister` must
  remove both, and `Register` for an already-known enemy replaces its previous collider.
- A Unity `Collider` that has been destroyed still compares equal to `null`; the registry
  treats such a collider as unknown even before `Unregister` runs.
- Callbacks are raised on the Unity main thread during presentation, the same thread as
  the tick; no threading contract changes.

## Alternatives

1. **Application registration port with an opaque collider handle** — Application defines
   `IEnemyColliderRegistration` with `int` handles, Infrastructure implements it,
   Presentation calls it through Application.
   - Why not chosen: Application gains a port it never uses, carrying a Unity instance id
     with no meaning in that layer; instance ids can be reused after destruction, so the
     registry must guard them.
2. **Infrastructure discovery through an Application identity interface** — `EnemyView`
   implements an Application-defined `IEnemyIdentity`; the registry finds views by object
   search and reads their colliders.
   - Why not chosen: discovery by search is the global object search T-8.1 forbids for
     services, runs on a schedule rather than on the lifetime edge, and leaves
     hidden-but-live views registered.

## Compliance with paradigm

- ADR-0007: no assembly reference is added in either direction. The only code that sees
  both layers is the composition root, as designed.
- ADR-0018: the PlayMode test assembly already references Application, Infrastructure,
  and Presentation, so it can subscribe the seam.
- ADR-0020: unchanged; the callback carries Unity and Domain types only.
- `docs/architect/paradigm.md`: no deviation; no exception is added.

## Migration plan

1. Record this ADR and index it.
2. Add `EnemyView.TargetCollider` and the two `EnemyViewRegistry` callbacks.
3. Add `IEnemyColliderRegistration`.
4. Hand `EnemyColliderRegistry` and its PlayMode fixture to `coder-role`.
5. T-8 subscribes the callbacks in the composition root.

## References

- `docs/coder/delivery-plan.md` § T-6.8
- `Assets/_Project/Contexts/Arena/Infrastructure/Targeting/IEnemyColliderRegistry.cs`
- `Assets/_Project/Contexts/Arena/Infrastructure/Targeting/UnityPlayerAttackTargetingResolver.cs`
- `Assets/_Project/Contexts/Arena/Presentation/Enemies/EnemyView.cs`
- `Assets/_Project/Contexts/Arena/Presentation/Enemies/EnemyViewRegistry.cs`
- `Assets/_Project/Contexts/Arena/Tests/GameArenaPlayMode/PlayModeTickHarness.cs`
