# Accessibility

Feature F-010 — Defeat and scene restart.

| screen | what it needs | label | role | keyboard path | assistive-technology behaviour |
|--------|---------------|-------|------|---------------|--------------------------------|
| S-003 — Defeat window | the defeat statement and the wave reached announced once when the window appears; the wave number in the announcement, not only on screen; no interactive element, so nothing to tab to | "Defeat window"; the statement region is labelled "Defeat"; the wave line is labelled "Wave reached" | alert (the window announces itself when it appears) | none — the screen has no control and takes no input | when the window appears, the statement and the wave reached are announced once as a single alert (for example "Defeat — wave 2 of 3"); it is not re-announced while it stays visible; the automatic restart is announced once when the fresh run begins ("New run, wave 1 of 3"); focus moves to `S-001` when it replaces the window |

## Focus order

- S-003 — Defeat window: nothing is focusable. Focus does not enter the window and is not trapped there; the screen lasts for the restart delay and is replaced without input.
- S-001 — Arena gameplay view: focus returns to the view when the fresh run begins, in the order F-001 and F-002 define — movement keys, mouse, Q, and the left mouse button act directly, with no tab traversal.
- S-002 — Victory window: unchanged from F-011.
