# ADR-0012: Keep spawn scheduling in Application and spawn placement authority in Domain

- **Status:** Accepted
- **Date:** 2026-09-16
- **Authors:** project team
- **Reviewers:** none
- **Related ADRs:** [ADR-0001](ADR-0001-Independent_identity_per_type.md), [ADR-0006](ADR-0006-sequential-external-interaction-protocol.md), [ADR-0010](ADR-0010-correlate-domain-interactions-with-aggregate.md), [ADR-0011](ADR-0011-domain-authority-over-movement-resolution.md)

## Context

Enemies must spawn outside the arena and enter it. The legacy implementation spawned one enemy per tick as an implicit rule and allowed configuration without a boss. Spawn positions were produced externally without domain validation, and enemy identities were allocated speculatively.

## Decision

Every wave contains exactly one boss; wave configuration states only the count of regular enemies.

The Arena Run does not schedule spawns. The Application asks the Arena Run for a spawn; the Arena Run answers whether a spawn is due and which enemy kind it is, opening an interaction only then.

The Application allocates the EnemyId and selects the physical spawn point. The Arena Run accepts the resolution only when the position is on the ground height, outside the arena bounds, and within the configured spawn band, and when the EnemyId is neither None nor already active.

Rejected spawn resolutions have no side effects. A successful spawn advances Aggregate Revision once and produces one EnemySpawned domain event.

## Rationale

- Spawn cadence is a pacing policy, not a consistency rule.
- Identity allocation and level geometry belong outside the Domain, but the Domain must bound what it accepts.
- A mandatory boss removes a branch from the wave lifecycle.

## Consequences

### Positive

- Adapters cannot spawn enemies inside the arena or arbitrarily far away.
- Identity is allocated only for spawns the Domain actually requested.
- Pacing can change without touching the aggregate.

### Negative

- The Application owns spawn timing and must call the aggregate accordingly.
- The spawn band width becomes part of the adapter contract.

### Neutral / Risks

- A wave with zero regular enemies begins directly in boss combat.
- Batch spawning would require a new interaction kind.

## Alternatives

1. **Spawn one enemy per tick inside the Domain** — couples the aggregate to frame cadence.
   - Why not chosen: cadence is not a domain rule.
2. **Accept any spawn position** — allows adapters to bypass level design constraints.
   - Why not chosen: the Domain must retain authority over placement limits.
3. **Keep a HasBoss flag** — permits configurations violating the game rule.
   - Why not chosen: the rule is invariant.

## References

- [Vaughn Vernon, Implementing Domain-Driven Design](https://www.domainlanguage.com/ddd/reference/)