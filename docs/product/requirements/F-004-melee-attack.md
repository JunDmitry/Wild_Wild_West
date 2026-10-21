# F-004 — Melee attack

- **Priority:** P1
- **State:** specified
- **Backlog:** `docs/product/backlog.md`
- **Handoff:** `docs/analyst/product-inbox/F-004.md`

## Problem

The brief's second weapon is melee. Without a close-range attack the melee weapon is cosmetic, the Q switch leads to a dead end, and enemies that close distance cannot be fought in melee.

## Requirements

- **R1** — while the melee weapon is equipped and the player is alive, pressing the left mouse button performs a melee attack.
- **R2** — the attack damages enemies within close reach in front of the player.
- **R3** — enemies beyond that reach, or behind the player, take no damage from it.
- **R4** — the melee attack consumes no ammunition and has no resource cost.
- **R5** — the attack can be repeated, limited only by its attack rate.
- **R6** — the attack is available only while melee is equipped and the player is alive.

## Acceptance criteria

- **A1** — with melee equipped, the attack input damages an enemy standing in front within reach.
- **A2** — the same attack leaves an enemy behind the player undamaged.
- **A3** — the same attack leaves an enemy beyond reach undamaged.
- **A4** — with the firearm equipped, the attack input does not perform a melee attack.
- **A5** — a dead player cannot perform a melee attack.

## Constraints

- Reach, arc, damage, and attack rate are tuning values, not product commitments.

## Out of scope

- Combos, blocking, parrying, staggering, or knockback.
- Multiple melee weapons or upgrades.
- Melee animation and audio.
