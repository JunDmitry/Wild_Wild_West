# ADR-0019: Model the melee Attack Arc in the Domain contract

- **Status:** Accepted
- **Date:** 2026-10-22
- **Authors:** architect-role
- **Reviewers:** none
- **Related ADRs:** [ADR-0011](ADR-0011-domain-authority-over-movement-resolution.md), [ADR-0013](ADR-0013-stage-attacks-into-start-and-impact.md), [ADR-0017](ADR-0017-view-relative-player-input.md)

## Context

Feature F-004 requires the melee attack to hit every Enemy in front of the
player and never an Enemy outside the Attack Arc (BR-115, BR-116). The melee
attack already reuses the shared attack lifecycle: weapon kind and catalog,
cooldown and windup, a single pending attack, one impact request per attack,
range validation, once-per-Enemy dedupe, and no resource consumption.

What the architecture lacked was the arc itself. `WeaponDefinition` carried
kind, damage, range, cooldown, and windup only; `PlayerAttackImpactRequest`
carried origin, aim direction, and range. `ApplyPlayerAttackImpact` therefore
accepted any Enemy within the weapon range, including Enemies beside or behind
the player, so the Domain could not enforce BR-116 and the future Unity melee
resolver would have been the only place the arc existed.

The Aim Direction is already the horizontal View Direction (ADR-0017), so the
arc is a planar cone around a direction the Domain already receives.

## Decision

The Attack Arc is a Domain concept.

- `AttackArc` (in `Game.Arena.Domain.Combat`) is a validated value object in
  degrees, `0 < Degrees <= 360`, with `AttackArc.Full` as the unrestricted
  default. It exposes the cosine of its half angle for comparisons.
- `WeaponDefinition` carries `Arc`. The existing constructor keeps working and
  delegates to an overload that defaults the arc to `AttackArc.Full`, so
  existing catalogs and the ranged weapon are unchanged.
- `PlayerAttackImpactRequest` carries `Arc`, copied from the equipped weapon
  when the request is opened.
- `ApplyPlayerAttackImpact` validates every target against the request range
  **and** the request arc. A target outside the arc is rejected with the new
  `PlayerAttackImpactRejectionReason.TargetOutsideArc`; the interaction stays
  open, exactly as range rejection already behaves, so the adapter can retry.
- The arc check compares the normalized horizontal offset from the request
  origin against the cosine of the half angle, with a small
  `GeometryTolerance.AttackArcTolerance`. A zero-length horizontal offset (an
  Enemy stacked on the player) is treated as inside.
- The Unity adapter still owns target selection: it returns every Enemy in
  range and arc, each once. The Domain re-validates and dedupes, so a faulty
  adapter cannot violate the invariant.

Out of scope: vertical arc (the arc is horizontal, matching the aim
direction), per-attack animation timing, and any tuning of arc values — the
catalog supplies them.

## Rationale

- BR-116 has two halves, range and arc. Enforcing range in the aggregate and
  the arc only in an adapter would leave the invariant half-owned by a
  replaceable adapter.
- Weapon tuning already lives in the Domain-owned catalog; if the arc lived
  only in the adapter, the weapon's shape would be split across layers.
- The change preserves every existing call site: the default full arc keeps
  the current ranged behaviour identical, and the standalone Domain and
  Application suites stay green.

## Consequences

### Positive

- The melee invariant is enforced by the aggregate and covered by Domain
  tests without Unity.
- The adapter's job is narrowed to geometric selection, which it can test in
  PlayMode without duplicating rules.
- Future weapons (cone sweeps, cleaves) get arc configuration for free.

### Negative

- The public `PlayerAttackImpactRequest` constructor changes; every
  construction site must pass the arc.
- One more rejection reason for consumers to handle.

### Neutral / Risks

- Ranged weapons keep a full arc; whether ranged should also narrow is a
  future decision, not implied here.
- Because the arc is horizontal, a target directly above or below the player
  is judged by its horizontal offset only; the ground-plane model makes that
  consistent with movement and range.

## Alternatives

1. **Arc in the request; adapter filters; Domain validates range only** —
   smaller Domain change, mirrors the ranged split.
   - Why not chosen: BR-116's arc half would be enforced only by the adapter.
2. **Arc only in the adapter, no contract change** —
   - Why not chosen: weapon shape would leave the Domain, and the aggregate
     could not enforce the invariant at all.

## Compliance with paradigm

- ADR-0007: `AttackArc` is a pure value type in the Domain assembly; no Unity
  type is introduced.
- ADR-0011: the Domain remains the authority over accepted interactions; the
  adapter still only proposes targets.
- ADR-0013: the arc travels with the impact request, so the start/impact split
  is unchanged.
- `docs/architect/paradigm.md`: no deviation; no exception is added.

## Migration plan

1. Add `AttackArc` and its tolerance.
2. Add the arc parameter to `WeaponDefinition` as an overload with a full-arc
   default.
3. Add `Arc` to `PlayerAttackImpactRequest` and pass the equipped weapon's arc.
4. Validate the arc in `ApplyPlayerAttackImpact`; add `TargetOutsideArc`.
5. Update `application-tick-contract.md` § External Resolution Boundary.
6. Extend Domain tests (arc accept/reject, full-arc default) and run the
   standalone suites.
7. Hand the Unity melee resolver and its PlayMode tests to `coder-role`
   (`docs/coder/architect-inbox/F-004.md`).

Each step leaves the standalone build and tests green.

## References

- `docs/architect/analyst-inbox/F-004.md` — US-006, BR-115, BR-116
- `docs/architect/architecture/arena-combat/application-tick-contract.md`
- `Assets/_Project/Contexts/Arena/Domain/Combat/AttackArc.cs`
- `Assets/_Project/Contexts/Arena/Domain/Aggregates/ArenaRun/ArenaRun.cs`
- `docs/coder/delivery-plan.md` — T-6.5 (Unity melee attack resolver)
