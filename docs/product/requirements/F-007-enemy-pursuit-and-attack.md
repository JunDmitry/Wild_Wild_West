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

## Acceptance criteria

- **A1** — an enemy farther than attack range approaches the player.
- **A2** — once within attack range, the enemy attacks.
- **A3** — a landed enemy attack reduces the player's health by the attack's damage.
- **A4** — an enemy beyond attack range causes no health loss.
- **A5** — after the player moves away, the enemy resumes pursuit and attacks again when in range.

## Constraints

- Attack range, damage, attack rate, and movement speed are tuning values.
- Enemy attack timing and telegraphs are not fixed by this feature.

## Out of scope

- Enemy variety, ranged enemies, flanking, cover use, or patrol behaviour.
- Dodging, blocking, or parrying by the player.
- Pathfinding around obstacles beyond reaching the player in an open arena.
- Special attacks or telegraphed wind-ups.
