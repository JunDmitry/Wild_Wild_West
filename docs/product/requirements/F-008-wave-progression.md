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

## Acceptance criteria

- **A1** — defeating the last remaining enemy of a wave leads to the next wave beginning.
- **A2** — while any enemy of the current wave is alive, no next-wave enemy enters the arena.
- **A3** — the wave counter increases by one when a new wave begins and not before.
- **A4** — completing the third wave is the last wave progression of the run; no fourth wave begins.

## Out of scope

- Endless mode, wave modifiers, or inter-wave menus and countdowns.
- Difficulty scaling rules between waves.
- A visible wave-number display.
