# F-008 — Wave progression

- **Priority:** P1
- **State:** specified
- **Backlog:** `docs/product/backlog.md`
- **Handoff:** `docs/analyst/product-inbox/F-008.md`

## Problem

The arena is fought in waves, and the brief requires a new wave to start only after the previous one is cleared. Without wave progression the run has no structure, no difficulty curve, and no path to victory.

## Requirements

- **R1** — the game tracks a wave counter that starts at the first wave.
- **R2** — a wave is complete when all of its enemies, including its boss, are defeated.
- **R3** — the next wave begins only after the current wave is complete.
- **R4** — while a wave is in progress, no enemy belonging to a later wave enters the arena.
- **R5** — the wave counter advances when a new wave begins.
- **R6** — the run comprises three waves; the third is the final wave.
- **R7** — during a run, the current wave and the total number of waves are visible.
- **R8** — the beginning of each new wave is announced.

## Acceptance criteria

- **A1** — defeating the last remaining enemy of a wave leads to the next wave beginning.
- **A2** — while any enemy of the current wave is alive, no next-wave enemy enters the arena.
- **A3** — the wave counter increases by one when a new wave begins and not before.
- **A4** — completing the third wave is the last wave progression of the run; no fourth wave begins.
- **A5** — at any moment during a run, the displayed wave number matches the wave in progress and the total is shown as three.
- **A6** — when a new wave begins, an announcement is shown and the displayed wave number increases at the same time.

## Out of scope

- Endless mode, wave modifiers, or inter-wave menus and countdowns.
- Difficulty scaling rules between waves.

## Revisions

- 2026-10-23 — R7/R8 and A5/A6 added; "a visible wave-number display" leaves
  "out of scope". The player previously had no way to know which wave they were
  in or that a new one had begun. Wave rules, three-wave structure, and
  progression are unchanged. Superseding request:
  `docs/design/product-inbox/F-008.md`.
