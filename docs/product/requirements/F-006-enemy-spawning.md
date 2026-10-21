# F-006 — Enemy spawning outside the arena, unseen

- **Priority:** P1
- **State:** specified
- **Backlog:** `docs/product/backlog.md`
- **Handoff:** `docs/analyst/product-inbox/F-006.md`, `docs/design/product-inbox/F-006.md`

## Problem

Enemies must enter the closed arena from beyond its edges, and the player must never see one appear. Without this, enemies either pop into view inside the arena or the arena stops feeling like a space under siege.

## Requirements

- **R1** — enemies enter the arena from positions outside its bounds and move inward.
- **R2** — an enemy's moment of appearing is outside the player's current view; the player never observes an enemy materialize on screen.
- **R3** — the enemies of a wave enter the arena during that wave, according to the wave's spawn pacing.
- **R4** — every enemy that has entered is alive and counts as part of the active wave.
- **R5** — enemies enter only while a wave is active; none enter after the run has ended.

## Acceptance criteria

- **A1** — enemies appear inside the arena only after coming in from beyond its boundary.
- **A2** — with the camera aimed at the arena, the player never sees an enemy appear within the view; enemies become visible only after entering from outside it.
- **A3** — each enemy that enters counts toward the current wave's remaining enemies.
- **A4** — no enemy enters the arena after the player has died or the run has been won.

## Constraints

- Enemy count, spawn positions, and pacing are tuning values, not product commitments.

## Out of scope

- Spawn effects, telegraphs, or audio.
- Exact waves' enemy counts and composition.
- Difficulty scaling between waves.
- Different enemy types.
