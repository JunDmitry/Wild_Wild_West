# F-009 — Boss enemy each wave

- **Priority:** P1
- **State:** specified
- **Backlog:** `docs/product/backlog.md`
- **Handoff:** `docs/analyst/product-inbox/F-009.md`

## Problem

The brief requires a boss — an enemy with a large amount of health — at the end of every wave. Without it each wave ends flat, and the victory condition (defeating the boss in each wave) cannot be met.

## Requirements

- **R1** — after all regular enemies of a wave are defeated, a boss enemy enters the arena from outside it.
- **R2** — the boss has substantially more health than any regular enemy.
- **R3** — the boss pursues and attacks the player like other enemies; its difference from them is its larger health.
- **R4** — the wave is complete only when the boss is defeated.
- **R5** — each of the three waves ends with exactly one boss.
- **R6** — the boss's entry is marked distinctly from a regular enemy's.
- **R7** — while the boss is alive, its remaining health is visible.

## Acceptance criteria

- **A1** — no boss appears while any regular enemy of the wave is alive.
- **A2** — when the last regular enemy of a wave is defeated, a boss enters the arena from outside it.
- **A3** — the boss's health is greater than that of any regular enemy in the run.
- **A4** — defeating the boss completes the wave, and the next wave then begins.
- **A5** — defeating the third wave's boss ends the run in victory.
- **A6** — the boss's entry is distinguishable from a regular enemy's entry.
- **A7** — while the boss is alive, an indicator shows its remaining health, it decreases as the boss takes damage, and it disappears when the boss is defeated.

## Constraints

- The boss's exact health, size, and attack values are tuning values.

## Out of scope

- Boss-specific attacks, phases, or mechanics.
- More than one boss type or multiple bosses per wave.
- Boss music and arena changes.

## Revisions

- 2026-10-23 — R6/R7 and A6/A7 added; "a boss health bar or boss-specific UI"
  leaves "out of scope". The boss was visually a larger enemy with no progress
  read, so a long fight gave no sense of how close it was. Boss health, pursuit,
  attack, and the wave-completion rule are unchanged. Superseding request:
  `docs/design/product-inbox/F-009.md`.
