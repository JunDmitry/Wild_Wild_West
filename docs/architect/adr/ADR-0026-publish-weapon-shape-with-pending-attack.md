# ADR-0026: Publish the weapon's attack shape with the pending attack

- **Status:** Accepted
- **Date:** 2026-10-24
- **Authors:** architect-role
- **Reviewers:** none
- **Related ADRs:** [ADR-0019](ADR-0019-attack-arc-in-domain-contract.md), [ADR-0020](ADR-0020-presentation-domain-read-model-values.md), [ADR-0024](ADR-0024-application-owned-hud-model.md)

## Context

The melee swing is a Presentation projection of the published pending attack:
the tick contract says it is drawn from `PlayerSnapshot.HasPendingAttack` with
`PendingAttack.WeaponKind`, `StartedAt`, and `ImpactAt`, and redrawn from the
live View Direction while the attack is pending. What the contract does not
give it is the attack's **shape** — how far the swing reaches and how wide it
is — so `MeleeSwingView` invented its own: the serialized fields
`_reach = 2f` and `_arcDegrees = 360f`, exposed through public setters and used
both to draw the line and to answer `Contains`.

That shape is not Presentation's to own. `WeaponDefinition` carries `Range`
(`Distance`) and `Arc` (`AttackArc`) — ADR-0019 made the arc a Domain concept
precisely so the aggregate, not an adapter, enforces the hit region. When the
impact opens, `ArenaRun` copies exactly those two into
`PlayerAttackImpactRequest`. So the Domain already knows the shape it will
validate; the published pending attack is the only place in the read model
where that knowledge is missing. A view-local copy can drift from the catalog
without any test noticing, and a swing can then draw a region the impact does
not use.

The same duplication leaked into the PlayMode fixture, which set
`swingView.Reach` and `swingView.ArcDegrees` from a hardcoded pair rather than
from the weapon the harness had built.

## Decision

We will publish the weapon's attack shape with the pending attack, so the
swing draws the shape the Domain validates.

- `PendingPlayerAttack` (Domain, published) gains `Distance Range` and
  `AttackArc Arc`, validated like `PlayerAttackImpactRequest` (range non-zero,
  arc valid) and included in equality and hash.
- `Player.StartAttack` copies both from the `WeaponDefinition` it starts the
  attack with.
- `PlayerStateSnapshot` and `Application.ReadModels.PlayerSnapshot` pass the
  struct through unchanged; **no Application contract changes**.
- `MeleeSwingPresenter` hands `PendingAttack.Range` and `PendingAttack.Arc` to
  `MeleeSwingView.Show`. The view owns no tuning: the serialized `_reach` and
  `_arcDegrees` fields and their setters are removed, leaving only the values
  most recently presented.

Out of scope: scene and prefab wiring (T-8), swing animation or timing, the
ranged weapon's drawn shape, and any tuning of the catalog values.

## Rationale

- The owner publishes its own fact. The Domain builds the shape into the
  impact request already, so publishing it on the pending attack is the same
  fact on the state side of the start/impact split.
- One source of truth. `MeleeSwingView.Contains` and the drawn line use the
  very values the Domain will check, so the visual cannot claim a region the
  impact rejects.
- It mirrors `PlayerAttackImpactRequest`, which has carried `Range` and `Arc`
  since ADR-0019; no new vocabulary is introduced.
- It costs no Application type: `PlayerSnapshot` already carries
  `PendingPlayerAttack`, and Presentation may read Domain value types
  (ADR-0020).
- The leak was observed in practice: the view's defaults and the harness's
  weapon agreed only by coincidence and by a hand-copied constant.

## Consequences

### Positive

- The drawn swing and the validated hit region share one value.
- Presentation holds no gameplay tuning; the View's serialized inspector fields
  can no longer silently override the weapon.
- Domain tests cover the published shape without Unity.

### Negative

- `PendingPlayerAttack`'s public constructor changes, so every construction
  site passes the pair.
- The Unity test assembly does not compile until the Presentation and PlayMode
  part lands (the coder handoff `melee-swing-shape`); the standalone Domain and
  Application suites stay green.

### Neutral / Risks

- The shape is captured at Attack Start. Weapon switching is refused while an
  attack is pending, so the start-time and impact-time shape agree by
  construction; if that rule ever changes, the impact must re-read the shape
  and this ADR is the place to revisit.
- The pending attack grows by two fields; `default(PendingPlayerAttack)` still
  means "no attack" and is never validated.

## Alternatives

1. **Application projects the shape into `PlayerSnapshot`** — the mapper takes
   the `WeaponCatalog` and adds shape fields to the read model.
   - Why not chosen: the mapper would need the catalog at every construction
     site, and the published shape would describe the *selected weapon* while
     the swing reads the pending attack's weapon kind — two facts that can
     drift apart.
2. **Composition injects the definition into the presenter** — the presenter
   receives `WeaponDefinition`/`WeaponCatalog` at wiring time.
   - Why not chosen: Presentation would reference Domain configuration, which
     ADR-0020 does not allow, and composition (T-8) is not wired, so nothing
     would supply it today and the fix could not be verified.
3. **Keep the fields and treat them as view tuning** — status quo.
   - Why not chosen: that is the leak being fixed; the visual can disagree with
     the Domain's hit region and nothing detects it.

## Compliance with paradigm

- ADR-0007: `Distance` and `AttackArc` are pure Domain value types; no Unity
  type enters Domain or Application.
- ADR-0019: the arc stays a Domain concept and now travels with the pending
  attack as well as the impact request.
- ADR-0020: Presentation only reads the published value types; it gains no new
  dependency and no gameplay rule.
- ADR-0024: unaffected — this is not HUD copy and adds no formatted model.
- `docs/architect/paradigm.md`: no deviation; no exception is added.

## Migration plan

1. Record this ADR.
2. Add `Range` and `Arc` to `PendingPlayerAttack`, with validation, equality,
   and hash; update the Domain construction sites.
3. Fill both in `Player.StartAttack` from the `WeaponDefinition`.
4. Add coverage that the published pending attack carries the definition's
   shape; keep the standalone suites green.
5. Update `application-tick-contract.md` § Tick Result.
6. Hand the Presentation rewiring and the PlayMode fixture to `coder-role`
   (`docs/coder/architect-inbox/melee-swing-shape.md`).

Steps 1–5 leave the standalone build and tests green. The Unity test assembly
is red between step 3 and the coder's step 6, which is expected and stated in
the handoff.

## References

- `Assets/_Project/Contexts/Arena/Presentation/Combat/MeleeSwingView.cs`
- `Assets/_Project/Contexts/Arena/Presentation/Combat/MeleeSwingPresenter.cs`
- `Assets/_Project/Contexts/Arena/Domain/Combat/PendingPlayerAttack.cs`
- `Assets/_Project/Contexts/Arena/Domain/Aggregates/ArenaRun/Player.cs`
- `Assets/_Project/Contexts/Arena/Domain/Aggregates/ArenaRun/ArenaRun.cs`
- `Assets/_Project/Contexts/Arena/Domain/Interactions/Attack/PlayerAttackImpactRequest.cs`
- `docs/architect/architecture/arena-combat/application-tick-contract.md` § Tick Result
- `docs/architect/tasks/melee-swing-shape.md`
