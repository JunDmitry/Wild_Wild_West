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
- **R7** — the melee attack is visible as a swing in front of the player.
- **R8** — a melee strike that damages an enemy is visibly confirmed at the moment of the hit.

## Acceptance criteria

- **A1** — with melee equipped, the attack input damages an enemy standing in front within reach.
- **A2** — the same attack leaves an enemy behind the player undamaged.
- **A3** — the same attack leaves an enemy beyond reach undamaged.
- **A4** — with the firearm equipped, the attack input does not perform a melee attack.
- **A5** — a dead player cannot perform a melee attack.
- **A6** — performing a melee attack produces a visible swing in front of the player.
- **A7** — a swing that damages an enemy produces a visible hit confirmation; a swing that damages nothing produces none.

## Constraints

- Reach, arc, damage, and attack rate are tuning values, not product commitments.

## Out of scope

- Combos, blocking, parrying, staggering, or knockback.
- Multiple melee weapons or upgrades.
- Melee attack audio.

## Revisions

- 2026-10-23 — R7/R8 and A6/A7 added. The feature had no user-visible surface, so
  a swing at air and a swing at an enemy read the same. A visible swing and a
  landed-hit confirmation now distinguish them; melee animation moves out of
  "out of scope" to make this possible. Reach, arc, damage, and attack rate are
  still tuning values. Superseding request:
  `docs/design/product-inbox/F-004.md`.
