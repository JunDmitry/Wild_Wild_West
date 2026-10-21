# US-016 — Every run starts at full health

- **Feature:** F-005 — Player health and death
- **Actor:** player
- **Priority:** P1 (from `docs/product/backlog.md`)
- **Rules:** `BR-020`, `BR-021`

## Story

As a player, I want every run to start at full health, so that each attempt
begins with the same chance.

## Examples

**E1 — a new run starts full**
- Given a new ArenaRun begins,
- when the player observes their current Health,
- then it equals the player's maximum Health.

**E2 — health never exceeds its maximum**
- Given the player's current Health is at its maximum,
- when the run continues without Damage,
- then the current Health stays at the maximum and never rises above it.

**E3 — a run does not begin weakened**
- Given the previous ArenaRun ended with the player Defeated,
- when a new ArenaRun begins,
- then the player is Alive at full Health.

## Notes

The player's maximum Health is a tuning value, not a product commitment.
