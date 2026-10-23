# F-010 — Defeat and scene restart

- **Priority:** P1
- **State:** specified
- **Backlog:** `docs/product/backlog.md`
- **Handoff:** `docs/analyst/product-inbox/F-010.md`

## Problem

The brief requires the scene to restart and the wave counter to reset after the player's death. Without a restart the death state is terminal and the game cannot be played again without leaving it.

## Requirements

- **R1** — when the player's health reaches zero, the run ends in defeat.
- **R2** — the scene restarts automatically after defeat, without a menu or confirmation.
- **R3** — the restarted scene begins a fresh run at the first wave.
- **R4** — on restart the player has full health and the firearm equipped.
- **R5** — every enemy and every run state from the previous attempt is gone after the restart.
- **R6** — before the scene restarts, the player is told the run ended in defeat.
- **R7** — the defeat message names the wave the player had reached.

## Acceptance criteria

- **A1** — when the player's health reaches zero, the scene restarts without player input.
- **A2** — after the restart, the run begins at the first wave.
- **A3** — after the restart, the player has full health and the firearm equipped.
- **A4** — no enemy from the previous attempt exists after the restart.
- **A5** — no input or confirmation is required between death and the restarted run.
- **A6** — when the player's health reaches zero, a defeat message is shown before the scene restarts.
- **A7** — the defeat message names the wave the player had reached when the run ended.

## Constraints

- Any delay between death and restart is a tuning value.
- The defeat message is shown within that delay; it never waits for the player.

## Out of scope

- Score summary, statistics, or rewards.
- Save/load or persistence of the failed run.
- Manual restart or a restart menu.

## Revisions

- 2026-10-23 — R6/R7 and A6/A7 added; "a defeat screen or defeat message"
  leaves "out of scope". A5 is corrected: it previously forbade any defeat
  surface between death and restart, which the new message would have
  contradicted — it now forbids an input or confirmation, which remains true.
  The restart stays automatic and the wave counter still resets; the message
  names the wave reached, which reads F-008's counter. Superseding request:
  `docs/design/product-inbox/F-010.md`.
