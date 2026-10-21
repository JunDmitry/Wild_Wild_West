# US-015 — The swing spares enemies outside reach or behind the player

- **Feature:** F-004 — Melee attack
- **Actor:** player
- **Priority:** P1 (from `docs/product/backlog.md`)
- **Rules:** `BR-116`

## Story

As a player, I want the swing to leave enemies beyond my reach or behind me
unharmed, so that I can rely on melee striking only what is actually in front
of me.

## Examples

**E1 — an enemy behind the player is unharmed**
- Given an Active enemy lies outside the Melee Weapon's Attack Arc, behind
  the Player,
- when the swing resolves at Attack Impact,
- then that enemy's Health is unchanged.

**E2 — an enemy beyond reach is unharmed**
- Given an Active enemy lies along the Aim Direction but beyond Attack Range,
- when the swing resolves,
- then that enemy's Health is unchanged.

**E3 — only the enemy in front is damaged**
- Given one Active enemy lies within Attack Range in front of the Player and
  another lies behind the Player,
- when the swing resolves,
- then only the enemy in front loses Health.

## Notes

Reach and arc are tuning values, not product commitments.
