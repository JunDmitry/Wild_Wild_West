# US-011 — A shot that reaches an enemy damages it

- **Feature:** F-003 — Firearm attack
- **Actor:** player
- **Priority:** P1 (from `docs/product/backlog.md`)
- **Rules:** `BR-090`, `BR-091`, `BR-095`, `BR-097`, `BR-112`

## Story

As a player, I want a shot that reaches an enemy to reduce that enemy's
health by the firearm's Damage, so that ranged attacks can defeat enemies.

## Examples

**E1 — a shot hits an enemy in range**
- Given an Active enemy lies along the shot's direction within Attack Range
  and has Health H,
- when the shot resolves at Attack Impact,
- then that enemy's Health decreases by the firearm's Damage,
- and the applied Damage equals the actual health reduction, with Health
  not falling below zero.

**E2 — the nearest enemy along the shot is affected**
- Given two Active enemies lie along the shot's direction within Attack
  Range,
- when the shot resolves,
- then only the nearer enemy's Health decreases.

**E3 — impact uses the state at impact time**
- Given an enemy was within Attack Range at Attack Start,
- when that enemy has moved outside Attack Range by the time the shot
  resolves,
- then the shot affects no enemy and no Health decreases.

## Notes

The firearm's damage value is a tuning value, not a product commitment.
