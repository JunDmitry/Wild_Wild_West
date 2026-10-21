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

## Acceptance criteria

- **A1** — when the player's health reaches zero, the scene restarts without player input.
- **A2** — after the restart, the run begins at the first wave.
- **A3** — after the restart, the player has full health and the firearm equipped.
- **A4** — no enemy from the previous attempt exists after the restart.
- **A5** — no defeat window or confirmation stands between death and the restarted run.

## Constraints

- Any delay between death and restart is a tuning value.

## Out of scope

- A defeat screen or defeat message.
- Score summary, statistics, or rewards.
- Save/load or persistence of the failed run.
- Manual restart or a restart menu.
