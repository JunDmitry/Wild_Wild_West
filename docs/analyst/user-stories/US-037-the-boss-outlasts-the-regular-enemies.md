# US-037 — The boss outlasts the regular enemies

- **Feature:** F-009 — Boss enemy each wave
- **Actor:** player
- **Priority:** P1 (from `docs/product/backlog.md`)
- **Rules:** `BR-063`, `BR-041`

## Story

As a player, I want the boss to withstand far more damage than any regular
enemy, so that the wave's closing fight feels like its hardest fight rather than
another routine kill.

## Examples

**E1 — more health than any regular enemy**
- Given any Regular Enemy in the run and the run's Boss,
- when their Health capacities are compared,
- then the Boss's maximum Health is greater than that Regular Enemy's.

**E2 — health stays within bounds**
- Given the Boss has Health remaining,
- when Damage is applied to it,
- then its Health never exceeds its maximum and never falls below zero.

**E3 — the boss falls only at zero**
- Given the Boss has Health remaining,
- when a player Attack lands,
- then the Boss remains Active unless that Attack's Damage brings its Health to
  zero.

## Notes

The boss's exact health is a tuning value. A boss health bar or any
boss-specific user interface is out of scope, so whether the boss is close to
defeat is learned from the fight itself.
