# ADR-0024: Prepare HUD text in an Application-owned HUD model

- **Status:** Accepted
- **Date:** 2026-10-23
- **Authors:** architect-role
- **Reviewers:** none
- **Related ADRs:** [ADR-0007](ADR-0007-unity-isolation-and-layer-dependencies.md), [ADR-0020](ADR-0020-presentation-domain-read-model-values.md), [ADR-0022](ADR-0022-application-notifications-for-one-shot-facts.md)

## Context

The F-002 and F-005 designs extend the arena gameplay view (S-001) with two
persistent HUD regions: the equipped weapon ("Firearm" / "Melee Weapon") and
health ("Health 100 / 100" with a proportional fill, announced as
"Health: 60 of 100"). US-019 makes player health visible during the run.

ADR-0020 lets Presentation read snapshot values directly and format them in
the view. The first F-002 handoff followed that rule: the presenter would map
`WeaponKind` to a display name and read `PlayerSnapshot.Health`.

The user decided on 2026-10-23 that HUD copy and formatting belong to the
Application, not to the view: the Application prepares a HUD model that the
view binds to. This record supersedes ADR-0020's consequence "no new public
Application contract" for the HUD only; ADR-0020 stays in force for the other
presenters, such as `PlayerView` reading `Position3D`.

## Decision

`Game.Arena.Application` owns a presentation-ready HUD model, published with
the tick result:

- `Application/Hud/ArenaRunHudSnapshot` carries the HUD labels and formatted
  values: `HealthLabel`, `HealthCurrent`, `HealthMaximum`, `HealthText`
  ("current / maximum"), `HealthFraction` (0…1), `EquippedWeaponLabel`,
  `EquippedWeaponText` ("Firearm" / "Melee Weapon"), and `IsDefeated`.
- `Application/Hud/ArenaRunHudSnapshotMapper` builds it from
  `ArenaRunSnapshot`.
- `ArenaRunTickResult.Hud` publishes it next to `Snapshot`. The result derives
  `Hud` from `Snapshot` when it is constructed, so the two always describe one
  revision and cannot be mismatched by a caller.

Rules that come with the decision:

- Formatting is culture-invariant; an unknown `WeaponKind` throws rather than
  producing a blank label.
- Presentation binds the HUD regions to `ArenaRunTickResult.Hud` and does not
  format health or weapon values itself.
- Non-HUD presentation keeps reading `ArenaRunTickResult.Snapshot` under
  ADR-0020.
- Accessibility announcement templates stay in Presentation: the platform
  decides how a status region speaks. The labels and values the announcement
  uses come from the HUD model.

## Rationale

- One source of user-facing HUD copy, testable in the standalone Application
  suite without Unity.
- Presenters stay thin projections of published state, so wording cannot
  drift between the on-screen readout and the accessibility text.
- The aggregate read model stays as it is; the HUD model is a projection for
  one consumer, not a second aggregate snapshot.
- Having the tick result carry it guarantees the HUD and the run state share
  a revision, with no second mapping step in the view.

## Consequences

### Positive

- HUD text, the fill fraction, and the defeated flag are covered by
  Application unit tests, independent of PlayMode.
- The view has a single input for both HUD regions.
- Future localization has an obvious seam: the HUD model.

### Negative

- A new public Application contract on every tick result; the tick result now
  duplicates health and weapon values that `Snapshot` already carries.
- English display copy lives in Application until localization is designed.
- `WeaponKind` is mapped in Application, so a new weapon kind requires a HUD
  mapper change as well as a Domain change.

### Neutral / Risks

- The snapshot remains the aggregate read model; nothing in the aggregate or
  its mapper changes.
- Composition (T-8) forwards `Hud` together with `Snapshot`; no new port or
  notification is introduced (ADR-0022 is untouched).

## Alternatives

1. **Snapshot-only, formatting in Presentation** — ADR-0020's premise, and
   what the first F-002 handoff said.
   - Why not chosen: the user chose Application-owned HUD copy; formatting in
     the view spreads wording across Unity code and makes it untestable in the
     standalone suite.
2. **An Application formatting port supplied by composition** — a service the
   view calls with the snapshot.
   - Why not chosen: same outcome with more indirection; a per-tick model on
     the result is simpler to test and reason about.
3. **Notification-driven HUD** — publish weapon and health changes as
   notifications.
   - Why not chosen: both are state, not one-shot facts; ADR-0022 reserves
     notifications for facts a snapshot cannot carry.

## Compliance with paradigm

- ADR-0007: Application stays free of Unity and of presentation frameworks;
  the HUD model is plain data and strings.
- ADR-0020: refined, not reversed — Presentation may still read snapshot
  values; the HUD is simply not where it formats them.
- ADR-0022: unchanged; no notification is added for state.

## Migration plan

1. Add `ArenaRunHudSnapshot` and `ArenaRunHudSnapshotMapper` under
   `Application/Hud/`.
2. Add `Hud` to `ArenaRunTickResult`, derived from `Snapshot` in its
   constructor.
3. Cover the mapper, the result, and one end-to-end tick in the Application
   tests.
4. Update the tick contract § Tick Result and the orientation cache.
5. Hand the HUD binding to `coder-role`; the F-002 weapon-indicator handoff is
   superseded by `F-002b`.

## References

- `docs/architect/design-inbox/F-002.md`
- `docs/architect/design-inbox/F-005.md`
- `docs/architect/tasks/F-005.md`
- `Assets/_Project/Contexts/Arena/Application/ReadModels/ArenaRunSnapshot.cs`
- `Assets/_Project/Contexts/Arena/Application/Ticks/ArenaRunTickResult.cs`
