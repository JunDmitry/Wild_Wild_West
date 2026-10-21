# F-005 — Player health and death

- **Priority:** P1
- **State:** specified
- **Backlog:** `docs/product/backlog.md`
- **Handoff:** `docs/analyst/product-inbox/F-005.md`, `docs/design/product-inbox/F-005.md`

## Problem

The brief requires the player to lose health when damaged and die at zero health, which is the precondition for the defeat-and-restart loop. Without it the player is indestructible and the run has no failure state.

## Requirements

- **R1** — the player has a current health value and a maximum health value, and every run starts at full health.
- **R2** — damage received from an enemy attack reduces current health by that attack's damage.
- **R3** — current health never falls below zero.
- **R4** — when current health reaches zero the player dies and can no longer act.
- **R5** — the player can observe their current health, and it updates when damage is taken.
- **R6** — a dead player remains dead until the run restarts.

## Acceptance criteria

- **A1** — when an enemy attack lands, the player's displayed health decreases by that attack's damage.
- **A2** — enough enemy hits reduce health to zero; a further hit does not make it negative.
- **A3** — at zero health the player cannot move, switch weapons, or attack.
- **A4** — current health is visible to the player and changes as soon as damage lands.
- **A5** — a newly started run begins at full health.

## Out of scope

- Healing, health pickups, armor, shields, or damage-over-time effects.
- Invulnerability frames or dodge mechanics.
- Death animation timing and death audio.
- Respawning without a scene restart (that is F-010).
