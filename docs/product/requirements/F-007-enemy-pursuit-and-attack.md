# F-007 — Enemy pursuit and close-range attack

- **Priority:** P1
- **State:** specified
- **Backlog:** `docs/product/backlog.md`
- **Handoff:** `docs/analyst/product-inbox/F-007.md`

## Problem

Enemies must threaten the player. Without pursuit they never reach the player; without an attack they are harmless targets, and the health, defeat, and wave-completion mechanics never trigger.

## Requirements

- **R1** — every living enemy moves toward the player while the run is active.
- **R2** — when an enemy is within attack range, it attacks the player instead of closing further.
- **R3** — a landed enemy attack reduces the player's health by that attack's damage.
- **R4** — an enemy outside attack range causes no damage to the player.
- **R5** — an enemy keeps pursuing and attacking until it is defeated or the run ends.
- **R6** — an enemy's attack is preceded by a visible cue, so the player can see the threat before damage lands.

## Acceptance criteria

- **A1** — an enemy farther than attack range approaches the player.
- **A2** — once within attack range, the enemy attacks.
- **A3** — a landed enemy attack reduces the player's health by the attack's damage.
- **A4** — an enemy beyond attack range causes no health loss.
- **A5** — after the player moves away, the enemy resumes pursuit and attacks again when in range.
- **A6** — when an enemy begins an attack, a visible cue precedes the moment the player's health is reduced.

## Constraints

- Attack range, damage, attack rate, and movement speed are tuning values.
- The cue's duration is a tuning value; the ordering — cue visible before damage lands — is fixed.

## Out of scope

- Enemy variety, ranged enemies, flanking, cover use, or patrol behaviour.
- Dodging, blocking, or parrying by the player.
- Pathfinding around obstacles beyond reaching the player in an open arena.
- Special attacks or additional enemy behaviours.

## Revisions

- 2026-10-23 — R6 and A6 added; the constraint now fixes the cue-before-damage
  ordering and leaves its duration as tuning, and "telegraphed wind-ups" leaves
  "out of scope". Without a cue an enemy attack was unreadable until health
  dropped. Because the ordering is a rule and not only a visual, this also
  revises the analysis: `docs/analyst/product-inbox/F-007b.md`. Design request:
  `docs/design/product-inbox/F-007.md`.
