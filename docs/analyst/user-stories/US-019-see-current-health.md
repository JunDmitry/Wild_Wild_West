# US-019 — I can see my current health

- **Feature:** F-005 — Player health and death
- **Actor:** player
- **Priority:** P1 (from `docs/product/backlog.md`)
- **Rules:** `BR-022`, `BR-026`

## Story

As a player, I want to see my current health, so that I know how much danger
I am in.

## Examples

**E1 — health is observable during the run**
- Given the player is Alive,
- when the player looks for their current Health,
- then the current value is observable.

**E2 — the observation updates with damage**
- Given the observed Health is H,
- when Damage is applied and the current Health becomes H − D,
- then the observed Health shows H − D as soon as the damage is applied.

**E3 — the observed value never goes below zero**
- Given the player's current Health reaches zero,
- when the player observes their Health,
- then the observed value is zero.

## Notes

How the health is presented is a design matter. Healing and any
regeneration are out of scope.
