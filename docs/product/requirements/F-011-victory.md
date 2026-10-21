# F-011 — Victory after three waves

- **Priority:** P1
- **State:** specified
- **Backlog:** `docs/product/backlog.md`
- **Handoff:** `docs/analyst/product-inbox/F-011.md`, `docs/design/product-inbox/F-011.md`

## Problem

The brief defines the game's win state: complete three waves and defeat the boss in each, after which a "Victory" window appears. Without it the game has no ending and the run cannot be concluded.

## Requirements

- **R1** — the run is won when all three waves are completed and each wave's boss has been defeated.
- **R2** — when the run is won, a window titled or clearly stating "Victory" appears.
- **R3** — the window appears only after the third wave's boss is defeated.
- **R4** — after victory, gameplay stops: no further enemies enter and combat input no longer affects the run.
- **R5** — the victory window is readable and communicates that the run was won.

## Acceptance criteria

- **A1** — defeating the third wave's boss opens the Victory window.
- **A2** — the window's message states that the run has been won.
- **A3** — victory does not occur after the first or second wave, nor while any enemy of the third wave remains.
- **A4** — after the window opens, no new enemies enter the arena and the player cannot resume combat.
- **A5** — the player's death instead of the third boss's defeat does not open the Victory window.

## Out of scope

- Victory menu actions (restart, quit, continue) and score displays.
- Rewards, unlocks, or leaderboards.
- Victory animation, music, or camera work beyond the window.
