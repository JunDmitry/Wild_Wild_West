# US-014 — The swing damages enemies in front within reach

- **Feature:** F-004 — Melee attack
- **Actor:** player
- **Priority:** P1 (from `docs/product/backlog.md`)
- **Rules:** `BR-090`, `BR-091`, `BR-095`, `BR-098`, `BR-115`

## Story

As a player, I want the swing to damage every enemy within reach in front of
me, so that I can fight off enemies that crowd me at close quarters.

## Examples

**E1 — an enemy in front within reach is damaged**
- Given an Active enemy lies within Attack Range in front of the Player and
  has Health H,
- when the swing resolves at Attack Impact,
- then that enemy's Health decreases by the Melee Weapon's Damage,
- and the applied Damage equals the actual health reduction, with Health not
  falling below zero.

**E2 — every enemy in front within reach is damaged**
- Given two Active enemies both lie within Attack Range in front of the
  Player,
- when the swing resolves,
- then both enemies' Health decreases.

**E3 — an enemy is damaged once per swing**
- Given one Active enemy lies within Attack Range in front of the Player,
- when the swing resolves,
- then that enemy's Health decreases once, by one application of the Melee
  Weapon's Damage.

**E4 — impact uses the state at impact time**
- Given an enemy was within Attack Range in front of the Player at Attack
  Start,
- when that enemy has left that reach by the time the swing resolves,
- then the swing does not affect it and its Health is unchanged.

## Notes

In front of the Player means within the Melee Weapon's Attack Arc around the
Aim Direction. Reach, arc, and Damage are tuning values, not product
commitments.
