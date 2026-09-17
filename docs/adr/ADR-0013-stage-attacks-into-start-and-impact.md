# ADR-0013: Stage attacks into start and impact

- **Status:** Accepted
- **Date:** 2026-09-17
- **Authors:** project team
- **Reviewers:** none
- **Related ADRs:** [ADR-0006](ADR-0006-sequential-external-interaction-protocol.md), [ADR-0010](ADR-0010-correlate-domain-interactions-with-aggregate.md), [ADR-0011](ADR-0011-domain-authority-over-movement-resolution.md)

## Context

Combat attacks must support windup, animation anticipation, player evasion, enemy evasion, and target changes between the moment an attack starts and the moment it can deal damage. A single immediate damage operation would make animation timing either cosmetic only or a hidden source of gameplay truth.

The project also requires Unity physics queries to remain outside the Domain while the Domain keeps authority over attack readiness, range, valid targets, and aggregate consistency.

## Decision

An attack is modeled as two domain stages: Attack Start and Attack Impact.

Attack Start records the attacker, weapon or attack definition, StartedAt, ImpactAt, and advances combat readiness according to cooldown. Attack Start does not deal damage.

Attack Impact is resolved later against the current Arena Run state. The target hit by Attack Impact may differ from any target that allowed Attack Start.

Player Attack Impact uses an external interaction request and resolution. Enemy attack impact may be resolved internally when no external query is needed.

A valid miss is a successful Attack Impact with no damage. A rejected interaction resolution does not complete the attack.

Cooldown starts at Attack Start. A combatant cannot start another attack while it has a pending attack.

## Rationale

- Windup and impact are gameplay rules, not animation callbacks.
- The Domain remains the source of truth for attack timing and readiness.
- Players and enemies can move between attack start and impact.
- A miss is distinct from a protocol error.
- The model supports ranged and melee attacks without binding to Unity physics.

## Consequences

### Positive

- Animation can visualize attack preparation without owning damage timing.
- Evasion during windup is represented naturally.
- Cooldown semantics are explicit and testable.
- Player attack impact can use Unity physics through the sequential interaction protocol.
- Target changes between start and impact are allowed.

### Negative

- Attack processing requires more states and tests.
- A pending attack must be completed or cancelled by domain rules.
- Application orchestration must ask for impact only when the Domain says it is due.

### Neutral / Risks

- Large time steps resolve impact at the next processed domain step rather than reconstructing exact historical geometry.
- Direction locking for specific weapons is not part of the current model.
- A richer ability system may extend or specialize this attack model later.

## Alternatives

1. **Apply damage immediately at attack start** — does not support windup or evasion.
   - Why not chosen: attack timing would be too limited for the intended gameplay.
2. **Let animation events apply damage** — makes Presentation a gameplay authority.
   - Why not chosen: animation must visualize domain state, not control it.
3. **Keep the original target fixed from attack start** — prevents target changes during windup.
   - Why not chosen: the current simple attack model resolves targets at impact time.

## References

- [Vaughn Vernon, Implementing Domain-Driven Design](https://www.domainlanguage.com/ddd/reference/)