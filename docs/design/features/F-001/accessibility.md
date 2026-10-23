# Accessibility

Feature F-001 — Player movement and third-person camera.

| screen | what it needs | label | role | keyboard path | assistive-technology behaviour |
|--------|---------------|-------|------|---------------|--------------------------------|
| S-001 — Arena gameplay view | a single focusable in-world view with direct keyboard movement and mouse view control; the player character and Arena Bounds distinguishable without relying on colour alone | "Arena gameplay view" | application (game view) | W, A, S, D move; mouse rotates the view; no tab traversal inside the view | the view is announced as one labelled region on entry; no per-frame movement is announced; the terminal state is announced by the defeat or victory surface that follows |

## Focus order

- S-001 — Arena gameplay view: focus enters the view; there are no tab stops inside it; movement keys and mouse act directly. Focus leaves only when the run ends and the defeat/restart or victory surface takes over.
