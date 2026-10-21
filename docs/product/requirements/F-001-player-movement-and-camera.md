# F-001 — Player movement and third-person camera

- **Priority:** P0
- **State:** specified
- **Backlog:** `docs/product/backlog.md`
- **Handoff:** `docs/analyst/product-inbox/F-001.md`, `docs/design/product-inbox/F-001.md`

## Problem

The arena is played from a third-person view. Without locomotion and view control the player cannot position, aim, or reach anything else in the game, so every other mechanic is unobservable.

## Requirements

- **R1** — the player moves horizontally with W, A, S, and D while the run is active.
- **R2** — movement is relative to the camera's current facing: W moves away from the camera, S toward it, A and D to its sides.
- **R3** — mouse movement rotates the camera view around the player.
- **R4** — the camera stays in a third-person position that keeps the player character visible.
- **R5** — the player is kept inside the arena bounds; moving into an edge never carries the player outside.
- **R6** — a dead player does not move in response to input.

## Acceptance criteria

- **A1** — holding W moves the player away from the camera; S toward it; A and D to the camera's left and right.
- **A2** — moving the mouse changes the view direction while the player remains visible.
- **A3** — the player can move and rotate the camera at the same time.
- **A4** — moving against an arena boundary stops at the boundary; the player never leaves the arena.
- **A5** — while the player is dead, movement input produces no movement.

## Constraints

- The arena is a closed space; no traversal outside it is required.

## Out of scope

- Jumping, crouching, sprinting, dodging, or climbing.
- Controller support and rebindable input.
- Camera collision with scenery, camera shake, or aim-assist.
- A first-person view.
