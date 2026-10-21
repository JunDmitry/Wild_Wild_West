# US-017 — Enemy attacks reduce my health

- **Feature:** F-005 — Player health and death
- **Actor:** player
- **Priority:** P1 (from `docs/product/backlog.md`)
- **Rules:** `BR-022`, `BR-046`, `BR-047`, `BR-091`

## Story

As a player, I want enemy attacks that land to reduce my health, so that
the fight carries real risk.

## Examples

**E1 — a landed attack reduces health by its damage**
- Given the player has current Health H,
- when an enemy attack lands with Damage D,
- then the player's current Health becomes H − D.

**E2 — health never goes below zero**
- Given the player has current Health H and a landed attack requests
  Damage D greater than H,
- when the attack lands,
- then the player's current Health becomes zero,
- and the applied Damage equals H, the actual health reduction.

**E3 — an attack that does not land changes nothing**
- Given an enemy is outside its attack range,
- when its attack would resolve,
- then the player's current Health is unchanged.

## Notes

Enemy Damage is a tuning value, not a product commitment. Healing,
armor, and damage-over-time effects are out of scope.
