# F-003 — Firearm attack

- **Priority:** P1
- **State:** specified
- **Backlog:** `docs/product/backlog.md`
- **Handoff:** `docs/analyst/product-inbox/F-003.md`

## Problem

The firearm is the starting and main weapon, and the arena loop depends on ranged damage. Without a working firearm the player cannot clear waves at range and the brief's primary weapon does not exist.

## Requirements

- **R1** — while the firearm is equipped and the player is alive, pressing the left mouse button fires a shot.
- **R2** — the shot travels in the aim direction determined by the camera.
- **R3** — a shot that reaches an enemy reduces that enemy's health by the firearm's damage.
- **R4** — a shot that reaches no enemy leaves every health value unchanged.
- **R5** — the firearm can be fired repeatedly, limited only by its fire rate.
- **R6** — the firearm uses no ammunition; firing never requires or consumes a resource.
- **R7** — the attack is available only while the firearm is equipped and the player is alive.

## Acceptance criteria

- **A1** — pressing the left mouse button with the firearm equipped produces a shot.
- **A2** — a shot aimed at an enemy reduces that enemy's health.
- **A3** — repeated shots at the same enemy reduce its health each time until it is defeated.
- **A4** — a shot that misses every enemy leaves all health values unchanged.
- **A5** — no ammunition counter exists and no reload action is required to keep firing.
- **A6** — with the melee weapon equipped, the same input does not fire the firearm.
- **A7** — a dead player cannot fire.

## Constraints

- Damage, fire rate, and projectile behaviour are tuning values, not product commitments.

## Out of scope

- Ammunition, reloading, magazines, or weapon durability.
- Additional firearm types, upgrades, or attachments.
- Spread, recoil, critical hits, or aiming down sights.
- Projectile art and audio.
