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
- **R8** — while the firearm can be fired, the view shows where a shot would travel.
- **R9** — a shot that lands on an enemy is visibly confirmed at the moment of the hit.

## Acceptance criteria

- **A1** — pressing the left mouse button with the firearm equipped produces a shot.
- **A2** — a shot aimed at an enemy reduces that enemy's health.
- **A3** — repeated shots at the same enemy reduce its health each time until it is defeated.
- **A4** — a shot that misses every enemy leaves all health values unchanged.
- **A5** — no ammunition counter exists and no reload action is required to keep firing.
- **A6** — with the melee weapon equipped, the same input does not fire the firearm.
- **A7** — a dead player cannot fire.
- **A8** — with the firearm equipped and the run active, an aiming marker is visible showing the direction a shot would travel.
- **A9** — a shot that damages an enemy produces a visible hit confirmation; a shot that damages nothing produces none.

## Constraints

- Damage, fire rate, and projectile behaviour are tuning values, not product commitments.

## Out of scope

- Ammunition, reloading, magazines, or weapon durability.
- Additional firearm types, upgrades, or attachments.
- Spread, recoil, critical hits, or aiming down sights.

## Revisions

- 2026-10-23 — R8/R9 and A8/A9 added. The feature had no user-visible surface:
  the player inferred aim from the camera and hits from enemy health. A marker
  now shows the shot's direction and a landed hit is confirmed. Hit rules,
  fire rate, and damage are unchanged. Superseding request:
  `docs/design/product-inbox/F-003.md`.
- Projectile art and audio.
